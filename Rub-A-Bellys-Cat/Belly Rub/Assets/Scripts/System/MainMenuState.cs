using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuState : GameState
{
    
    public static UnityEvent<MainMenuState> OnMainMenuStart = new UnityEvent<MainMenuState>();
    public static UnityEvent<MainMenuState> OnMainMenuEnd = new UnityEvent<MainMenuState>();

    private GameObject menu;
    public override void OnStateEnter(GameManager gameManager)
    {
        base.OnStateEnter(gameManager);
        menu = GameObject.Instantiate(Resources.Load<GameObject>("UI/Main Menu"));
        OnMainMenuStart?.Invoke(this);
    }
    public override void OnStateExit(GameManager target)
    {
        base.OnStateExit(target);
        if (menu != null)
        {
            GameObject.Destroy(menu);
        }
        OnMainMenuEnd?.Invoke(this);
        GameManager.Instance.RestartGame();
    }
}
