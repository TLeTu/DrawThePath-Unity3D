using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // List of TextAssets for levels, can be populated in the inspector
    [SerializeField] private TextAsset[] _levels;
    [SerializeField] private GameObject _mainMenuUI;
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
    private void NewGame()
    {
        // Reset player lives and other game state
        _playerLives = 3;
        LevelManager.Instance.LoadLevel(_levels[0]);
    }
    public void UponPlayerCollision(GameObject other)
    {
        _playerLives--;
        if (_playerLives <= 0)
        {
            Debug.Log("Game Over");
            // Handle game over logic, e.g., show game over screen, reset lives, etc.
            LevelManager.Instance.EndLevel();
        }
        else
        {
            Debug.Log($"Player died. Lives remaining: {_playerLives}");
            LevelManager.Instance.DestroyObstacle(other);
            LevelManager.Instance.RespawnPlayer();
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