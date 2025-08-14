using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    private Vector3? _moveTarget = null;

    // Call this to start moving toward the target position (x and z only)
    public void MoveTowardsXZ(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        _moveTarget = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
    }

    private void Update()
    {
        if (_moveTarget.HasValue)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetXZ = _moveTarget.Value;
            if (Vector3.Distance(currentPosition, targetXZ) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(currentPosition, targetXZ, _moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetXZ;
                _moveTarget = null;
            }
        }
    }
}
