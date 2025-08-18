using UnityEngine;
using UnityEngine.InputSystem; // <-- New Input System
using System.Collections.Generic;

public class InputController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning || playerController.IsDead)
            return;

        // --- Mouse input (New Input System) ---
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            TrySelectTile(mousePos);
        }

        // --- Touch input (New Input System) ---
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                {
                    Vector2 touchPos = touch.position.ReadValue();
                    TrySelectTile(touchPos);
                }
            }
        }
    }

    // Attempts to select a tile at the given screen position
    private void TrySelectTile(Vector2 screenPosition)
    {
        Debug.Log($"Trying to select tile at screen position: {screenPosition}");
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");

            if (GridManager.Instance != null && hit.collider.CompareTag("Ground"))
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
                        var grid = GridManager.Instance.GetGrid();
                        var targetNode = grid[targetCoords.x, targetCoords.y];
                        bool wasWall = targetNode.wall;
                        targetNode.wall = false;
                        path = AStarPathfinding.Instance.FindPath(playerController.transform.position, worldPosition);
                        targetNode.wall = wasWall;
                    }

                    if (path != null && path.Count >= 1)
                        playerController.FollowPath(path);
                    else
                        Debug.LogWarning("No path found to target tile.");
                }
            }
        }
    }
}
