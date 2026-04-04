using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Menu Music")]
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private AudioClip _levelSelectMusic; // Optional: separate music for level select, if null uses main menu music

    [Header("Game Music")]
    [SerializeField] private AudioClip _gameplayMusic;
    [SerializeField] private AudioClip _gameOverMusic;
    [SerializeField] private AudioClip _victoryMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip _buttonClickSFX;
    [SerializeField] private AudioClip _playerMoveSFX;
    [SerializeField] private AudioClip _playerCollisionSFX;
    [SerializeField] private AudioClip _levelCompleteSFX;
    [SerializeField] private AudioClip _levelFailSFX;

    [Header("Audio Settings")]
    [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.8f;
    [SerializeField] private bool _muteMusic = false;
    [SerializeField] private bool _muteSFX = false;

    private AudioClip _currentlyPlayingMusic;
    private float _fadeSpeed = 2f;
    private bool _isFading = false;
    private AudioClip _targetMusic;

    // Track fade phases to avoid restarting clips each frame
    private enum FadePhase { None, FadingOut, FadingIn }
    private FadePhase _fadePhase = FadePhase.None;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources if not assigned
        if (_musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            _musicSource = musicGO.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;
            _musicSource.spatialBlend = 0f; // force 2D
            _musicSource.dopplerLevel = 0f; // no doppler for music
        }

        if (_sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            _sfxSource = sfxGO.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;
            _sfxSource.spatialBlend = 0f; // default SFX as 2D in manager
            _sfxSource.dopplerLevel = 0f;
        }

        // Ensure loop is set correctly for existing audio sources
        if (_musicSource != null)
        {
            _musicSource.loop = true;
        }

        UpdateAudioSettings();
    }

    private void OnEnable()
    {
        GameEvents.OnShowMainMenu += PlayMainMenuMusic;
        GameEvents.OnShowLevelsMenu += PlayLevelSelectMusic;
        GameEvents.OnGameStarted += PlayGameplayMusic;
        GameEvents.OnGameOver += PlayGameOverMusic;
        GameEvents.OnGameWin += PlayVictoryMusic;
        GameEvents.OnPlayerCollision += HandlePlayerCollisionSound;
    }

    private void OnDisable()
    {
        GameEvents.OnShowMainMenu -= PlayMainMenuMusic;
        GameEvents.OnShowLevelsMenu -= PlayLevelSelectMusic;
        GameEvents.OnGameStarted -= PlayGameplayMusic;
        GameEvents.OnGameOver -= PlayGameOverMusic;
        GameEvents.OnGameWin -= PlayVictoryMusic;
        GameEvents.OnPlayerCollision -= HandlePlayerCollisionSound;
    }

    private void Update()
    {
        // Handle music crossfading
        if (_isFading)
        {
            HandleMusicCrossfade();
        }
    }

    #region Music Control

    public void PlayMainMenuMusic()
    {
        AudioClip musicToPlay = _mainMenuMusic;
        if (musicToPlay != null)
        {
            PlayMusic(musicToPlay);
        }
    }

    public void PlayLevelSelectMusic()
    {
        // Use level select music if available, otherwise use main menu music
        AudioClip musicToPlay = _levelSelectMusic != null ? _levelSelectMusic : _mainMenuMusic;
        if (musicToPlay != null)
        {
            PlayMusic(musicToPlay);
        }
    }

    public void PlayGameplayMusic()
    {
        if (_gameplayMusic != null)
        {
            PlayMusic(_gameplayMusic);
        }
    }

    public void PlayGameOverMusic()
    {
        if (_gameOverMusic != null)
        {
            PlayMusic(_gameOverMusic);
        }
    }

    public void PlayVictoryMusic()
    {
        if (_victoryMusic != null)
        {
            PlayMusic(_victoryMusic);
        }
        if (_levelCompleteSFX != null)
        {
            PlayLevelCompleteSFX();
        }
    }

    private void PlayVictoryMusic(int score)
    {
        PlayVictoryMusic();
    }

    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _currentlyPlayingMusic = null;
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || _muteMusic) return;

        // If same music is already playing, don't restart it
        if (_currentlyPlayingMusic == clip && _musicSource.isPlaying && !_isFading)
        {
            return;
        }

        // Start crossfade if music is currently playing
        if (_musicSource.isPlaying && _currentlyPlayingMusic != clip)
        {
            StartCrossfade(clip);
        }
        else
        {
            // Play immediately if no music is playing
            _musicSource.clip = clip;
            _musicSource.loop = true; // Ensure loop is enabled
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
            _currentlyPlayingMusic = clip;
            _isFading = false;
            _fadePhase = FadePhase.None;
            _targetMusic = null;
        }
    }

    private void StartCrossfade(AudioClip newClip)
    {
        if (newClip == null || newClip == _currentlyPlayingMusic) return;
        _targetMusic = newClip;
        _isFading = true;
        _fadePhase = FadePhase.FadingOut;
    }

    private void HandleMusicCrossfade()
    {
        switch (_fadePhase)
        {
            case FadePhase.FadingOut:
                if (_musicSource.volume > 0f)
                {
                    _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, 0f, _fadeSpeed * Time.unscaledDeltaTime);
                }
                else
                {
                    // Switch to new music once, at volume 0, then start fading in
                    _musicSource.clip = _targetMusic;
                    _musicSource.loop = true; // Ensure loop is enabled for new music
                    _musicSource.volume = 0f;
                    _musicSource.Play();
                    _currentlyPlayingMusic = _targetMusic;
                    _fadePhase = FadePhase.FadingIn;
                }
                break;

            case FadePhase.FadingIn:
                _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, _musicVolume, _fadeSpeed * Time.unscaledDeltaTime);
                if (_musicSource.volume >= _musicVolume - Mathf.Epsilon)
                {
                    _musicSource.volume = _musicVolume;
                    _isFading = false;
                    _fadePhase = FadePhase.None;
                    _targetMusic = null;
                }
                break;

            default:
                _isFading = false;
                _fadePhase = FadePhase.None;
                break;
        }
    }

    #endregion

    #region Sound Effects

    public void PlayButtonClickSFX()
    {
        PlaySFX(_buttonClickSFX);
    }

    public void PlayPlayerMoveSFX()
    {
        PlaySFX(_playerMoveSFX);
    }

    public void PlayPlayerCollisionSFX()
    {
        PlaySFX(_playerCollisionSFX);
    }

    public void PlayLevelCompleteSFX()
    {
        PlaySFX(_levelCompleteSFX);
    }

    public void PlayLevelFailSFX()
    {
        PlaySFX(_levelFailSFX);
    }

    private void HandlePlayerCollisionSound(GameObject other)
    {
        if (other != null && (other.CompareTag("Obstacle") || other.CompareTag("Enemy")))
        {
            PlayPlayerCollisionSFX();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && !_muteSFX && _sfxSource != null)
        {
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }
    }

    #endregion

    #region Audio Settings

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        if (_musicSource != null && !_isFading)
        {
            _musicSource.volume = _musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        if (_sfxSource != null)
        {
            _sfxSource.volume = _sfxVolume;
        }
    }

    public void SetMusicMute(bool mute)
    {
        _muteMusic = mute;
        if (_musicSource != null)
        {
            _musicSource.mute = mute;
        }
    }

    public void SetSFXMute(bool mute)
    {
        _muteSFX = mute;
        if (_sfxSource != null)
        {
            _sfxSource.mute = mute;
        }
    }

    public void ToggleMusicMute()
    {
        SetMusicMute(!_muteMusic);
    }

    public void ToggleSFXMute()
    {
        SetSFXMute(!_muteSFX);
    }

    private void UpdateAudioSettings()
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _musicVolume;
            _musicSource.mute = _muteMusic;
            _musicSource.loop = true; // Always ensure music loops
            _musicSource.spatialBlend = 0f; // ensure 2D each time
            _musicSource.dopplerLevel = 0f;
        }

        if (_sfxSource != null)
        {
            _sfxSource.volume = _sfxVolume;
            _sfxSource.mute = _muteSFX;
            _sfxSource.loop = false; // SFX should never loop
            _sfxSource.spatialBlend = 0f; // SFX from manager are UI/global
            _sfxSource.dopplerLevel = 0f;
        }
    }

    #endregion

    #region Getters

    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;
    public bool IsMusicMuted() => _muteMusic;
    public bool IsSFXMuted() => _muteSFX;
    public bool IsPlayingMusic() => _musicSource != null && _musicSource.isPlaying;
    public AudioClip GetCurrentMusic() => _currentlyPlayingMusic;

    #endregion
}
