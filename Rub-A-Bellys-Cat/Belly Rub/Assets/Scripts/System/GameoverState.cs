using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.Events;
using Zoodle;

public class GameoverState : GameState
{
    public static UnityEvent<GameoverState> OnGameOverStart = new UnityEvent<GameoverState>();
    public static UnityEvent<GameoverState> OnGameOverEnd = new UnityEvent<GameoverState>();

    private GameObject menu;
    private SessionResults sessionResults;

    public GameoverState(SessionResults results)
    {
        sessionResults = results;
    }
    
    public override void OnStateEnter(GameManager target)
    {
        base.OnStateEnter(target);
        menu = GameObject.Instantiate(Resources.Load<GameObject>("UI/Gameover Hud"));
        menu.GetComponent<GameoverMenu>().Setup(sessionResults);

        OnGameOverStart.Invoke(this);
    }

    public override void OnStateExit(GameManager target)
    {
        base.OnStateExit(target);
        GameObject.Destroy(menu);

        OnGameOverEnd.Invoke(this);
    }
}
