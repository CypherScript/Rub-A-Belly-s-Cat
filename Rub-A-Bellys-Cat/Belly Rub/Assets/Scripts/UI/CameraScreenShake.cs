using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;

public class CameraScreenShake : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private SessionState sessionState;
    public static bool CanShakeScreen = true;
    
    private void Start()
    {
        SessionState.OnSessionStart.AddListener(delegate
        {
            TryGetSession();
        });   
    }

    void TryGetSession()
    {
        if (GameManager.Instance.StateHandler.TryGetState(out SessionState session))
        {
            sessionState = session;
            sessionState.OnTakeDamage.AddListener(delegate
            {
                if(CanShakeScreen)
                    animator.SetTrigger("TakeHit");
            });
            Lightning.OnLightningStrike.AddListener(delegate()
            {
                if(CanShakeScreen)
                    animator.SetTrigger("Lightning");
            });
        }
        else
        {
            Debug.LogError("BELLY RUB ERROR: No session state was found for " + gameObject.name);
        }
    }
}
