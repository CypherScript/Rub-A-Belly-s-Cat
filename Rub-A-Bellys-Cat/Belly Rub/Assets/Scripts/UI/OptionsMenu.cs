using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle, sfxToggle, screenShakeToggle;
    [SerializeField] private Slider musicVolumeSlider, sfxVolumeSlider;

    private void OnEnable()
    {
        musicToggle.SetIsOnWithoutNotify(!AudioManager.IsMusicMuted);
        musicVolumeSlider.SetValueWithoutNotify(AudioManager.MusicVolume);
        
        sfxToggle.SetIsOnWithoutNotify(!AudioManager.IsSoundEffectsMuted);
        sfxVolumeSlider.SetValueWithoutNotify(AudioManager.SfxVolume);
        
        screenShakeToggle.SetIsOnWithoutNotify(CameraScreenShake.CanShakeScreen);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.OnSetMusicVolume.Invoke(value);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.OnSetSfxVolume.Invoke(value);
    }
    
    public void ToggleMusic()
    {
        AudioManager.OnToggleMusic.Invoke();
    }

    public void ToggleSoundEffects()
    {
        AudioManager.OnToggleSoundsEffects.Invoke();
    }

    public void ToggleScreenShake()
    {
        CameraScreenShake.CanShakeScreen = !CameraScreenShake.CanShakeScreen;
    }
}
