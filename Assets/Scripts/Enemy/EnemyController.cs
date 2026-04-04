using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    private Vector3 firstPosition;
    private Vector3 secondPosition;
    private bool movingToSecond = true;
    private System.Collections.Generic.List<Node> currentPath = null;
    private int pathIndex = 0;
    private float reachedThreshold = 0.05f;
    private bool _isGameRunning = false;

    private void OnEnable()
    {
        GameEvents.OnGameStarted += OnGameStarted;
        GameEvents.OnEndLevel += OnEndLevel;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= OnGameStarted;
        GameEvents.OnEndLevel -= OnEndLevel;
    }

    private void OnGameStarted() => _isGameRunning = true;
    private void OnEndLevel() => _isGameRunning = false;


    public void SetPosition(Vector3 firstPosition, Vector3 secondPosition)
    {
        this.transform.position = firstPosition;
        this.firstPosition = firstPosition;
        this.secondPosition = secondPosition;
        SetPathTo(secondPosition);
    }
    public void RemovePath()
    {
        currentPath = null;
        pathIndex = 0;
    }

    private void SetPathTo(Vector3 target)
    {
        currentPath = AStarPathfinding.Instance.FindPath(transform.position, target);
        pathIndex = 0;
    }

    private void Update()
    {
        if (!_isGameRunning)
        {
            return; // Ignore movement if the game is not running
        }

        if (currentPath == null || currentPath.Count == 0)
            return;

        // Move along the path
        Node targetNode = currentPath[pathIndex];
        Vector3 targetWorld = GridManager.Instance.GridToWorld(new Vector2Int(targetNode.row, targetNode.col));
        Vector3 targetPos = new Vector3(targetWorld.x, transform.position.y, targetWorld.z);

        // Face direction
        Vector3 moveDir = (targetPos - transform.position);
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.2f);
        }

        // Move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < reachedThreshold)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                // Reached end, reverse direction
                movingToSecond = !movingToSecond;
                SetPathTo(movingToSecond ? secondPosition : firstPosition);
            }
        }

    }
}