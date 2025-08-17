using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    [SerializeField] private string _levelsFolderName;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyPoolParent;
    [SerializeField] private int _initialEnemyCount = 10; // Initial number of enemies in the pool
    private List<GameObject> _enemyPool = new List<GameObject>();
    private List<GameObject> _enemiesInLevel = new List<GameObject>();

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
        // Initialize enemy pool
        if (_enemyPoolParent != null && _enemyPrefab != null)
        {
            for (int i = 0; i < _initialEnemyCount; i++)
            {
                GameObject enemyObject = Instantiate(_enemyPrefab, _enemyPoolParent.transform);
                enemyObject.SetActive(false);
                _enemyPool.Add(enemyObject);
            }
        }
    }
    private void SpawnEnemies(List<EnemyData> enemies)
    {
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("No enemies to spawn.");
            return;
        }

        foreach (var enemyData in enemies)
        {
            Vector3 startPosition = GridManager.Instance.GridToWorld(new Vector2Int(enemyData.startX, enemyData.startY));
            startPosition.y = GridManager.Instance.GetTileSize() / 2f; // Adjust Y position to be above the ground

            Vector3 endPosition = GridManager.Instance.GridToWorld(new Vector2Int(enemyData.endX, enemyData.endY));
            // Get from the pool or instantiate a new enemy
            GameObject enemyObject;
            if (_enemyPool.Count > 0)
            {
                enemyObject = _enemyPool[0];
                _enemyPool.RemoveAt(0);
                enemyObject.SetActive(true);
                _enemiesInLevel.Add(enemyObject);
            }
            else
            {
                enemyObject = Instantiate(_enemyPrefab, startPosition, Quaternion.identity);
                enemyObject.transform.SetParent(_enemyPoolParent.transform);
                _enemiesInLevel.Add(enemyObject);
            }
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.SetPosition(startPosition, endPosition);
            }
            else
            {
                Debug.LogError("EnemyController component is missing on the enemy prefab.");
            }
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
            SpawnEnemies(levelData.enemies);
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
        EndLevel();
        if (GameManager.Instance != null)
        {
            LoadLevel(GameManager.Instance.GetCurrentLevel());
        }
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
        foreach (var enemy in _enemiesInLevel)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
                _enemyPool.Add(enemy);
                enemy.GetComponent<EnemyController>().RemovePath();
            }
        }
        _enemiesInLevel.Clear();
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