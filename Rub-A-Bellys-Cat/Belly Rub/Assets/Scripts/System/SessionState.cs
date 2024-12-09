using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zoodle;

namespace BellyRub
{
    /// <summary>
    /// GameState that handles values and progression for a gameplay session.
    /// </summary>
    public class SessionState : GameState
    {
        public static UnityEvent<SessionState> OnSessionStart = new UnityEvent<SessionState>();
        public static UnityEvent<SessionState> OnSessionEnd = new UnityEvent<SessionState>();
       

        public static bool TryGetSessionState(out SessionState sessionState) =>
            GameManager.Instance.StateHandler.TryGetState(out sessionState);

        public int score = 0;
        public float patienceLevel = 100f;
        public int difficultyLevel = 0;

        public List<CatAttackController> activeCounterAttacks = new List<CatAttackController>();
        public float timeLastRubbed = float.MinValue;
        public float timeLastAttacked = float.MinValue;
        public  UnityEvent OnTakeDamage = new();
        public  UnityEvent<CatAttackType> OnPlayDamageVfx = new();
        public  UnityEvent OnGlintShown = new UnityEvent();
        public  UnityEvent OnClawExtended = new UnityEvent();
        public  UnityEvent OnClawRetracted = new UnityEvent();
        public  UnityEvent OnBellyRubbed = new UnityEvent();
        public  UnityEvent OnStopRub = new UnityEvent();
        public  UnityEvent OnHairballFired = new UnityEvent();
        public UnityEvent<bool> OnTailStatic = new UnityEvent<bool>();
        public UnityEvent OnLightningStrike = new UnityEvent();
        public  UnityEvent<AudioClip> OnThemeChanged = new UnityEvent<AudioClip>();
        public UnityEvent<bool> OnLaserFired = new UnityEvent<bool>();
        public UnityEvent<int> OnHeartSpawned = new UnityEvent<int>();
        public UnityEvent<bool> OnfleshyGrowth = new UnityEvent<bool>();

        public bool IsTouchAllowed => Time.time - timeLastAttacked >= CatGlobalSetting.DamagedLockOutTime;
        public float TimeScale => IsEnableCatIntensity ? 1f + (score - 900) /100f * CatGlobalSetting.CatIntensityScalar : 1f;
        public bool IsEnableCatIntensity { get; set; } = false;
        public bool IsOverrideTimeScale { get; set; } = false;

        public CatGlobalSetting CatGlobalSetting { get; private set; } = null;
        private bool isRubSFXPlaying = false;
        
        private GameObject menu;
        private SessionResults results;
        private LayerMask _projectileLayer = LayerMask.NameToLayer("Projectile");
        private LayerMask _bulletHellLayer = LayerMask.NameToLayer("BulletHell");

        // Multiplier-related variables
        public float scoreMultiplier = 1f; // Current multiplier applied to the score.
        private float timeSinceLastRubIncrease = 0f; // Time elapsed since the last increment in score multiplier.
        private float timeSinceStoppedRubbing = 0f; // Time elapsed since the player stopped rubbing the belly.
        
        const float MAX_MULTIPLIER = 2f;  // Maximum allowed multiplier value.
        private float GRACE_PERIOD = 3f; // Time allowed before resetting the multiplier after inactivity.
        private int multiplierIncreasePeriod = 5; // Interval at which the multiplier increases while rubbing.
        private bool isSessionEndByDamage = false;
        
        public SessionState(CatGlobalSetting globalSetting = null)
        {
            CatGlobalSetting = globalSetting;
        }

        public override void OnStateEnter(GameManager gameManager)
        {
            base.OnStateEnter(gameManager);
            
            patienceLevel = 100f;
            results = new SessionResults();
            // Register OnUpdateAction with GameManager
            gameManager.OnUpdateAction.AddModifier(OnUpdateAction, 0);

            //menu = GameObject.Instantiate(Resources.Load<GameObject>("UI/Session Hud"));

            // Send event
            OnSessionStart.Invoke(this);
        }

        public override void OnStateExit(GameManager gameManager)
        {
            base.OnStateExit(gameManager);

            // Deregister OnUpdateAction
            gameManager.OnUpdateAction.RemoveModifier(OnUpdateAction);
            results.score = score;

            if (isSessionEndByDamage)
            {
                gameManager.StartCoroutine(WaitForHitstop());
                IEnumerator WaitForHitstop()
                {
                    while (gameManager.StateHandler.HasState<HitstopState>())
                    {
                        Debug.Log("WAITING FOR HITSTOP!");
                        yield return null;
                    }
                    gameManager.EndGame(results);
                }
            }
            GameObject.Destroy(menu);
            OnSessionEnd.Invoke(this);
        }

        // Registers a CatAttackController as being ready to counterattack if the player is touching the cat's belly.
        public void RegisterCounterAttack(CatAttackController counterAttackingController)
        {
            activeCounterAttacks.Add(counterAttackingController);
        }

        // Deregisters a CatAttackController when it is no longer counterattacking.
        public void DeregisterCounterAttack(CatAttackController counterAttackingController)
        {
            activeCounterAttacks.Remove(counterAttackingController);
        }

        // Triggers each frame while rubbing the cat's belly, even if not rubbing.
        public void OnTouchingBelly()
        {
            // If any counter attacking controllers are found, trigger the first one's attack
            CatAttackController counterAttackingController = activeCounterAttacks.FirstOrDefault();
            if (counterAttackingController != null)
            {
                counterAttackingController.CounterAttack();
            }
        }

