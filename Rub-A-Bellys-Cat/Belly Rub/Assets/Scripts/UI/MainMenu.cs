using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void RestartGame()
    {
        if(GameManager.Instance.StateHandler.TryGetState(out MainMenuState mainMenuState))
        {
            GameManager.Instance.StateHandler.ExitState(mainMenuState);
        }
        AudioManager.Instance.PlayButtonSfx();
    }
}
