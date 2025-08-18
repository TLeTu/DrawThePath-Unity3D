using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private PlayerPathVisualizer _pathVisualizer;
    [SerializeField] private Animator _animator;
    private List<Vector3> _currentPathWorldPositions = null;

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
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            if (_animator != null) _animator.SetBool("isMoving", false);
            return; // Ignore input if the game is not running
        }

        bool isActuallyMoving = false;
        if (_moveTarget.HasValue)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetXZ = _moveTarget.Value;
            Vector3 moveDirection = (targetXZ - currentPosition);
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                // Rotate to face the direction of movement
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
            float stopDistance = 0.05f; // Slightly larger buffer to avoid float issues
            if (Vector3.Distance(currentPosition, targetXZ) > stopDistance)
            {
                transform.position = Vector3.MoveTowards(currentPosition, targetXZ, _moveSpeed * Time.deltaTime);
                isActuallyMoving = true;
            }
            else
            {
                transform.position = targetXZ;
                // If following a path, move to next point
                if (_pathQueue != null && _pathQueue.Count > 0)
                {
                    _moveTarget = _pathQueue.Dequeue();
                    isActuallyMoving = true; // Will move to next point next frame
                }
                else
                {
                    _moveTarget = null;
                    // Path finished, hide path visualizer
                    if (_pathVisualizer != null)
                    {
                        _pathVisualizer.HidePath();
                    }
                    _currentPathWorldPositions = null;
                }
            }
        }
        if (_animator != null)
        {
            _animator.SetBool("isMoving", isActuallyMoving);
        }
    }

    // Call this to move the player along a path of nodes (from AStarPathfinding)
    public void FollowPath(System.Collections.Generic.List<Node> path)
    {
        if (path == null || path.Count == 0)
            return;

        _pathQueue = new Queue<Vector3>();
        _currentPathWorldPositions = new List<Vector3>();
        foreach (var node in path)
        {
            Vector3 pos = GridManager.Instance.GridToWorld(new Vector2Int(node.row, node.col));
            Vector3 worldPos = new Vector3(pos.x, transform.position.y, pos.z);
            _pathQueue.Enqueue(worldPos);
            _currentPathWorldPositions.Add(new Vector3(pos.x, pos.y, pos.z)); // Slightly above ground
        }
        // Show path visualizer
        if (_pathVisualizer != null && _currentPathWorldPositions.Count > 1)
        {
            _pathVisualizer.ShowPath(_currentPathWorldPositions);
        }
        // Start moving to the first point
        if (_pathQueue.Count > 0)
        {
            _moveTarget = _pathQueue.Dequeue();
        }
    }
    public void SpawnPlayer(Vector3 position)
    {
        // Set the player's y so that the feet are on top of the tile
        // Set the animator trigger Dead to false
        if (_animator != null)
        {
            _animator.SetBool("isDead", false);
        }
        float tileTopY = GridManager.Instance.GetTileSize() * 0.5f;
        float playerHeight = 1f;
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            playerHeight = renderer.bounds.size.y;
        }
        position.y = tileTopY;
        Debug.Log($"Spawning player at: {position} (playerHeight={playerHeight})");
        transform.position = position;
        _moveTarget = null;
        _pathQueue = null;
        gameObject.SetActive(true);

    }
    public void PlayDeadAnimation()
    {
        if (_animator != null)
        {
            _animator.SetBool("isDead", true);
        }
    }
    // ontriggerenter if the object collides with tag "Obstacle" respawn the player
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player collided with an obstacle, respawning...");

        // Play collision sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerCollisionSFX();
        }

        //call the game manager to respawn the player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UponPlayerCollision(other.gameObject);
        }
        // stop the current path
        _moveTarget = null;
        _pathQueue = null;
        if (_pathVisualizer != null)
        {
            _pathVisualizer.HidePath();
        }
    }
    public void Destroy()
    {
        _moveTarget = null;
        _pathQueue = null;
        if (_pathVisualizer != null)
        {
            _pathVisualizer.HidePath();
        }
        // Disable the player object
        gameObject.SetActive(false);
    }
}
