using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;

public class PauseState : GameState
{
    private GameObject menu;

    public override void OnStateEnter(GameManager target)
    {
        base.OnStateEnter(target);
        if (SessionState.TryGetSessionState(out SessionState sessionState))
        {
            sessionState.IsOverrideTimeScale = true;
        }
        Time.timeScale = 0f;
        menu = GameObject.Instantiate(Resources.Load<GameObject>("UI/Pause Hud"));
    }

    public override void OnStateExit(GameManager target)
    {
        base.OnStateExit(target);
        if(SessionState.TryGetSessionState(out SessionState sessionState))
        {
            sessionState.IsOverrideTimeScale = false;
            Time.timeScale = sessionState.TimeScale;
        } else
        {
            Time.timeScale = 1f;
        }
        GameObject.Destroy(menu);
    }
}
