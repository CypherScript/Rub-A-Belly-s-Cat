using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.Events;

public class HitNotifier : MonoBehaviour
{
    [field: SerializeField] private CatAttackType ExpectedType;
    [field: SerializeField] private UnityEvent OnHit;
    private SessionState sessionState;
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
        sessionState.OnPlayDamageVfx?.AddListener(TryRunEvent);
    }
    
    void OnSessionEnd(SessionState session)
    {
        sessionState.OnPlayDamageVfx?.RemoveListener(TryRunEvent);
       
    }

    private void TryRunEvent(CatAttackType damageSource)
    {
        if (damageSource == ExpectedType)
        {
            OnHit.Invoke();
        }
    }
}
