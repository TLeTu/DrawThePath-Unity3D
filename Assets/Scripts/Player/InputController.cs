using UnityEngine;
using System.Collections.Generic;

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

                if (playerController != null && AStarPathfinding.Instance != null)
                {
                    bool isTargetWalkable = GridManager.Instance.IsWalkable(targetCoords.x, targetCoords.y);
                    var path = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                    if (isTargetWalkable)
                    {
                        if (path != null && path.Count >= 1)
                        {
                            playerController.FollowPath(path);
                        }
                        else
                        {
                            Debug.LogWarning("No path found to target tile.");
                        }
                    }
                    else
                    {
                        // Try to find a path to the unwalkable tile (should end adjacent)
                        if (path != null && path.Count >= 1)
                        {
                            playerController.FollowPath(path);
                        }
                        else
                        {
                            // Temporarily treat all tiles as walkable and try again
                            Debug.Log("No path to unwalkable tile, trying with all tiles walkable...");
                            // Backup walkable state
                            var grid = GridManager.Instance.GetGrid();
                            List<(int,int,bool)> backup = new List<(int,int,bool)>();
                            for (int row = 0; row < GridManager.Instance.gridHeight; row++)
                            {
                                for (int col = 0; col < GridManager.Instance.gridWidth; col++)
                                {
                                    var node = grid[row, col];
                                    backup.Add((row, col, node.wall));
                                    node.wall = false;
                                }
                            }
                            var pathAny = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                            // Restore walkable state
                            foreach (var (row, col, wasWall) in backup)
                            {
                                grid[row, col].wall = wasWall;
                            }
                            if (pathAny != null && pathAny.Count >= 1)
                            {
                                playerController.FollowPath(pathAny);
                            }
                            else
                            {
                                Debug.LogWarning("No path found to target tile, even with all tiles walkable.");
                            }
                        }
                    }
                }
            }
        }
    }
}
