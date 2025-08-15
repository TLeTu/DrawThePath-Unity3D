using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public GameObject groundGrid;

    public GameObject cubePrefab;
    public int gridWidth = 8;
    public int gridHeight = 8;
    public int[,] tileTypes = new int[,] {
        {7, 9, 9, 9, 9, 9, 9, 4},
        {8, 10, 10, 10, 10, 10, 10, 8},
        {8, 10, 2, 4, 10, 3, 10, 8},
        {8, 10, 10, 8, 10, 8, 10, 8},
        {8, 10, 10, 6, 9, 5, 10, 8},
        {8, 10, 10, 10, 10, 10, 10, 8},
        {8, 10, 10, 7, 4, 10, 10, 8},
        {6, 9, 9, 5, 6, 9, 9, 5}
    };
    public int walkableTileType = 10;
    private Node[,] grid;

    [SerializeField] private GridMeshCombiner _meshCombiner;

    void Awake()
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
    }

    void Start()
    {
        GenerateGrid();
        if (_meshCombiner != null)
        {
            _meshCombiner.CombineMeshesByMaterial();
        }
    }

    void GenerateGrid()
    {
        // Initialize the grid array
        grid = new Node[gridHeight, gridWidth];
        
        // Get the size of the cube from the prefab's Renderer
        float cubeSize = 1f;
        if (cubePrefab != null)
        {
            Renderer rend = cubePrefab.GetComponent<Renderer>();
            if (rend != null)
            {
                cubeSize = rend.bounds.size.x;
            }
            else
            {
                // Try to get from a child if not on root
                rend = cubePrefab.GetComponentInChildren<Renderer>();
                if (rend != null)
                    cubeSize = rend.bounds.size.x;
            }
        }

        // Calculate starting offset so grid is centered at (0,0,0)
        float startX = -(gridWidth * cubeSize) / 2f + cubeSize / 2f;
        float startZ = (gridHeight * cubeSize) / 2f - cubeSize / 2f;

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                // World position
                float x = startX + col * cubeSize;
                float z = startZ - row * cubeSize; // minus because rows go downward

                Vector3 position = new Vector3(x, 0, z);

                // Spawn cube
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                // Set parent to groundGrid if assigned
                if (groundGrid != null)
                {
                    cube.transform.SetParent(groundGrid.transform);
                }

                // Name cube with its logical coordinates
                cube.name = $"Cube ({row},{col})";

                int tileType = tileTypes[row, col];
                bool isWall = tileType != walkableTileType;

                grid[row, col] = new Node(isWall, row, col, tileType, position);
                // Debug out the row, col and position of the created node
                Debug.Log($"Created Node at ({row},{col}) - Position: {position}, IsWall: {isWall}");
            }
        }
    }

    // Converts a world position to grid coordinates (row, col)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float tileSize = GetTileSize();
        float originX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float originZ = (gridHeight * tileSize) / 2f - tileSize / 2f;
        float localX = worldPos.x - originX;
        float localZ = originZ - worldPos.z;
        int col = Mathf.FloorToInt(localX / tileSize);
        int row = Mathf.FloorToInt(localZ / tileSize);
        col = Mathf.Clamp(col, 0, gridWidth - 1);
        row = Mathf.Clamp(row, 0, gridHeight - 1);
        return new Vector2Int(row, col);
    }

    // Converts grid coordinates (row, col) to the center world position of the tile
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float tileSize = GetTileSize();
        float originX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float originZ = (gridHeight * tileSize) / 2f - tileSize / 2f;
        float x = originX + gridPos.y * tileSize; // gridPos.y is col
        float z = originZ - gridPos.x * tileSize; // gridPos.x is row
        return new Vector3(x, 0f, z);
    }

    // Helper to get tile size from prefab
    public float GetTileSize()
    {
        if (cubePrefab != null)
        {
            Renderer rend = cubePrefab.GetComponent<Renderer>();
            if (rend != null)
                return rend.bounds.size.x;
            rend = cubePrefab.GetComponentInChildren<Renderer>();
            if (rend != null)
                return rend.bounds.size.x;
        }
        return 1f;
    }

    // Public methods to access grid data
    public Node GetNode(int row, int col)
    {
        if (row >= 0 && row < gridHeight && col >= 0 && col < gridWidth)
            return grid[row, col];
        return null;
    }

    public bool IsWalkable(int row, int col)
    {
        Node node = GetNode(row, col);
        return node != null && !node.IsWalkable();
    }

    public Node[,] GetGrid()
    {
        return grid;
    }
}
