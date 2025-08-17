using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // List of TextAssets for levels, can be populated in the inspector
    public bool IsGameRunning = false;
    [SerializeField] private TextAsset[] _levels;
    [SerializeField] private GameObject _mainMenuUI;
    [SerializeField] private GameObject _levelsMenuUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gameWinUI;
    private int _playerLives = 3;
    private int _currentLevelIndex = 0;
    // Removed GameState system
    // private IGameState _currentGameState;
    // Track progress (highest unlocked level + best scores)
    private PlayerProgress _progress;

    // Timer and scoring system
    [SerializeField] private float _maxTime = 120f; // seconds
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
    }
    private void Start()
    {
        // Initialize the game state, load the first level, etc.
        _timer = _maxTime;
        _timerRunning = false;

        // Show main menu if runtime UI exists
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMainMenu();
        }
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
                // Handle time out -> game over
                Debug.Log("Time's up!");
                if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
                if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
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
            _playerLives--;
            if (_playerLives <= 0)
            {
                Debug.Log("Game Over");
                _timerRunning = false;
                IsGameRunning = false;
                if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
                if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
            }
            else
            {
                Debug.Log($"Player died. Lives remaining: {_playerLives}");
                LevelManager.Instance.DestroyObstacle(other);
                LevelManager.Instance.RespawnPlayer();
            }
        }
        else if (other.CompareTag("Goal"))
        {
            Debug.Log("Player reached the goal!");
            _timerRunning = false;
            CalculateScore();
            Debug.Log($"Score: {_score}");

            // Save progress: best score for this level and unlock next
            SaveSystem.UpdateLevelResult(_progress, _currentLevelIndex, _score, _levels != null ? _levels.Length : 0);
            SaveSystem.Save(_progress);

            if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
            if (UIManager.Instance != null) UIManager.Instance.ShowGameWin();
            IsGameRunning = false;
        }
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

    public int GetHighestUnlockedLevel()
    {
        return _progress != null ? _progress.highestUnlockedLevel : 0;
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

    public bool TryPlayNextLevel()
    {
        int next = _currentLevelIndex + 1;
        if (next < GetLevelsCount() && IsLevelUnlocked(next))
        {
            NewGame(next);
            return true;
        }
        return false;
    }

    public void ShowMainMenu()
    {
        if (_mainMenuUI != null)
        {
            _mainMenuUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Main Menu UI is not assigned in GameManager.");
        }
    }
    public void HideMainMenu()
    {
        if (_mainMenuUI != null)
        {
            _mainMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Main Menu UI is not assigned in GameManager.");
        }
    }
    public void ShowLevelsMenu()
    {
        if (_levelsMenuUI != null)
        {
            _levelsMenuUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Levels Menu UI is not assigned in GameManager.");
        }
    }
    public void HideLevelsMenu()
    {
        if (_levelsMenuUI != null)
        {
            _levelsMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Levels Menu UI is not assigned in GameManager.");
        }
    }
    public void ShowGameOverUI()
    {
        if (_gameOverUI != null)
        {
            _gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over UI is not assigned in GameManager.");
        }
    }
    public void HideGameOverUI()
    {
        if (_gameOverUI != null)
        {
            _gameOverUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Game Over UI is not assigned in GameManager.");
        }
    }
    public void ShowGameWinUI()
    {
        if (_gameWinUI != null)
        {
            _gameWinUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Win UI is not assigned in GameManager.");
        }
    }
    public void HideGameWinUI()
    {
        if (_gameWinUI != null)
        {
            _gameWinUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Game Win UI is not assigned in GameManager.");
        }
    }

    // Auto-load all level TextAssets from Resources/Levels if not assigned in inspector
    private void AutoLoadLevelsIfEmpty()
    {
        if (_levels == null || _levels.Length == 0)
        {
            var loaded = Resources.LoadAll<TextAsset>("Levels");
            _levels = loaded ?? new TextAsset[0];
            Debug.Log($"[GameManager] Auto-loaded {_levels.Length} level(s) from Resources/Levels");
        }
    }
}