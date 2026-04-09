using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // List of TextAssets for levels, can be populated in the inspector
    public bool IsGameRunning = false;
    [SerializeField] private TextAsset[] _levels;
    private int _playerLives = 3;
    private int _currentLevelIndex = 0;
    // Removed GameState system
    // private IGameState _currentGameState;
    // Track progress (highest unlocked level + best scores)
    private PlayerProgress _progress;

    // Timer and scoring system
    [Header("Scoring")]
    [SerializeField] private float _maxTime = 120f; // seconds
    [Range(0, 1)] [SerializeField] private float _3StarThreshold = 0.8f; // 80%
    [Range(0, 1)] [SerializeField] private float _2StarThreshold = 0.5f; // 50%
    private float _timer;
    private bool _timerRunning = false;
    private int _score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Auto-load levels if none are assigned in the inspector
        AutoLoadLevelsIfEmpty();

        // Load player progress and ensure lists are sized to number of levels
        _progress = SaveSystem.Load();
        SaveSystem.EnsureCapacity(_progress, _levels != null ? _levels.Length : 0);

        // Listen to events that this manager is responsible for
        GameEvents.OnGameOver += OnGameOver;
        GameEvents.OnGameWin += OnGameWin;
    }

    private void OnEnable()
    {
        GameEvents.OnStartGameRequested += NewGame;
        GameEvents.OnPlayerCollision += UponPlayerCollision;
        GameEvents.OnRetryLevelRequested += RetryLevel;
        GameEvents.OnPlayNextLevelRequested += HandlePlayNextLevelRequest;
        GameEvents.OnGoToLevelsMenuRequested += HandleGoToLevelsMenuRequest;
    }

    private void OnDisable()
    {
        GameEvents.OnStartGameRequested -= NewGame;
        GameEvents.OnPlayerCollision -= UponPlayerCollision;
        GameEvents.OnRetryLevelRequested -= RetryLevel;
        GameEvents.OnPlayNextLevelRequested -= HandlePlayNextLevelRequest;
        GameEvents.OnGoToLevelsMenuRequested -= HandleGoToLevelsMenuRequest;
        
        GameEvents.OnGameOver -= OnGameOver;
        GameEvents.OnGameWin -= OnGameWin;
    }

    private void Start()
    {
        _timer = _maxTime;
        _timerRunning = false;
        GameEvents.TriggerShowMainMenu();
    }
    private void Update()
    {
        // Removed _currentGameState?.Update();
        if (_timerRunning && IsGameRunning)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = 0f;
                _timerRunning = false;
                IsGameRunning = false;
                Debug.Log("Time's up! Firing OnGameOver event.");
                GameEvents.TriggerGameOver();
            }
        }
    }
    public void NewGame(int level)
    {
        IsGameRunning = true;
        // Ensure we have levels available
        if (_levels == null || _levels.Length == 0)
        {
            Debug.LogError("No levels available. Assign _levels in the inspector or place TextAssets under Assets/Resources/Levels.");
            return;
        }

        _playerLives = 3;
        _timer = _maxTime;
        _timerRunning = true;
        if (level < 0)
        {
            if (_currentLevelIndex < 0 || _currentLevelIndex >= _levels.Length)
            {
                Debug.LogError($"Current level index {_currentLevelIndex} is out of range (0..{_levels.Length - 1}).");
                return;
            }
            LevelManager.Instance.LoadLevel(_levels[_currentLevelIndex]);
        }
        else
        {
            if (level < 0 || level >= _levels.Length)
            {
                Debug.LogError($"Invalid level index {level}. Levels available: {_levels.Length}");
                return;
            }
            // Enforce locked levels
            if (!IsLevelUnlocked(level))
            {
                Debug.LogWarning($"Level {level} is locked. Highest unlocked: {GetHighestUnlockedLevel()}");
                return;
            }
            LevelManager.Instance.LoadLevel(_levels[level]);
            _currentLevelIndex = level;
        }
        _score = 0;

        GameEvents.TriggerGameStarted();
        GameEvents.TriggerShowInGameHUD();
    }
    public TextAsset GetCurrentLevel()
    {
        if (_currentLevelIndex < 0 || _currentLevelIndex >= _levels.Length)
        {
            Debug.LogError("Invalid level index.");
            return null;
        }
        return _levels[_currentLevelIndex];
    }
    public void UponPlayerCollision(GameObject other)
    {
        if (other.CompareTag("Obstacle"))
        {
            StartCoroutine(HandleObstacleCollision(other));
        }
        else if (other.CompareTag("Goal"))
        {
            Debug.Log("Player reached the goal!");
            _timerRunning = false;
            CalculateScore();
            Debug.Log($"Score: {_score}");

            int earnedStars = CalculateStars(_score);

            // Save progress: best score for this level and unlock next
            SaveSystem.UpdateLevelResult(_progress, _currentLevelIndex, _score, earnedStars, _levels != null ? _levels.Length : 0);
            SaveSystem.Save(_progress);
            IsGameRunning = false;
            GameEvents.TriggerGameWin(_score, earnedStars);
        }
        else if (other.CompareTag("Enemy"))
        {
            StartCoroutine(HandleEnemyCollision());
        }
    }

    private System.Collections.IEnumerator HandleObstacleCollision(GameObject other)
    {
        // Call the levelmanager PlayDeadAnimation
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.PlayDeadAnimation();
        }
        
        // Wait for animation to complete (adjust time as needed)
        yield return new WaitForSeconds(2.0f);
        
        _playerLives--;
        if (_playerLives <= 0)
        {
            Debug.Log("Game Over");
            _timerRunning = false;
            IsGameRunning = false;
            GameEvents.TriggerGameOver();
        }
        else
        {
            Debug.Log($"Player died. Lives remaining: {_playerLives}");
            LevelManager.Instance.DestroyObstacle(other);
            LevelManager.Instance.RespawnPlayer();
        }
    }

    private System.Collections.IEnumerator HandleEnemyCollision()
    {
        Debug.Log("Player collided with an enemy, respawning...");
        
        // Call the levelmanager PlayDeadAnimation
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.PlayDeadAnimation();
        }
        
        // Wait for animation to complete (adjust time as needed)
        yield return new WaitForSeconds(2.0f);
        
        _playerLives--;
        if (_playerLives <= 0)
        {
            Debug.Log("Game Over");
            _timerRunning = false;
            IsGameRunning = false;
            GameEvents.TriggerGameOver();
        }
        else
        {
            Debug.Log($"Player died. Lives remaining: {_playerLives}");
            LevelManager.Instance.RespawnPlayer();
        }
    }

    private void OnGameOver()
    {
        IsGameRunning = false;
        _timerRunning = false;
        GameEvents.TriggerEndLevel();
    }

    private void OnGameWin(int score, int stars)
    {
        IsGameRunning = false;
        _timerRunning = false;
        GameEvents.TriggerEndLevel();
    }


    private void CalculateScore()
    {
        // Example: score is proportional to time left
        _score = Mathf.RoundToInt(_timer * 10); // 10 points per second left
    }

    public int GetScore()
    {
        return _score;
    }

    public float GetTimeLeft()
    {
        return _timer;
    }

    public float GetMaxTime()
    {
        return _maxTime;
    }

    public int CalculateStars(int score)
    {
        if (score <= 0) return 0;

        float maxScore = _maxTime * 10;
        if (score >= maxScore * _3StarThreshold)
        {
            return 3;
        }
        if (score >= maxScore * _2StarThreshold)
        {
            return 2;
        }
        return 1;
    }

    // --- Progress helpers ---
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (_levels == null) return false;
        if (levelIndex < 0 || levelIndex >= _levels.Length) return false;
        if (_progress == null) return levelIndex == 0; // default: only level 0 unlocked
        return levelIndex <= _progress.highestUnlockedLevel;
    }

    public int GetBestScore(int levelIndex)
    {
        if (_levels == null) return 0;
        if (_progress == null) return 0;
        SaveSystem.EnsureCapacity(_progress, _levels.Length);
        if (levelIndex < 0 || levelIndex >= _progress.bestScores.Count) return 0;
        return _progress.bestScores[levelIndex];
    }

    public int GetBestStars(int levelIndex)
    {
        if (_levels == null) return 0;
        if (_progress == null) return 0;
        SaveSystem.EnsureCapacity(_progress, _levels.Length);
        if (levelIndex < 0 || levelIndex >= _progress.stars.Count) return 0;
        return _progress.stars[levelIndex];
    }

    public int GetHighestUnlockedLevel()
    {
        return _progress != null ? _progress.highestUnlockedLevel : 0;
    }

    public int GetTotalCollectedStars()
    {
        return SaveSystem.GetTotalStars(_progress);
    }

    public void ResetProgress()
    {
        SaveSystem.ResetAll();
        _progress = new PlayerProgress();
        SaveSystem.EnsureCapacity(_progress, _levels != null ? _levels.Length : 0);
    }

    public int GetLevelsCount() => _levels != null ? _levels.Length : 0;
    public int GetCurrentLevelIndex() => _currentLevelIndex;

    public void RetryLevel()
    {
        NewGame(_currentLevelIndex);
    }

    private void HandlePlayNextLevelRequest()
    {
        int next = _currentLevelIndex + 1;
        if (next < GetLevelsCount() && IsLevelUnlocked(next))
        {
            NewGame(next);
        }
        // If we can't play, nothing happens. The UI will remain on the win screen.
    }

    private void HandleGoToLevelsMenuRequest()
    {
        IsGameRunning = false;
        GameEvents.TriggerEndLevel();
        GameEvents.TriggerShowLevelsMenu();
    }

    // Auto-load all level TextAssets from Resources/Levels if not assigned in inspector
    private void AutoLoadLevelsIfEmpty()
    {
        if (_levels == null || _levels.Length == 0)
        {
            var loaded = Resources.LoadAll<TextAsset>("Levels");
            if (loaded != null && loaded.Length > 0)
            {
                // Sort the levels numerically based on the number in their name
                _levels = loaded.OrderBy(asset => 
                {
                    Match match = Regex.Match(asset.name, @"\d+");
                    return match.Success ? int.Parse(match.Value) : 0;
                }).ToArray();
            }
            else
            {
                _levels = new TextAsset[0];
            }
            Debug.Log($"[GameManager] Auto-loaded {_levels.Length} level(s) from Resources/Levels");
        }
    }
}