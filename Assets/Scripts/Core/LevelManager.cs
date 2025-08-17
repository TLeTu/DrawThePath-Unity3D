using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private string _levelsFolderName;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private EnemyController _enemyController;
    private Vector3 _playerSpawnPosition;
    private Vector3 _endPosition;

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

    // Method to load a level by name
    public void LoadLevel(TextAsset levelAsset)
    {
        // Implement level loading logic here
        LevelData levelData = LoadLevelDataFromJson<LevelData>(levelAsset);

        if (GridManager.Instance != null && levelData != null)
        {
            GridManager.Instance.InitializeGrid(levelData.width, levelData.height, levelData.tiles);
            _playerSpawnPosition = GridManager.Instance.GridToWorld(new Vector2Int(levelData.startTileX, levelData.startTileY));
            _endPosition = GridManager.Instance.GridToWorld(new Vector2Int(levelData.endTileX, levelData.endTileY));
            if (_playerController != null)
            {
                _playerController.SpawnPlayer(_playerSpawnPosition);
            }
            else
            {
                Debug.LogError("PlayerController is not assigned in LevelManager.");
            }
            GridManager.Instance.SpawnGoal(_endPosition);
            if (_enemyController != null)
            {
                _enemyController.SetPosition(_endPosition, _playerSpawnPosition);
            }
            else
            {
                Debug.LogError("EnemyController is not assigned in LevelManager.");
            }
        }
        else
        {
            Debug.LogError("GridManager instance is null or level data is invalid.");
        }
    }

    // Method to reset the current level
    public void ResetLevel()
    {
        Debug.Log("Resetting current level");
        // Implement level reset logic here
    }
    public void EndLevel()
    {
        Debug.Log("Ending current level");
        // Implement level end logic here, e.g., show end screen, load next level, etc.
        _playerController.Destroy();
        if (GridManager.Instance != null)
        {
            GridManager.Instance.DestroyGrid();
        }
    }
    public void DestroyObstacle(GameObject obstacle)
    {
        if (obstacle != null)
        {
            GridManager.Instance.DestroyObstacle(obstacle);
        }
        else
        {
            Debug.LogWarning("Attempted to destroy a null obstacle.");
        }
    }
    public void RespawnPlayer()
    {
        if (_playerController != null)
        {
            Debug.Log($"Respawning player at: {_playerSpawnPosition}");
            _playerController.SpawnPlayer(_playerSpawnPosition);
        }
        else
        {
            Debug.LogError("PlayerController is not assigned in LevelManager.");
        }
    }
    public T LoadLevelDataFromJson<T>(TextAsset levelAsset) where T : class
    {
        if (levelAsset == null)
        {
            Debug.LogError("Level asset is null.");
            return null;
        }
        string json = levelAsset.text;
        T data = JsonUtility.FromJson<T>(json);
        Debug.Log($"Loaded JSON from TextAsset: {levelAsset.name}");
        // Debug out the data
        Debug.Log($"Data: {JsonUtility.ToJson(data, true)}");
        return data;
    }
}