using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private PlayerPathVisualizer pathVisualizer;
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
                else
                {
                    // Path finished, hide path visualizer
                    if (pathVisualizer != null)
                    {
                        pathVisualizer.HidePath();
                    }
                    _currentPathWorldPositions = null;
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
        _currentPathWorldPositions = new List<Vector3>();
        foreach (var node in path)
        {
            Vector3 pos = GridManager.Instance.GridToWorld(new Vector2Int(node.row, node.col));
            Vector3 worldPos = new Vector3(pos.x, transform.position.y, pos.z);
            _pathQueue.Enqueue(worldPos);
            _currentPathWorldPositions.Add(new Vector3(pos.x, pos.y, pos.z)); // Slightly above ground
        }
        // Show path visualizer
        if (pathVisualizer != null && _currentPathWorldPositions.Count > 1)
        {
            pathVisualizer.ShowPath(_currentPathWorldPositions);
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
        float tileTopY = GridManager.Instance.GetTileSize() * 0.5f;
        float playerHeight = 1f;
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            playerHeight = renderer.bounds.size.y;
        }
        position.y = tileTopY + playerHeight * 0.5f;
        Debug.Log($"Spawning player at: {position} (playerHeight={playerHeight})");
        transform.position = position;
        _moveTarget = null;
        _pathQueue = null;
    }
    // ontriggerenter if the object collides with tag "Obstacle" respawn the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Player collided with an obstacle, respawning...");
            //call the level manager to respawn the player
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RespawnPlayer();
            }
            // stop the current path
            _moveTarget = null;
            _pathQueue = null;
            if (pathVisualizer != null)
            {
                pathVisualizer.HidePath();
            }
            // set the tile to walkable
            Vector2Int obstacleCoords = GridManager.Instance.WorldToGrid(other.transform.position);
            Node obstacleNode = GridManager.Instance.GetNode(obstacleCoords.x, obstacleCoords.y);
            if (obstacleNode != null)
            {
                obstacleNode.SetWalkable(true);
                Destroy(other.gameObject);
                Debug.Log($"Obstacle at {obstacleCoords} set to walkable.");
            }

        }
    }

}
