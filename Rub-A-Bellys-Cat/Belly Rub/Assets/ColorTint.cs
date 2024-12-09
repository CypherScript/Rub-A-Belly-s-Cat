using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTint : MonoBehaviour
{
    [SerializeField] private Volume effect;
    [SerializeField] private float speed;

    private SessionState _sessionState;
    private bool locked;
    
    private void Awake()
    {
        ToggleEffect(false);
    }

    protected virtual void OnEnable()
    {
        TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
        SessionState.OnSessionStart.AddListener(OnSessionStart);
    }

    protected virtual void OnDisable()
    {
        TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
        SessionState.OnSessionStart.RemoveListener(OnSessionStart);
    }
    
    protected virtual void OnTutorialStart(TutorialState tutorialState)
    {
        StopAllCoroutines();
        locked = false;
        _sessionState = null;
        if (effect.profile.TryGet(out ColorAdjustments tint))
        {
            tint.saturation.value = 0;
        }
    }

    protected virtual void OnSessionStart(SessionState session)
    {
        _sessionState = session;
    }
    
    private void Update()
    {
        if (locked || _sessionState == null) return;
        if(_sessionState.patienceLevel <= 0)
            ToggleEffect(true);
    }

    

    private void ToggleEffect(bool state)
    {
        if (this == null) return;
        locked = true;
        float blurAmount = state ? -100 : 0;
        StopAllCoroutines();
        StartCoroutine(FadeEffect(blurAmount));
    }

    IEnumerator FadeEffect(float targetValue)
    {
        if (effect.profile.TryGet(out ColorAdjustments tint))
        {
            while (!Mathf.Approximately(tint.saturation.value,targetValue))
            {
                tint.saturation.value = Mathf.Lerp(tint.saturation.value, targetValue, Time.unscaledDeltaTime * speed);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
