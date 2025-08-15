using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerController playerController; // Reference to PlayerController for movement

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
                Vector2Int targetCoords = GridManager.Instance.WorldToGrid(hit.point);
                Debug.Log($"Clicked grid cell: Row {targetCoords.x}, Col {targetCoords.y}");
                Vector3 worldPosition = GridManager.Instance.GridToWorld(targetCoords);
                Debug.Log($"World position of clicked tile: {worldPosition}");

                if (playerController != null)
                {
                    // Use AStarPathfinding singleton to get path from player to clicked tile
                    if (AStarPathfinding.Instance != null)
                    {
                        var path = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                        if (path != null && path.Count > 1)
                        {
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
}
