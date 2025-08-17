using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // List of TextAssets for levels, can be populated in the inspector
    public bool IsGameRunning => _currentGameState is InGameState;
    [SerializeField] private TextAsset[] _levels;
    [SerializeField] private GameObject _mainMenuUI;
    [SerializeField] private GameObject _levelsMenuUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gameWinUI;
    private int _playerLives = 3;
    private int _currentLevelIndex = 0;
    private IGameState _currentGameState;

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
    }
    private void Start()
    {
        // Initialize the game state, load the first level, etc.
        // NewGame();
        ChangeGameState(new MainMenuState());
        _timer = _maxTime;
        _timerRunning = false;
    }
    private void Update()
    {
        _currentGameState?.Update();
        if (_timerRunning && IsGameRunning)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = 0f;
                _timerRunning = false;
                // Optionally, handle time out (e.g., game over)
                Debug.Log("Time's up!");
                LevelManager.Instance.EndLevel();
                ChangeGameState(new GameOverState());
            }
        }
    }
    public void NewGame(int level)
    {
        _playerLives = 3;
        _timer = _maxTime;
        _timerRunning = true;
        if (level < 0)
        {
            LevelManager.Instance.LoadLevel(_levels[_currentLevelIndex]);
        }
        else
        {
            if (level < 0 || level >= _levels.Length)
            {
                Debug.LogError("Invalid level index.");
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
                LevelManager.Instance.EndLevel();
                ChangeGameState(new GameOverState());
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
            LevelManager.Instance.EndLevel();
            ChangeGameState(new GameWinState());
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
    public void ChangeGameState(IGameState newState)
    {
        if (_currentGameState != null)
        {
            _currentGameState.Exit();
        }
        _currentGameState = newState;
        _currentGameState.Enter();
    }
}