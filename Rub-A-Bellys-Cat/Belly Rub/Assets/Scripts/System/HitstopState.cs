using BellyRub;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zoodle;

public class HitstopState : GameState
{
    public float TimeScaleFreezeValue;
    public float TimeScaleIncreaseRate;
    public float FreezeDuration;

    private float _timeElapsed = 0;
    private float _targetTimeScale = 1f;

    public HitstopState(float timeScaleIncreaseRate, float freezeDuration,  float timeScaleFreeze = 0f)
    {
        TimeScaleFreezeValue = timeScaleFreeze;
        TimeScaleIncreaseRate = timeScaleIncreaseRate;
        FreezeDuration = freezeDuration;
    }

    public override void OnStateEnter(GameManager gameManager)
    {
        base.OnStateEnter(gameManager);
        Debug.Log("CALLED HITSTOP ENTER STATE");
        gameManager.OnUpdateAction.AddModifier(OnUpdateAction, 0);

        if (SessionState.TryGetSessionState(out SessionState sessionState))
        {
            sessionState.IsOverrideTimeScale = true;
            _targetTimeScale = sessionState.TimeScale;
        }
        Time.timeScale = TimeScaleFreezeValue;
    }

    public override void OnStateExit(GameManager gameManager)
    {
        base.OnStateExit(gameManager);
        Debug.Log("CALLED HITSTOP EXIT STATE");
        gameManager.OnUpdateAction.RemoveModifier(OnUpdateAction);

        if (SessionState.TryGetSessionState(out SessionState sessionState))
        {
            sessionState.IsOverrideTimeScale = false;
            Time.timeScale = _targetTimeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private ActionResult OnUpdateAction(GameManager gameManager)
    {
        _timeElapsed += Time.unscaledDeltaTime;
        float targetTimeScale = 1;

        if(SessionState.TryGetSessionState(out SessionState sessionState))
        {
            targetTimeScale = sessionState.TimeScale;
        }

        if (_timeElapsed > FreezeDuration)
        {
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, targetTimeScale, TimeScaleIncreaseRate);
        }

        if (Time.timeScale >= targetTimeScale)
        {
            GameManager.Instance.StateHandler.ExitState(this);
        }

        return ActionResult.Continue;
    }
}
