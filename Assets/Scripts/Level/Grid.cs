using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cubeSize = 1f; // size of each cube

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
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

                // Name cube with its logical coordinates
                cube.name = $"Cube ({row},{col})";
            }
        }
    }
}
