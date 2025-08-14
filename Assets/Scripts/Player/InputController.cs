using UnityEngine;

public class InputController : MonoBehaviour
{
    void Update()
    {
        // Mouse left click
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse left click detected");
        }

        // Touch input (for mobile)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Debug.Log("Touch input detected");
                }
            }
        }
    }
}
