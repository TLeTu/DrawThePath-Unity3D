using UnityEngine;
using System.Collections.Generic;

public class InputController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController; // Reference to PlayerController for movement

    private void Update()
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
    private void TrySelectTile(Vector2 screenPosition)
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
                    List<Node> path = null;
                    if (isTargetWalkable)
                    {
                        path = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                    }
                    else
                    {
                        // Temporarily treat the target tile as walkable
                        var grid = GridManager.Instance.GetGrid();
                        var targetNode = grid[targetCoords.x, targetCoords.y];
                        bool wasWall = targetNode.wall;
                        targetNode.wall = false;
                        path = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                        targetNode.wall = wasWall;
                    }
                    if (path != null && path.Count >= 1)
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
