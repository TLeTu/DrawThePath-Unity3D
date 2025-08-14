using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerController playerController; // Reference to PlayerController for movement
    public GridController gridController; // Reference to GridController for grid math

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
            if (gridController != null && hit.collider.gameObject.tag == "Ground")
            {
                Vector3 hitPoint = hit.point;
                Vector2Int gridPos = gridController.WorldToGrid(hitPoint);
                Vector3 center = gridController.GridToWorld(gridPos);
                Debug.Log($"Selected tile: row={gridPos.x}, col={gridPos.y}, center={center}");
                if (playerController != null)
                {
                    Debug.Log($"Moving player towards: {center}");
                    playerController.MoveTowardsXZ(center);
                }
            }
        }
    }
}
