using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace BellyRub
{
    /// <summary>
    ///     Component that sends commands to activate and adjust the danger level of attacks, and punish/reward belly rubs
    /// </summary>
    public class CatController : MonoBehaviour
    {
        [field: SerializeField] private CatAttackPattern OverridePattern { get;  set; } = null;

        [field: SerializeField] private float MinDelayAfterSessionStart { get; set; } = 3f;

        [field: SerializeField] private float MinDelayAfterDifficultyChange { get; set; } = 1.5f;

        [field: SerializeField] private float MinDelayBetweenPatterns { get; set; } = 0.25f;

        [field: SerializeField] public List<CatAttackController> Attacks { get; private set; } = new();

        public CatDifficultySetting CurrentDifficulty =>
            _session != null ? DifficultySettings[_session.difficultyLevel] : null;

        public List<CatDifficultySetting> DifficultySettings => _session != null ? _session.CatGlobalSetting.DifficultySettings : null;

        private SessionState _session;
        private int _currentDifficultyLevel, _currentScoreThreshold;
        private CatAttackPattern _oneTimeOverridePattern = null;

        private void OnEnable()
        {
            SessionState.OnSessionStart.AddListener(OnSessionStart);
            TutorialState.OnTutorialStart.AddListener(OnTutorialStart);

            TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);
            SessionState.OnSessionEnd.AddListener(OnSessionEnd);
        }

        private void OnDisable()
        {
            SessionState.OnSessionStart.RemoveListener(OnSessionStart);
            TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);

            TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);
            SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
        }

        public void OnTutorialStart(TutorialState tutorialState)
        {
            _session = tutorialState.SessionState;
        }

        public void OnTutorialEnd(TutorialState tutorialState)
        {
            _session = null;
        }

        public void OnSessionStart(SessionState session)
        {
            _session = session;
            _currentDifficultyLevel = _session.difficultyLevel;
            _currentScoreThreshold = CurrentDifficulty.ScoreThreshold;
            ActivateNewAttacks(CurrentDifficulty.NewlyActivatedAttacks);
            StartCoroutine(OnSessionStartCo());

            IEnumerator OnSessionStartCo()
            {
                yield return new WaitForSeconds(MinDelayAfterSessionStart);
                Attack();
            }
        }

        public void OnSessionEnd(SessionState session)
        {
            _session = null;
            foreach (var CatAttack in Attacks)
            {
                CatAttack._isActivated = false;
                CatAttack._isReserved = false;
            }
            StopAllCoroutines();
        }

        [Button]
        private void IncreaseDifficulty()
        {
            if (_session == null)
            {
                Debug.LogError("No session found");
                return;
            }

            if(_session.difficultyLevel == DifficultySettings.Count - 1)
            {
                Debug.Log("DIFFICULTY LEVEL NOT INCREASED!");
                Debug.Log("ENABLING CAT INTENSITY!");
                _session.IsEnableCatIntensity = true;
                return;
            }

            _session.difficultyLevel++;
            _currentScoreThreshold = DifficultySettings[_session.difficultyLevel].ScoreThreshold;

            Debug.Log("DIFFICULTY LEVEL INCREASED TO: " + _session.difficultyLevel);

            if (DifficultySettings[_session.difficultyLevel].Theme != null)
            {
                _session.OnThemeChanged?.Invoke(DifficultySettings[_session.difficultyLevel].Theme);
            }
        }

        [Button]
        private void Attack()
        {
            StartCoroutine(AttackCo());

            IEnumerator AttackCo()
            {
                var difficultySetting = DifficultySettings[_session.difficultyLevel];

                if (difficultySetting == null)
                {
                    Debug.LogError("Couldn't find current difficulty setting!");
                    yield break;
                }

                // Select next attack pattern randomly based on frequency

                CatAttackPattern nextPattern = null;

                if (_oneTimeOverridePattern != null)
                {
                    nextPattern = _oneTimeOverridePattern;
                    _oneTimeOverridePattern = null;
                }
                else if (OverridePattern != null)
                {
                    nextPattern = OverridePattern;
                }
                else
                {
                    float totalWeight = difficultySetting.AttackPatterns.Sum(attack => attack.Frequency);
                    float randomValue = Random.Range(0, totalWeight);
                    float cumulativeWeight = 0;
                    foreach (var attack in difficultySetting.AttackPatterns)
                    {
                        cumulativeWeight += attack.Frequency;
                        if (randomValue < cumulativeWeight)
                        {
                            nextPattern = attack;
                            break;
                        }
                    }
                }


                // TODO: Throw error if no attack pattern was found
                Debug.Log("Selected Pattern: " + nextPattern.name);


                // Perform next attack pattern's steps in sequence
                bool isStepFailed = false;
                bool hasStartedNextPattern = false;
                Dictionary<CatAttackType, List<CatAttackController>> reservedAttackControllers = new();

                foreach (var step in nextPattern.Steps)
                {
                    switch (step)
                    {
                        case PrepareTargetStep prepareTargetStep:
                            // Add reserved attack controllers to dict
                            foreach(CatAttackQuantity attackQuantity in prepareTargetStep.Targets)
                            {
                                List<CatAttackController> randomValidTargets = Attacks.Where(a => a.IsValidToReserve(attackQuantity.AttackType)).OrderBy(a => Random.value).Take(attackQuantity.count).ToList();
                                //Debug.Log($"Reserving {randomValidTargets.Count} attack controllers of type {attackQuantity.AttackType}");
                                
                                if(randomValidTargets.Count >= attackQuantity.count)
                                {
                                    reservedAttackControllers[attackQuantity.AttackType] = randomValidTargets;
                                    foreach(var con in randomValidTargets)
                                    {
                                        con._isReserved = true;
                                    }
                                }
                                else
                                {
                                    //Debug.Log("Skipping attack pattern, failed to register enough valid targets.");
                                    isStepFailed = true;
                                    break;
                                }
                            }
                            break;


                        case DelayStep delayStep:
                            yield return new WaitForSeconds(delayStep.DelayTime);
                            break;

                        case WaitForAttacksCompletedStep waitForAttacksCompletedStep:
                            float startTime = Time.time;
                            if (waitForAttacksCompletedStep.IncludeNonReservedAttacks)
                                yield return new WaitUntil(() => Attacks.All(c => c._dangerLevel == 0) || Time.time > startTime + 5);
                            else
                                yield return new WaitUntil(() => reservedAttackControllers.SelectMany(kvp => kvp.Value).All(c => c._dangerLevel == 0) || Time.time > startTime + 5);
                            break;


                        case RaiseDangerStep raiseDangerStep:

                            foreach(CatAttackData attackData in raiseDangerStep.Targets)
                            {
                                switch(attackData)
                                {
                                    case CatAttackIndices attackIndices:
                                        foreach (int index in attackIndices.Indices)
                                        {
                                            if (index >= reservedAttackControllers[attackIndices.AttackType].Count) continue;

                                            CatAttackController selectedController = reservedAttackControllers[attackIndices.AttackType][index];
                                            selectedController.IncreaseDanger(attackIndices.AttackType);
                                            Debug.Log("Selected Controller: " + selectedController.name + ". Danger Increased.");
                                        }
                                        break;

                                    case CatAttackQuantity attackQuantity:
                                        List<CatAttackController> randomValidTargets = Attacks.Where(a => a.IsValidToReserve(attackQuantity.AttackType))
                                            .OrderBy(a => Random.value).Take(attackQuantity.count).ToList();

                                        foreach (var controller in randomValidTargets)
                                            controller.IncreaseDanger(attackQuantity.AttackType);

                                        break;
                                }
                            }
                            break;


                        case LowerDangerStep lowerDangerStep:
                            // TODO : Lower danger levels for the specified controllers (handle both CatAttackQuantity and CatAttackIndices cases)
                            foreach (CatAttackData attackData in lowerDangerStep.Targets)
                            {
                                switch (attackData)
                                {
                                    case CatAttackIndices attackIndices:
                                        foreach (int index in attackIndices.Indices)
                                        {
                                            if (index >= reservedAttackControllers[attackIndices.AttackType].Count) continue;

                                            CatAttackController selectedController = reservedAttackControllers[attackIndices.AttackType][index];
                                            selectedController.DecreaseDanger(attackIndices.AttackType);
                                            Debug.Log("Selected Controller: " + selectedController.name + ". Danger Decreased.");
                                        }
                                        break;

                                    case CatAttackQuantity attackQuantity:
                                        List<CatAttackController> randomValidTargets = Attacks.Where(a => a.IsValidToReserve(attackQuantity.AttackType))
                                            .OrderBy(a => Random.value).Take(attackQuantity.count).ToList();

                                        foreach (var controller in randomValidTargets)
                                            controller.DecreaseDanger(attackQuantity.AttackType);

                                        break;
                                }
                            }
                            break;


                        case StartNextPatternStep startNextPatternStep:
                            GetReadyForNextAttack();
                            hasStartedNextPattern = true;
                            break;


                        default:
                            break;
                    }

                    if (isStepFailed)
                    {
                        break;
                    }
                }

                // Release reserved attack controllers
                foreach (var kvp in reservedAttackControllers)
                {
                    foreach(var con in kvp.Value)
                    {
                        con._isReserved = false;
                    }
                }

                if (!hasStartedNextPattern)
                {
                    GetReadyForNextAttack();
                }
            }
        }

        // Selects the next pattern after a certain delay time.
        private void GetReadyForNextAttack()
        {
            if (_session.score >= _currentScoreThreshold)
            {
                IncreaseDifficulty();
            }

            StartCoroutine(StartNextPatternCo());

            IEnumerator StartNextPatternCo()
            {
                while (_currentDifficultyLevel < _session.difficultyLevel)
                {
                    _currentDifficultyLevel++;
                    ActivateNewAttacks(DifficultySettings[_currentDifficultyLevel].NewlyActivatedAttacks);
                    OverrideNextPattern(DifficultySettings[_currentDifficultyLevel].FirstAttackPattern);
                    yield return new WaitForSeconds(MinDelayAfterDifficultyChange);
                }
                yield return new WaitForSeconds(MinDelayBetweenPatterns);
                Attack();
            }
        }

        private void ActivateNewAttacks(List<CatAttackSource> newAttacks)
        {
            for (var i = 0; i < newAttacks.Count; i++)
            {
                var unactiveController = Attacks.Where(a => !a._isActivated && a.IsValidToActivate(newAttacks[i])).Take(1);

                foreach (var controller  in unactiveController)
                {
                    controller.Activate();
                }

            }
        }

        private void OverrideNextPattern(CatAttackPattern attackPattern)
        {
            _oneTimeOverridePattern = attackPattern;
        }
    }
}