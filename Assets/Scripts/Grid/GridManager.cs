using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }

    [SerializeField] private GameObject _groundGrid;
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private GameObject _obstacleGrid;
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private GameObject _cubePoolParent;
    [SerializeField] private GameObject _obstaclePoolParent;
    [SerializeField] private GameObject _goalPrefab;
    [SerializeField] private int _walkableTileType = 1;
    private Node[,] _grid;
    private int[,] _tileTypes;
    // A pool of cubes to avoid instantiation overhead
    private List<GameObject> _cubePool = new List<GameObject>();
    private List<GameObject> _obstaclePool = new List<GameObject>();

    private void Awake()
    {
        // Singleton pattern implementation
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
        // Initialize the cube pool
        if (_cubePoolParent != null && _cubePrefab != null)
        {
            for (int i = 0; i < 100; i++) // Pre-populate the pool with 100 cubes
            {
                GameObject cube = Instantiate(_cubePrefab);
                cube.transform.SetParent(_cubePoolParent.transform);
                cube.SetActive(false);
                _cubePool.Add(cube);
            }
        }
        // Initialize the obstacle pool
        if (_obstaclePoolParent != null && _obstaclePrefab != null)
        {
            for (int i = 0; i < 100; i++) // Pre-populate the pool with 100 obstacles
                {
                GameObject obstacle = Instantiate(_obstaclePrefab);
                obstacle.transform.SetParent(_obstaclePoolParent.transform);
                obstacle.SetActive(false);
                _obstaclePool.Add(obstacle);
            }
        }
    }

    public void InitializeGrid(int width, int height, IntArrayWrapper[] tilesTypes)
    {
        gridWidth = width;
        gridHeight = height;
        _tileTypes = new int[gridHeight, gridWidth];
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                _tileTypes[i, j] = tilesTypes[i].row[j];
            }
        }
        GenerateGrid();
        AdjustCameraToGrid();
    }
    // Adjusts the main camera's orthographic size and position to fit the grid
    public void AdjustCameraToGrid()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float tileSize = GetTileSize();
        float gridWorldWidth = gridWidth * tileSize;
        float gridWorldHeight = gridHeight * tileSize;

        float aspect = cam.aspect;
        float sizeBasedOnHeight = gridWorldHeight / 2f + 1f;
        float sizeBasedOnWidth = gridWorldWidth / (2f * aspect) + 1f;
        cam.orthographicSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);

    }
    public void SpawnGoal(Vector3 goalPosition)
    {
        if (_goalPrefab != null)
        {
            goalPosition.y = _groundGrid.transform.position.y + GetTileSize() / 2f + 1f;
            GameObject goal = Instantiate(_goalPrefab, goalPosition, Quaternion.identity);
            goal.name = "Goal";
            if (_obstacleGrid != null)
            {
                goal.transform.SetParent(_obstacleGrid.transform);
            }
        }
        else
        {
            Debug.LogError("Goal prefab is not assigned in GridManager.");
        }
    }

    public void GenerateGrid()
    {
        // Initialize the _grid array
        _grid = new Node[gridHeight, gridWidth];

        // Get the size of the cube from the prefab's Renderer
        float cubeSize = GetTileSize();

        // Calculate starting offset so _grid is centered at (0,0,0)
        float startX = -(gridWidth * cubeSize) / 2f + cubeSize / 2f;
        float startZ = (gridHeight * cubeSize) / 2f - cubeSize / 2f;

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                // World position
                float x = startX + col * cubeSize;
                float z = startZ - row * cubeSize; // minus because rows go downward

                Vector3 position = new Vector3(x, _groundGrid.transform.position.y, z);

                /// Get a cube from the pool or instantiate a new one
                GameObject cube;
                if (_cubePool.Count > 0)
                {
                    cube = _cubePool[0];
                    _cubePool.RemoveAt(0);
                    cube.transform.position = position;
                    cube.SetActive(true);
                }
                else
                {
                    cube = Instantiate(_cubePrefab, position, Quaternion.identity);
                }
                // Set parent to _groundGrid if assigned
                if (_groundGrid != null)
                {
                    cube.transform.SetParent(_groundGrid.transform);
                }

                // Name cube with its logical coordinates
                cube.name = $"Cube ({row},{col})";

                int tileType = _tileTypes[row, col];
                bool isWall = tileType != _walkableTileType;

                _grid[row, col] = new Node(isWall, row, col, tileType, position);
                // Debug out the row, col and position of the created node
                Debug.Log($"Created Node at ({row},{col}) - Position: {position}, IsWall: {isWall}");
                // If isWall is true, instantiate an obstacle
                if (isWall && _obstaclePrefab != null)
                {
                    // Calculate obstacle y position, it should be the _groundGrid's y position + half the tilesize + half the obstacle prefab object size
                    float obstacleY = _groundGrid.transform.position.y + cubeSize / 2f;
                    Vector3 obstaclePosition = new Vector3(x, obstacleY, z);
                    GameObject obstacle;
                    if (_obstaclePool.Count > 0)
                    {
                        obstacle = _obstaclePool[0];
                        _obstaclePool.RemoveAt(0);
                        obstacle.transform.position = obstaclePosition;
                        obstacle.SetActive(true);
                    }
                    else
                    {
                        obstacle = Instantiate(_obstaclePrefab, obstaclePosition, Quaternion.identity);
                    }
                    // Name obstacle with its logical coordinates
                    obstacle.name = $"Obstacle ({row},{col})";
                    obstacle.transform.SetParent(_obstacleGrid.transform);
                }
            }
        }
    }
    public void DestroyGrid()
    {
        if (_grid != null)
        {
            foreach (Node node in _grid)
            {
                if (node != null)
                {
                    // Return the cube to the pool
                    GameObject cube = _groundGrid.transform.Find($"Cube ({node.row},{node.col})")?.gameObject;
                    if (cube != null)
                    {
                        cube.SetActive(false);
                        _cubePool.Add(cube);
                        if (_cubePoolParent != null)
                        {
                            cube.transform.SetParent(_cubePoolParent.transform);
                        }
                    }
                    // Do the same for obstacles
                    GameObject obstacle = _obstacleGrid.transform.Find($"Obstacle ({node.row},{node.col})")?.gameObject;
                    if (obstacle != null)
                    {
                        obstacle.SetActive(false);
                        _obstaclePool.Add(obstacle);
                        if (_obstaclePoolParent != null)
                        {
                            obstacle.transform.SetParent(_obstaclePoolParent.transform);
                        }
                    }
                }
            }
            _grid = null;

            // Destroy anything left in the grids children
            foreach (Transform child in _groundGrid.transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Destroy(child.gameObject);

                }
            }
            foreach (Transform child in _obstacleGrid.transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
    public void DestroyObstacle(GameObject obstacle)
    {
        if (obstacle != null)
        {
            obstacle.SetActive(false);
            _obstaclePool.Add(obstacle);
            if (_obstaclePoolParent != null)
            {
                obstacle.transform.SetParent(_obstaclePoolParent.transform);
            }
            // Also make the corresponding Node walkable
            Vector2Int coords = WorldToGrid(obstacle.transform.position);
            if (coords.x >= 0 && coords.x < gridHeight && coords.y >= 0 && coords.y < gridWidth)
            {
                Node node = _grid[coords.x, coords.y];
                if (node != null)
                {
                    node.wall = false; // Mark the node as walkable
                }
            }
        }
    }

    // Converts a world position to _grid coordinates (row, col)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float tileSize = GetTileSize();
        float startX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float startZ = (gridHeight * tileSize) / 2f - tileSize / 2f;

        // Calculate column and row
        int col = Mathf.RoundToInt((worldPos.x - startX) / tileSize);
        int row = Mathf.RoundToInt((startZ - worldPos.z) / tileSize);

        // Clamp to _grid bounds
        col = Mathf.Clamp(col, 0, gridWidth - 1);
        row = Mathf.Clamp(row, 0, gridHeight - 1);

        Debug.Log($"[WorldToGrid] worldPos: {worldPos}, tileSize: {tileSize}, startX: {startX}, startZ: {startZ}, row: {row}, col: {col}");
        return new Vector2Int(row, col);
    }

    // Converts _grid coordinates (row, col) to the center world position of the tile
    public Vector3 GridToWorld(Vector2Int _gridPos)
    {
        float tileSize = GetTileSize();
        float startX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float startZ = (gridHeight * tileSize) / 2f - tileSize / 2f;

        // Clamp _gridPos to _grid bounds
        int row = Mathf.Clamp(_gridPos.x, 0, gridHeight - 1);
        int col = Mathf.Clamp(_gridPos.y, 0, gridWidth - 1);

        float x = startX + col * tileSize;
        float z = startZ - row * tileSize;
        float y = _groundGrid != null ? _groundGrid.transform.position.y : 0f;

        Vector3 world = new Vector3(x, y, z);
        Debug.Log($"[_gridToWorld] _gridPos: {_gridPos}, tileSize: {tileSize}, startX: {startX}, startZ: {startZ}, world: {world}");
        return world;
    }

    // Helper to get tile size from prefab
    public float GetTileSize()
    {
        if (_cubePrefab != null)
        {
            Renderer rend = _cubePrefab.GetComponent<Renderer>();
            if (rend != null)
                return rend.bounds.size.x;
            rend = _cubePrefab.GetComponentInChildren<Renderer>();
            if (rend != null)
                return rend.bounds.size.x;
        }
        return 1f;
    }

    // Public methods to access _grid data
    public Node GetNode(int row, int col)
    {
        if (row >= 0 && row < gridHeight && col >= 0 && col < gridWidth)
            return _grid[row, col];
        return null;
    }

    public bool IsWalkable(int row, int col)
    {
        Node node = GetNode(row, col);
        return node != null && node.IsWalkable();
    }

    public Node[,] GetGrid()
    {
        return _grid;
    }

    
}
