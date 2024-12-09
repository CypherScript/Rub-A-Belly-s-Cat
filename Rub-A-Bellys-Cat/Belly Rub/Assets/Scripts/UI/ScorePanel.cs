using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ScorePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _currentDigitText;
    [SerializeField] private TMP_Text _nextDigitText;
    [SerializeField] private RollingScoreMeter _rollingScoreMeter;
    [SerializeField] private Animator _nextDigitAnimator;
    
    [ShowInInspector]
    private int _displayedDigit = 0;
    private static readonly int IncrementDigit = Animator.StringToHash("IncrementDigit");

    private void SetScoreText(int currentVal, int nextVal)
    {
        _currentDigitText.text = currentVal.ToString();
        _nextDigitText.text = nextVal.ToString();
    }
    
    private int GetNextNumber()
    {
        int nextNumber;
        
        if (_displayedDigit == 9)
        {
            nextNumber = 0;
        }
        else
        {
            nextNumber = _displayedDigit + 1;
        }

        return nextNumber;
    }

    public void SetDisplayedDigit(int score, int divisor)
    {
        _displayedDigit = (int)(score / divisor) % 10;
        SetScoreText(_displayedDigit, GetNextNumber());
    }
    
    private void IncrementDisplayedDigitText()
    {
        _displayedDigit = GetNextNumber();

        SetScoreText(_displayedDigit, GetNextNumber());
        CheckShouldRollNextDigit();
    }

    public void CheckShouldRollNextDigit()
    {
        if (_nextDigitAnimator != null && _displayedDigit == 0)
        {
            _nextDigitAnimator.SetTrigger(IncrementDigit);
        }
    }

    public void OnRollAnimationComplete()
    {
        IncrementDisplayedDigitText();
    }

    public void OnLsdRollAnimationComplete()
    {
        IncrementDisplayedDigitText();
        _rollingScoreMeter.DisplayValue++;
    }
}
