using BellyRub;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zoodle;
using SessionState = BellyRub.SessionState;

public class TutorialState : GameState
{
    public static UnityEvent<TutorialState> OnTutorialStart = new UnityEvent<TutorialState>();
    public static UnityEvent<TutorialState> OnTutorialEnd = new UnityEvent<TutorialState>();

    public SessionState SessionState { get; private set; }

    public TutorialState() { }
    
    private GameObject menu;
    private AudioManager audioManager;

    public override void OnStateEnter(GameManager gameManager)
    {
        base.OnStateEnter(gameManager);

        SessionState = new SessionState(CatGlobalSetting.Instance);

        Debug.Log("ENTER TUTORIAL STATE!");

        OnTutorialStart?.Invoke(this);
    }

    public override void OnStateExit(GameManager gameManager)
    {
        base.OnStateExit(gameManager);

        Debug.Log("EXIT TUTORIAL STATE!");

        OnTutorialEnd?.Invoke(this);
    }
}
