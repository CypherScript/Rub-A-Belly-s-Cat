using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void RestartGame()
    {
        AudioManager.Instance.PlayButtonSfx();
        GameManager.Instance.RestartGame();
    }

    public void ResumeGame()
    {
        GameManager.Instance.TogglePause();
        AudioManager.Instance.PlayButtonSfx();
    }

    public void PlayButtonSfx()
    {
        AudioManager.Instance.PlayButtonSfx();
    }
}
