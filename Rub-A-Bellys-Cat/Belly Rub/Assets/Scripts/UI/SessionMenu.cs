using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;

public class SessionMenu : MonoBehaviour
{
    public void TogglePause()
    {
        GameManager.Instance.TogglePause();
        AudioManager.Instance.PlayButtonSfx();
    }
}
