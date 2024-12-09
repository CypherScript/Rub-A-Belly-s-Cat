using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionScoreDisplay;

    public void Setup(SessionResults results)
    {
        sessionScoreDisplay.text = results.score.ToString();
    }

    public void RestartGame()
    {
        AudioManager.Instance.PlayButtonSfx();
        GameManager.Instance.RestartGame();
    }
}
