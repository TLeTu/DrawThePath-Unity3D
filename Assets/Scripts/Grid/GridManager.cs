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
    [SerializeField] private int walkableTileType = 10;
    private Node[,] grid;
    private int[,] tileTypes;

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
    }

    public void InitializeGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        tileTypes = new int[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tileTypes[i, j] = walkableTileType; // Default to walkable
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
                // If isWall is true, instantiate an obstacle
                if (isWall && obstaclePrefab != null)
                {
                    Vector3 obstaclePosition = new Vector3(x, obstacleGrid.transform.position.y, z);
                    GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
                    obstacle.transform.SetParent(obstacleGrid.transform);
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
            combinedObj.transform.SetParent(transform, false);
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
