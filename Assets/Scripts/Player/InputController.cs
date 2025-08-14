using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerController playerController; // Reference to PlayerController for movement
    public string targetTag = "YourTag"; // Set this to the tag you want to detect
    void Update()
    {
        // Mouse left click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse left click detected");
            GameObject obj = GetGameObjectAtPointerWithTag(targetTag);
            if (obj != null)
            {
                Debug.Log("Clicked object position: " + obj.transform.position);
                playerController.MoveTowardsXZ(obj.transform.position);
            }
        }

        // Touch input (for mobile)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("Touch input detected");
                    GameObject obj = GetGameObjectAtPointerWithTag(targetTag, touch.position);
                    if (obj != null)
                    {
                        Debug.Log("Touched object position: " + obj.transform.position);
                        playerController.MoveTowardsXZ(obj.transform.position);
                    }
                }
            }
        }
    }

    // Returns the GameObject with the specified tag at the mouse position (for mouse input)
    GameObject GetGameObjectAtPointerWithTag(string tag)
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(tag))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    // Overload for touch input
    GameObject GetGameObjectAtPointerWithTag(string tag, Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(tag))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }
}