        // Triggers at fixed intervals while rubbing the cat's belly.
        public void OnRubBellyTick(float intensity)
        {
            timeLastRubbed = Time.time;

            if (GameManager.Instance.StateHandler.TryGetState(out TutorialState state))
            {
                GameManager.Instance.StateHandler.ExitState(state);
                GameManager.Instance.StartGame(state.SessionState);
            }

            if (!isRubSFXPlaying)
            {
                isRubSFXPlaying = true;
            }

            OnBellyRubbed?.Invoke();

            // Increase patience based on the intensity of rub and global settings
            float patienceToAdd = Mathf.Lerp(CatGlobalSetting.MinPatienceRewardPerTick,
                CatGlobalSetting.MaxPatienceRewardPerTick, intensity);

            patienceLevel += patienceToAdd;

            // Ensure patience doesn't exceed its max value
            patienceLevel = Mathf.Min(patienceLevel, 100f);

            // TODO: Increase score and patience based on intensity of rub + global setting
            // Increase score based on multiplier
            int scoreToAdd = Mathf.FloorToInt(Mathf.Lerp(CatGlobalSetting.MinPointRewardPerTick,
                CatGlobalSetting.MaxPointRewardPerTick, intensity) * scoreMultiplier);
            OnHeartSpawned.Invoke(scoreToAdd/2);
            score += scoreToAdd;
            
        }

        public void TakeDamage(CatAttackType damageSource = CatAttackType.PawClaw)
        {
            Debug.Log("Took damage!");

            timeLastAttacked = Time.time;

            if (isRubSFXPlaying)
            {
                isRubSFXPlaying = false;
            }

            patienceLevel -= CatGlobalSetting.PatienceDamagePerHit;
            
            OnPlayDamageVfx.Invoke(damageSource);
            OnTakeDamage?.Invoke();

            ResetMultiplierDueToAttack();
        }

        private ActionResult OnUpdateAction(GameManager gameManager)
        {
            if (!IsOverrideTimeScale)
            {
                Time.timeScale = TimeScale;
            }

            // Increase the multiplier for every second (or other interval) the player is rubbing. You can adjust this value.
            if (timeSinceLastRubIncrease >= multiplierIncreasePeriod)
            {
                if (scoreMultiplier < MAX_MULTIPLIER)
                {
                    scoreMultiplier += .1f;
                    Debug.Log( "Multiplier increased to: " + scoreMultiplier);
                }
                else
                {
                    Debug.Log("Multiplier at cap: " + scoreMultiplier);
                }
                   
                
                timeSinceLastRubIncrease = 0f;
            }
            
            //Debug.Log("New Patience Level: " + patienceLevel);
            // TODO: Reduce patience if too long since timeLastRubbed
            if (patienceLevel <= 0)
            {
                isSessionEndByDamage = true;
                gameManager.StateHandler.EnterState(new HitstopState(0.005f, 1f));
                gameManager.StateHandler.ExitState(this);
                return ActionResult.Finish;
            }

            if (IsTouchAllowed && Input.GetMouseButton((0)) && !gameManager.StateHandler.TryGetState(out PauseState pauseState))
            {
                if(TryRaycastProjectile(Input.mousePosition, out Projectile detectedProjectile))
                {
                    detectedProjectile.OnHit(this);
                }
                else if(TryRaycastBulletHell(Input.mousePosition, out BulletHellProjectile detectedBullet))
                {
                    detectedBullet.OnHit(this);
                }
            }
            
            // Check if enough time has passed since the last rub
            if (Time.time - timeLastRubbed > CatGlobalSetting.PatienceDrainDelay && IsTouchAllowed)
            {
                OnStopRub?.Invoke();
                
                // Deplete patience at the set rate
                patienceLevel -= CatGlobalSetting.PatienceDrainPerSecond * Time.deltaTime;

                // Ensure patience doesn't fall below zero
                patienceLevel = Mathf.Max(patienceLevel, 0f);
            }

            if (timeLastRubbed > CatGlobalSetting.TickLength && isRubSFXPlaying)
            {
                isRubSFXPlaying = false;
            }

            return ActionResult.Continue;
        }
        
        // Checks if a collider on the target layer is touched, and outputs position
        private bool TryRaycastProjectile(Vector2 position, out Projectile detectedProjectile)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero, _projectileLayer);

            if (hit.collider != null)
            {
                detectedProjectile = hit.collider.GetComponent<Projectile>();

                if (detectedProjectile == null)
                    detectedProjectile = hit.collider.GetComponentInParent<Projectile>();

                if (detectedProjectile == null) return false;

                return true;
            }
            else
            {
                detectedProjectile = null;
                return false;
            }
        }

        // Checks if a collider on the target layer is touched, and outputs position
        private bool TryRaycastBulletHell(Vector2 position, out BulletHellProjectile detectedProjectile)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero, _bulletHellLayer);

            if (hit.collider != null)
            {
                detectedProjectile = hit.collider.GetComponent<BulletHellProjectile>();

                if (detectedProjectile == null) return false;

                return true;
            }
            else
            {
                detectedProjectile = null;
                return false;
            }
        }

        //Multiplier System
        public void UpdateMultiplierOnRubbing()
        {
            timeSinceLastRubIncrease += Time.deltaTime;
            timeSinceStoppedRubbing = 0f;
        }

        public void ResetMultiplierOnInactivity()
        {
            timeSinceStoppedRubbing += Time.deltaTime;
            if (timeSinceStoppedRubbing > GRACE_PERIOD)
            {
                scoreMultiplier = 1;
                timeSinceLastRubIncrease = 0f;
            }
        }
        
        public void ResetMultiplierDueToAttack()
        {
            scoreMultiplier = 1;
            timeSinceLastRubIncrease = 0f;
            timeSinceStoppedRubbing = 0;
            Debug.Log("Multiplier reset due to attack!");
        }
    }
  
}
[Serializable]
public class SessionResults
{
    public int score;
}