using BellyRub;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup soundEffectGroup, musicGroup;
    [SerializeField] private float _rubFadeRate;
    [SerializeField] private AudioSource _rubAudioSource = null;
    [SerializeField] private AudioSource _bgmAudioSource = null;
    [SerializeField] private MMSoundManager _soundManager;
    private MMF_Player _sfxProfile;
    private MMF_Sound _rub, _scratch, _clawExtended, _clawRetracted, _glint, _hairball, _laser, _thirdEyeLaser, _static, _lightning ,_button, _fleshyGrowth ;
    private MMF_Sound _mainMenuTheme, _mainTheme, _hellTheme, _gameOverTheme;
    private SessionState _sessionState = null;
    private bool isRubbing = false;

    public static AudioManager Instance;
    public static readonly UnityEvent OnToggleMusic = new ();
    public static readonly UnityEvent<float> OnSetMusicVolume = new();
    public static readonly UnityEvent OnToggleSoundsEffects = new();
    public static readonly UnityEvent<float> OnSetSfxVolume = new();
    public static float MusicVolume = 1f, SfxVolume = 1f;
    public static bool IsMusicMuted, IsSoundEffectsMuted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        
        InitializeManager();
    }

    private void Update()
    {
        Rub();
        HandleBgmLoop();
    }

    private void OnEnable()
    {
        SessionState.OnSessionStart.AddListener(OnSessionStart);
        SessionState.OnSessionEnd.AddListener(OnSessionEnd);

        MainMenuState.OnMainMenuStart.AddListener(OnMainMenuStart);
        MainMenuState.OnMainMenuEnd.AddListener(OnMainMenuEnd);

        TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);

        GameoverState.OnGameOverStart.AddListener(OnGameOverStart);
        GameoverState.OnGameOverEnd.AddListener(OnGameOverEnd);
        
        OnToggleMusic.AddListener(ToggleMusic);
        OnSetMusicVolume.AddListener(SetMusicVolume);
        OnToggleSoundsEffects.AddListener(ToggleSoundEffects);
        OnSetSfxVolume.AddListener(SetSfxVolume);

    }

    private void OnDisable()
    {
        SessionState.OnSessionStart.RemoveListener(OnSessionStart);
        SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);

        MainMenuState.OnMainMenuStart.RemoveListener(OnMainMenuStart);
        MainMenuState.OnMainMenuEnd.RemoveListener(OnMainMenuEnd);

        TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);

        GameoverState.OnGameOverStart.RemoveListener(OnGameOverStart);
        GameoverState.OnGameOverEnd.RemoveListener(OnGameOverEnd);
        
        OnToggleMusic.RemoveListener(ToggleMusic);
        OnSetMusicVolume.RemoveListener(SetMusicVolume);
        OnToggleSoundsEffects.RemoveListener(ToggleSoundEffects);
        OnSetSfxVolume.RemoveListener(SetSfxVolume);

    }

    public void OnMainMenuStart(MainMenuState tutorialState)
    {
        PlayTheme(_mainMenuTheme.Sfx);
    }

    public void OnMainMenuEnd(MainMenuState mainMenuState)
    {
        _bgmAudioSource.Stop();
    }

    public void OnTutorialStart(TutorialState tutorialState)
    {

    }

    public void OnTutorialEnd(TutorialState tutorialState)
    {

    }

    public void OnSessionStart(SessionState session)
    {
        PlayTheme(_mainTheme.Sfx);
        _sessionState = session;
        RegisterListeners();
    }

    public void OnSessionEnd(SessionState session)
    {
        _bgmAudioSource.Stop();
        DeregisterListeners();
        _sessionState = null;
    }

    public void OnGameOverStart(GameoverState gameoverState)
    {
        PlayTheme(_gameOverTheme.Sfx);
    }

    public void OnGameOverEnd(GameoverState gameoverState)
    {
        _bgmAudioSource.Stop();
    }

    void RegisterListeners()
    {
        _sessionState.OnTakeDamage.AddListener(delegate { PlayScratchSfx(); });
        _sessionState.OnGlintShown.AddListener(delegate { PlayGlintSfx(); });
        _sessionState.OnClawExtended.AddListener(delegate { PlayClawExtendedSfx(); });
        _sessionState.OnClawRetracted.AddListener(delegate { PlayClawRetractedSfx(); });
        _sessionState.OnBellyRubbed.AddListener(delegate { PlayRubFadeInSfx(); });
        _sessionState.OnStopRub.AddListener(delegate { PlayRubFadeOutSfx(); });
        _sessionState.OnHairballFired.AddListener(delegate { PlayHairballSfx(); });
        _sessionState.OnThemeChanged.AddListener(PlayTheme);
        _sessionState.OnLaserFired.AddListener(PlayLaserSfx);
        _sessionState.OnTailStatic.AddListener(PlayStaticSfx);
        _sessionState.OnLightningStrike.AddListener(PlayLightningSfx);
        _sessionState.OnfleshyGrowth.AddListener(PlayFleshyGrowth);
    }

    void DeregisterListeners()
    {
        _sessionState.OnTakeDamage.RemoveListener(delegate { PlayScratchSfx(); });
        _sessionState.OnGlintShown.RemoveListener(delegate { PlayGlintSfx(); });
        _sessionState.OnClawExtended.RemoveListener(delegate { PlayClawExtendedSfx(); });
        _sessionState.OnClawRetracted.RemoveListener(delegate { PlayClawRetractedSfx(); });
        _sessionState.OnBellyRubbed.RemoveListener(delegate { PlayRubFadeInSfx(); });
        _sessionState.OnStopRub.RemoveListener(delegate { PlayRubFadeOutSfx(); });
        _sessionState.OnHairballFired.RemoveListener(delegate { PlayHairballSfx(); });
        _sessionState.OnThemeChanged.RemoveListener(PlayTheme);
        _sessionState.OnLaserFired.RemoveListener(PlayLaserSfx);
        _sessionState.OnTailStatic.RemoveListener(PlayStaticSfx);
        _sessionState.OnLightningStrike.RemoveListener(PlayLightningSfx);
        _sessionState.OnfleshyGrowth.RemoveListener(PlayFleshyGrowth);
    }

    private void InitializeManager()
    {
        _sfxProfile = GetComponent<MMF_Player>();
        _sfxProfile.Initialization();

        _rub = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Rub");
        _scratch = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Scratch");
        _clawExtended = _sfxProfile.GetFeedbackOfType<MMF_Sound>("ClawExtended");
        _clawRetracted = _sfxProfile.GetFeedbackOfType<MMF_Sound>("ClawRetracted");
        _glint = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Glint");
        _hairball = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Hairball");
        _laser = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Laser");
        _fleshyGrowth = _sfxProfile.GetFeedbackOfType<MMF_Sound>("FleshyGrowth");
        _thirdEyeLaser = _sfxProfile.GetFeedbackOfType<MMF_Sound>("ThirdEyeLaser");
        _static = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Static");
        _lightning = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Lightning");
        _button = _sfxProfile.GetFeedbackOfType<MMF_Sound>("Button");

        _mainMenuTheme = _sfxProfile.GetFeedbackOfType<MMF_Sound>("MainMenuTheme");
        _mainTheme = _sfxProfile.GetFeedbackOfType<MMF_Sound>("MainTheme");
        _hellTheme = _sfxProfile.GetFeedbackOfType<MMF_Sound>("HellTheme");
        _gameOverTheme = _sfxProfile.GetFeedbackOfType<MMF_Sound>("GameOverTheme");

        _rubAudioSource.loop = true;
        _rubAudioSource.clip = _rub.Sfx;
        _rubAudioSource.volume = 0;

        _bgmAudioSource.loop = true;
    }

    private void ToggleMusic()
    {
        IsMusicMuted = !IsMusicMuted;
        SetMusicVolume(MusicVolume);
    }

    private void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp(value, 0.0001f, 1f);
        musicGroup.audioMixer.SetFloat("MusicVolume", IsMusicMuted ? -80f : Mathf.Lerp(-80f, 0, value));
    }

    private void ToggleSoundEffects()
    {
        IsSoundEffectsMuted = !IsSoundEffectsMuted;
        SetSfxVolume(SfxVolume);
    }
    
    private void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp(value, 0.0001f, 1f);
        soundEffectGroup.audioMixer.SetFloat("SfxVolume", IsSoundEffectsMuted ? -80f : Mathf.Lerp(-80f, 0, value));
    }
    
    public void PlayRubFadeInSfx()
    {
        if(!_rubAudioSource.isPlaying)
            _rubAudioSource.Play();

        isRubbing = true;
    }

    public void PlayRubFadeOutSfx()
    {
        isRubbing = false;
    }

    public void PlayScratchSfx()
    {
        _rubAudioSource.Stop();
        _rubAudioSource.volume = 0;
        isRubbing = false;

        _scratch.Play(transform.position);
    }
    
    public void PlayLaserSfx(bool play)
    {
        if (play)
            _laser.Play(transform.position);
        else
            _laser.Stop(transform.position);
    }

    public void PlayFleshyGrowth(bool play)
    {
        if (play)
            _fleshyGrowth.Play(transform.position);
        else
            _fleshyGrowth.Stop(transform.position);
    }
    
    
    public void PlayThirdEyeLaserSfx(bool play)
    {
        if (play)
            _thirdEyeLaser.Play(transform.position);
        else
            _thirdEyeLaser.Stop(transform.position);
    }

    public void PlayButtonSfx() => _button.Play(transform.position);

    private void PlayClawExtendedSfx() => _clawExtended.Play(transform.position);

    private void PlayClawRetractedSfx() => _clawRetracted.Play(transform.position);

    private void PlayGlintSfx() => _glint.Play(transform.position);

    private void PlayHairballSfx() => _hairball.Play(transform.position);

    public void PlayStaticSfx(bool play)
    {
        if (play)
            _static.Play(transform.position);
        else
            _static.Stop(transform.position);
    }

    private void PlayLightningSfx() => _lightning.Play(transform.position);

    private void PlayTheme(AudioClip clip)
    {
        if (_bgmAudioSource == null || clip == null) return;

        _bgmAudioSource.Stop();
        _bgmAudioSource.time = 0f;
        _bgmAudioSource.clip = clip;
        _bgmAudioSource.Play();
    }

    private void HandleBgmLoop()
    {
        if (!_bgmAudioSource.isPlaying || _bgmAudioSource == null) return;

        if (_bgmAudioSource.clip == _mainTheme.Sfx && _bgmAudioSource.time >= 60f)
        {
            _bgmAudioSource.time -= 48f;
        }
        else if (_bgmAudioSource.clip == _hellTheme.Sfx && _bgmAudioSource.time >= 161.33f)
        {
            _bgmAudioSource.time -= 149.33f;
        }
        else if (_bgmAudioSource.clip == _gameOverTheme.Sfx && _bgmAudioSource.time >= 37.09f)
        {
            _bgmAudioSource.time -= 24.91f;
        }
    }

    private void Rub()
    {
        if (_rubAudioSource == null) return;

        if (IsSoundEffectsMuted)
        {
            _rubAudioSource.volume = 0;
            return;
        }
        
        if (isRubbing && !GameManager.Instance.StateHandler.TryGetState(out PauseState pauseState))
            _rubAudioSource.volume = Mathf.MoveTowards(_rubAudioSource.volume, 1, _rubFadeRate * Time.unscaledDeltaTime);
        else
            _rubAudioSource.volume = Mathf.MoveTowards(_rubAudioSource.volume, 0, _rubFadeRate * Time.unscaledDeltaTime);
    }
}
