using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private string _levelsFolderName;
    [SerializeField] private PlayerController _playerController;
    private Vector3 _playerSpawnPosition;

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
    public void LoadLevel(string levelName)
    {
        Debug.Log($"Loading level: {levelName}");
        // Implement level loading logic here
        LevelData levelData = LoadLevelDataFromJson<LevelData>(levelName);

        if (GridManager.Instance != null && levelData != null)
        {
            GridManager.Instance.InitializeGrid(levelData.width, levelData.height, levelData.tiles);
            _playerSpawnPosition = GridManager.Instance.GridToWorld(new Vector2Int(levelData.startTileX, levelData.startTileY));
            if (_playerController != null)
            {
                _playerController.SpawnPlayer(_playerSpawnPosition);
            }
            else
            {
                Debug.LogError("PlayerController is not assigned in LevelManager.");
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
    public T LoadLevelDataFromJson<T>(string fileName)
    {
        // Example: folder = "Levels", fileName = "Level1.json"
        string path = System.IO.Path.Combine(Application.dataPath, _levelsFolderName, fileName);
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"JSON file not found at path: {path}");
            return default;
        }
        string json = System.IO.File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);
        Debug.Log($"Loaded JSON from {path}");
        // Debug out the data
        Debug.Log($"Data: {JsonUtility.ToJson(data, true)}");
        return data;
    }
}