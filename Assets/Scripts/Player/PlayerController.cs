using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;



    private Vector3? _moveTarget = null;
    private Queue<Vector3> _pathQueue = null;

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
                // If following a path, move to next point
                if (_pathQueue != null && _pathQueue.Count > 0)
                {
                    _moveTarget = _pathQueue.Dequeue();
                }
            }
        }
    }

    // Call this to move the player along a path of nodes (from AStarPathfinding)
    public void FollowPath(System.Collections.Generic.List<Node> path)
    {
        if (path == null || path.Count == 0)
            return;

        _pathQueue = new Queue<Vector3>();
        foreach (var node in path)
        {
            Vector3 pos = GridManager.Instance.GridToWorld(new Vector2Int(node.row, node.col));
            _pathQueue.Enqueue(new Vector3(pos.x, transform.position.y, pos.z));
        }
        // Start moving to the first point
        if (_pathQueue.Count > 0)
        {
            _moveTarget = _pathQueue.Dequeue();
        }
    }
    public void SpawnPlayer(Vector3 position)
    {
        transform.position = position;
        _moveTarget = null;
        _pathQueue = null;
    }
}
