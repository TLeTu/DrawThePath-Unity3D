using UnityEngine;

public class GridController : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    [SerializeField] private GridMeshCombiner _meshCombiner;
    private GameObject _gridCore; // Optional parent for the grid cubes

    void Start()
    {
        // The gridCore the object this script is attached to
        _gridCore = this.gameObject;
        GenerateGrid();
        if (_meshCombiner != null)
        {
            _meshCombiner.CombineMeshesByMaterial();
        }
    }

    void GenerateGrid()
    {
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
        float startX = - (gridWidth * cubeSize) / 2f + cubeSize / 2f;
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
                // Set parent to gridCore if assigned
                if (_gridCore != null)
                {
                    cube.transform.SetParent(_gridCore.transform);
                }

                // Name cube with its logical coordinates
                cube.name = $"Cube ({row},{col})";
            }
        }
    }
}
