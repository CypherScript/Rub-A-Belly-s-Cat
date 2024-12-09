using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [field: SerializeField] private TextMeshProUGUI scoreText;
    private SessionState sessionState;
    private void Start()
    {
        if(GameManager.Instance.StateHandler.TryGetState(out TutorialState tutorialState))
        {
            sessionState = tutorialState.SessionState;
        }
        else
        {
            Debug.LogError("BELLY RUB ERROR: No session state was found for " + gameObject.name);
        }
    }

    private void Update()
    {
        // todo: may want string formatting
        // todo: maybe move to an event?
        if(sessionState != null)
            scoreText.text = sessionState.score.ToString();
    }
}
