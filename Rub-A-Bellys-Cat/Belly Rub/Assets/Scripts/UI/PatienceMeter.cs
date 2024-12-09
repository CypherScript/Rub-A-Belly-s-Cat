using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PatienceMeter : MonoBehaviour
{
    [field: SerializeField] private float barFillSpeed;
    [field: SerializeField] private float barLagFillSpeed;
    [field: SerializeField] private Image barFill;
    [field: SerializeField] private Image barLagFill;
    [field: SerializeField] float _barLagTime = 1.0f;
    [field: SerializeField] private Animator animator;
   
    private SessionState sessionState;
    private float currentValue;
    private bool shouldDepleteBackgroundMeter = true;

    private void OnEnable()
    {
        SessionState.OnSessionStart.AddListener(OnSessionStart);
        SessionState.OnSessionEnd.AddListener(OnSessionEnd);
    }
    
    private void OnDisable()
    {
        SessionState.OnSessionStart.RemoveListener(OnSessionStart);
        SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
    }

    void OnSessionStart(SessionState session)
    {
        sessionState = session;
        sessionState.OnTakeDamage?.AddListener(OnAttacked);
    }
    
    void OnSessionEnd(SessionState session)
    {
        sessionState.OnTakeDamage?.RemoveListener(OnAttacked);
        sessionState = null;
    }
    
    void Update()
    {
        if (sessionState == null) return;

        currentValue = sessionState.patienceLevel;
        
        barFill.fillAmount = Mathf.LerpUnclamped(barFill.fillAmount, currentValue / 100, Time.unscaledDeltaTime * barFillSpeed);

        if (shouldDepleteBackgroundMeter)
        {
            barLagFill.fillAmount = Mathf.LerpUnclamped(barLagFill.fillAmount, currentValue / 100, Time.unscaledDeltaTime * barLagFillSpeed);
        }
    }

    public void OnAttacked()
    {
        Debug.Log("Hey I got Attacked!");
        shouldDepleteBackgroundMeter = false; // Stop depleting when attacked
        animator.SetTrigger("TakeHit");
        StartCoroutine(BackgroundMeterDelay());
    }

    IEnumerator BackgroundMeterDelay()
    {
        yield return new WaitForSecondsRealtime(_barLagTime); // Adjust this delay as needed
        shouldDepleteBackgroundMeter = true; // Start depleting after the delay
    }
}

