using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // List of TextAssets for levels, can be populated in the inspector
    [SerializeField] private TextAsset[] levels;
    private int _playerLives = 3;

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
        NewGame();
    }
    private void NewGame()
    {
        // Reset player lives and other game state
        _playerLives = 3;
        LevelManager.Instance.LoadLevel(levels[0]);
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
}