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
    private IGameState _currentGameState;

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
    }
    private void Update()
    {
        _currentGameState?.Update();
    }
    public void NewGame(int level)
    {
        // Reset player lives and other game state
        _playerLives = 3;
        LevelManager.Instance.LoadLevel(_levels[level]);
    }
    public void UponPlayerCollision(GameObject other)
    {
        if (other.CompareTag("Obstacle"))
        {
            _playerLives--;
            if (_playerLives <= 0)
            {
                Debug.Log("Game Over");
                // Handle game over logic, e.g., show game over screen, reset lives, etc.
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
            LevelManager.Instance.EndLevel();
            ChangeGameState(new GameWinState());
        }
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