using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }

    [SerializeField] private GameObject groundGrid;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject obstacleGrid;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject cubePoolParent;
    [SerializeField] private GameObject obstaclePoolParent;
    [SerializeField] private int walkableTileType = 1;
    private Node[,] grid;
    private int[,] tileTypes;
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
        if (cubePoolParent != null && cubePrefab != null)
        {
            for (int i = 0; i < 100; i++) // Pre-populate the pool with 100 cubes
            {
                GameObject cube = Instantiate(cubePrefab);
                cube.transform.SetParent(cubePoolParent.transform);
                cube.SetActive(false);
                _cubePool.Add(cube);
            }
        }
        // Initialize the obstacle pool
        if (obstaclePoolParent != null && obstaclePrefab != null)
        {
            for (int i = 0; i < 100; i++) // Pre-populate the pool with 100 obstacles
                {
                GameObject obstacle = Instantiate(obstaclePrefab);
                obstacle.transform.SetParent(obstaclePoolParent.transform);
                obstacle.SetActive(false);
                _obstaclePool.Add(obstacle);
            }
        }
    }

    public void InitializeGrid(int width, int height, IntArrayWrapper[] tilesTypes)
    {
        gridWidth = width;
        gridHeight = height;
        tileTypes = new int[gridHeight, gridWidth];
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                tileTypes[i, j] = tilesTypes[i].row[j];
            }
        }
        GenerateGrid();
        if (groundGrid != null)
        {
            CombineMeshes(groundGrid);
        }
    }

    public void GenerateGrid()
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

                Vector3 position = new Vector3(x, groundGrid.transform.position.y, z);

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
                    cube = Instantiate(cubePrefab, position, Quaternion.identity);
                }
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
                // If isWall is true, instantiate an obstacle
                if (isWall && obstaclePrefab != null)
                {
                    // Calculate obstacle y position, it should be the groundGrid's y position + half the tilesize + half the obstacle prefab object size
                    float obstacleY = groundGrid.transform.position.y + cubeSize / 2f + (obstaclePrefab.GetComponent<Renderer>()?.bounds.size.y ?? 0f) / 2f;
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
                        obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    }
                    // Name obstacle with its logical coordinates
                    obstacle.name = $"Obstacle ({row},{col})";
                    obstacle.transform.SetParent(obstacleGrid.transform);
                }
            }
        }
    }
    public void DestroyGrid()
    {
        if (grid != null)
        {
            foreach (Node node in grid)
            {
                if (node != null)
                {
                    // Return the cube to the pool
                    GameObject cube = groundGrid.transform.Find($"Cube ({node.row},{node.col})")?.gameObject;
                    if (cube != null)
                    {
                        cube.SetActive(false);
                        _cubePool.Add(cube);
                        if (cubePoolParent != null)
                        {
                            cube.transform.SetParent(cubePoolParent.transform);
                        }
                    }
                    // Do the same for obstacles
                    GameObject obstacle = obstacleGrid.transform.Find($"Obstacle ({node.row},{node.col})")?.gameObject;
                    if (obstacle != null)
                    {
                        obstacle.SetActive(false);
                        _obstaclePool.Add(obstacle);
                        if (obstaclePoolParent != null)
                        {
                            obstacle.transform.SetParent(obstaclePoolParent.transform);
                        }
                    }
                }
            }
            // Destroy the combined mesh (leftover children that are not pooled cubes)
            if (groundGrid != null)
            {
                foreach (Transform child in groundGrid.transform)
                {
                    if (!child.name.StartsWith("Cube ("))
                        Destroy(child.gameObject);
                }
            }
            grid = null;
        }
    }
    public void DestroyObstacle(GameObject obstacle)
    {
        if (obstacle != null)
        {
            obstacle.SetActive(false);
            _obstaclePool.Add(obstacle);
            if (obstaclePoolParent != null)
            {
                obstacle.transform.SetParent(obstaclePoolParent.transform);
            }
            // Also make the corresponding Node walkable
            Vector2Int coords = WorldToGrid(obstacle.transform.position);
            if (coords.x >= 0 && coords.x < gridHeight && coords.y >= 0 && coords.y < gridWidth)
            {
                Node node = grid[coords.x, coords.y];
                if (node != null)
                {
                    node.wall = false; // Mark the node as walkable
                }
            }
        }
    }

    // Converts a world position to grid coordinates (row, col)
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float tileSize = GetTileSize();
        float startX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float startZ = (gridHeight * tileSize) / 2f - tileSize / 2f;

        // Calculate column and row
        int col = Mathf.RoundToInt((worldPos.x - startX) / tileSize);
        int row = Mathf.RoundToInt((startZ - worldPos.z) / tileSize);

        // Clamp to grid bounds
        col = Mathf.Clamp(col, 0, gridWidth - 1);
        row = Mathf.Clamp(row, 0, gridHeight - 1);

        Debug.Log($"[WorldToGrid] worldPos: {worldPos}, tileSize: {tileSize}, startX: {startX}, startZ: {startZ}, row: {row}, col: {col}");
        return new Vector2Int(row, col);
    }

    // Converts grid coordinates (row, col) to the center world position of the tile
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float tileSize = GetTileSize();
        float startX = -(gridWidth * tileSize) / 2f + tileSize / 2f;
        float startZ = (gridHeight * tileSize) / 2f - tileSize / 2f;

        // Clamp gridPos to grid bounds
        int row = Mathf.Clamp(gridPos.x, 0, gridHeight - 1);
        int col = Mathf.Clamp(gridPos.y, 0, gridWidth - 1);

        float x = startX + col * tileSize;
        float z = startZ - row * tileSize;
        float y = groundGrid != null ? groundGrid.transform.position.y : 0f;

        Vector3 world = new Vector3(x, y, z);
        Debug.Log($"[GridToWorld] gridPos: {gridPos}, tileSize: {tileSize}, startX: {startX}, startZ: {startZ}, world: {world}");
        return world;
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
        return node != null && node.IsWalkable();
    }

    public Node[,] GetGrid()
    {
        return grid;
    }

    public void CombineMeshes(GameObject parent)
    {
        List<CombineInstance> combineList = new List<CombineInstance>();
        Material firstMaterial = null;
        string firstTag = "Untagged";
        bool foundMaterial = false;
        List<GameObject> cubesToDisable = new List<GameObject>();

        // Iterate over all child cubes of the input parent
        foreach (Transform child in parent.transform)
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mf.sharedMesh == null)
                continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.subMeshIndex = 0;
            ci.transform = child.localToWorldMatrix;
            combineList.Add(ci);

            if (!foundMaterial && mr.sharedMaterial != null)
            {
                firstMaterial = mr.sharedMaterial;
                firstTag = child.tag;
                foundMaterial = true;
            }
            cubesToDisable.Add(child.gameObject);
        }

        if (combineList.Count > 0 && firstMaterial != null)
        {
            GameObject combinedObj = new GameObject($"Combined_{firstMaterial.name}");
            combinedObj.transform.SetParent(parent.transform, false);
            combinedObj.isStatic = true;

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support large meshes
            combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

            MeshFilter mf = combinedObj.AddComponent<MeshFilter>();
            mf.sharedMesh = combinedMesh;

            MeshRenderer mr = combinedObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = firstMaterial;

            // Add a MeshCollider for click/touch detection
            MeshCollider meshCol = combinedObj.AddComponent<MeshCollider>();
            meshCol.sharedMesh = combinedMesh;

            // Set the tag to match the cubes' tag
            combinedObj.tag = firstTag;
        }

        // Disable original cubes
        foreach (var go in cubesToDisable)
        {
            go.SetActive(false);
        }
    }
}
