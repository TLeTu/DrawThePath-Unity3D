using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerController playerController; // Reference to PlayerController for movement
    public AStarPathfinding pathfinder; // Reference to AStarPathfinding

    void Update()
    {
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked, trying to select tile...");

            TrySelectTile(Input.mousePosition);
        }
        // Touch input
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    TrySelectTile(touch.position);
                }
            }
        }
    }

    // Attempts to select a tile at the given screen position
    void TrySelectTile(Vector2 screenPosition)
    {
        Debug.Log($"Trying to select tile at screen position: {screenPosition}");
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            // Check if we hit the grid's collider
            if (GridManager.Instance != null && hit.collider.gameObject.tag == "Ground")
            {
                Vector2Int gridCoords = GridManager.Instance.WorldToGrid(hit.point);
                Debug.Log($"Clicked grid cell: Row {gridCoords.x}, Col {gridCoords.y}");
                Vector3 worldPosition = GridManager.Instance.GridToWorld(gridCoords);
                Debug.Log($"World position of clicked tile: {worldPosition}");

                // Use AStarPathfinding to get path from player to clicked tile
                if (playerController != null && pathfinder != null)
                {
                    Vector3 playerPos = playerController.transform.position;
                    var path = pathfinder.FindPath(playerPos, worldPosition);
                    if (path != null && path.Count > 0)
                    {
                        // Move player along the path
                        playerController.FollowPath(path);
                    }
                    else
                    {
                        Debug.LogWarning("No path found to target tile.");
                    }
                }
            }
        }
    }
}
