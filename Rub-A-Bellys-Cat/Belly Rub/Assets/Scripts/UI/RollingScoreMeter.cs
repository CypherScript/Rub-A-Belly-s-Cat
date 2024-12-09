
using System;
using BellyRub;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RollingScoreMeter : MonoBehaviour
{
    [SerializeField] private RectTransform _levelUpFillTransform;
    [SerializeField] private TextMeshProUGUI _levelUpText;
    [SerializeField] private Animator _meterAnimator;
    [SerializeField] private Animator _panelAnimator;
    [SerializeField] private int score;
    [SerializeField] private List<ScorePanel> _scorePanels;

    private SessionState _sessionState;
    private bool _hasReachedMaxDifficulty = false;
    private int _difficultyIndex = 0;
    private int _minFillThreshold = 0;
    private int _maxFillThreshold = 0;
    
    private static readonly int IsDisplayScoreAccurate = Animator.StringToHash("IsDisplayScoreAccurate");

    [field: SerializeField] public int DisplayValue { get; set; }
    
    [SerializeField] private float maxSpeedMultiplier = 2f;  // This means the animation can speed up to 2 times its normal speed.
    [SerializeField] private float maximumExpectedDifference = 40; // Adjust this based on what you expect the largest score difference to be.
    private float defaultAnimatorSpeed;

    void Start()
    {
        if(GameManager.Instance.StateHandler.TryGetState(out TutorialState tutorialState))
        {
            _sessionState = tutorialState.SessionState;
            _minFillThreshold = 0;
            _maxFillThreshold = _sessionState.CatGlobalSetting.DifficultySettings[0].ScoreThreshold;
            _levelUpText.text = $"Lvl {_sessionState.difficultyLevel + 1}";
        }
        else
        {
            Debug.LogError("BELLY RUB ERROR: No session state was found for " + gameObject.name);
        }

        defaultAnimatorSpeed = _panelAnimator.speed; // Store the default speed at the start
        DisplayValue = _sessionState.score;
    }
    
    void Update()
    {
        score = _sessionState.score;

        if (!_hasReachedMaxDifficulty)
        {
            _levelUpFillTransform.localScale = Vector3.Lerp(_levelUpFillTransform.localScale, new Vector3(Mathf.InverseLerp(_minFillThreshold, _maxFillThreshold, score), 1, 1), .1f);
            if (score >= _maxFillThreshold)
            {
                _meterAnimator.SetTrigger("On Level Up");
                _difficultyIndex++;
                _minFillThreshold = _maxFillThreshold;
                if (_difficultyIndex < _sessionState.CatGlobalSetting.DifficultySettings.Count)
                {
                    _maxFillThreshold = _sessionState.CatGlobalSetting.DifficultySettings[_difficultyIndex].ScoreThreshold;
                    UpdateLevelUpText();
                }
                else
                {
                    _levelUpText.text = "LEVEL MAX!";
                    _levelUpFillTransform.localScale = Vector3.one;
                    _hasReachedMaxDifficulty = true;
                }
            }
        }
        HandleRollingScore();
    }

    private void UpdateLevelUpText()
    {
        StartCoroutine(UpdateLevelUpTextCo());
        IEnumerator UpdateLevelUpTextCo()
        {
            _levelUpText.text = "Level Up!";
            yield return new WaitForSeconds(1.25f);
            _levelUpText.text = $"Lvl {_sessionState.difficultyLevel + 1}";
        }
    }

    
    private void HandleRollingScore()
    {
        float difference = score - DisplayValue;

        switch (difference)
        {
            case 0:
                _panelAnimator.SetBool(IsDisplayScoreAccurate, true);
                return;
            case < 0:
            {
                for (int i = 0; i < _scorePanels.Count; i++)
                {
                    _scorePanels[i].SetDisplayedDigit(score, (int)Mathf.Pow(10, i));
                }
                _panelAnimator.SetBool(IsDisplayScoreAccurate, true);
                DisplayValue = score;
                return;
            }
        }
        _panelAnimator.speed = Mathf.Lerp(1, maxSpeedMultiplier, Mathf.InverseLerp(0, maximumExpectedDifference, difference)) * defaultAnimatorSpeed;
        _panelAnimator.SetBool(IsDisplayScoreAccurate, false);
    }
}
