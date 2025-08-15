using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    public static AStarPathfinding Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // Reset all nodes before pathfinding
        ResetNodes();
        
        Vector2Int startGrid = GridManager.Instance.WorldToGrid(startPos);
        Vector2Int targetGrid = GridManager.Instance.WorldToGrid(targetPos);
        
        return FindPath(startGrid.x, startGrid.y, targetGrid.x, targetGrid.y);
    }
    
    public List<Node> FindPath(int startRow, int startCol, int targetRow, int targetCol)
    {
        if (!IsValidPosition(startRow, startCol) || !IsValidPosition(targetRow, targetCol))
        {
            Debug.LogWarning("Invalid start or target position for pathfinding");
            return new List<Node>();
        }
        
        Node startNode = GridManager.Instance.GetNode(startRow, startCol);
        Node targetNode = GridManager.Instance.GetNode(targetRow, targetCol);
        
        if (targetNode.wall)
        {
            Debug.LogWarning("Target position is not walkable");
            return new List<Node>();
        }
        
        return FindPath(startNode, targetNode);
    }
    
    private List<Node> FindPath(Node start, Node target)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        
        openSet.Add(start);
        
        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            
            // Find node with lowest F cost
            for (int i = 1; i < openSet.Count; i++)
            {
                Node node = openSet[i];
                if (node.FCost < currentNode.FCost || 
                    (node.FCost == currentNode.FCost && node.hCost < currentNode.hCost))
                {
                    currentNode = node;
                }
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            // Path found
            if (currentNode.Equals(target))
            {
                return RetracePath(start, target);
            }
            
            // Check neighbors
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (neighbor.wall || closedSet.Contains(neighbor))
                {
                    continue;
                }
                
                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, target);
                    neighbor.parent = currentNode;
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        
        Debug.LogWarning("Path not found");
        return new List<Node>();
    }
    
    private List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currentNode = end;
        
        while (!currentNode.Equals(start))
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        return path;
    }
    
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.col - nodeB.col);
        int distY = Mathf.Abs(nodeA.row - nodeB.row);
        
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        
        return 14 * distX + 10 * (distY - distX);
    }
    
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        
        // Check 4-directional neighbors (up, down, left, right)
        int[] rowOffsets = { -1, 1, 0, 0 };
        int[] colOffsets = { 0, 0, -1, 1 };
        
        for (int i = 0; i < 4; i++)
        {
            int newRow = node.row + rowOffsets[i];
            int newCol = node.col + colOffsets[i];
            
            if (IsValidPosition(newRow, newCol))
            {
                neighbors.Add(GridManager.Instance.GetNode(newRow, newCol));
            }
        }
        
        return neighbors;
    }
    
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < GridManager.Instance.gridHeight && col >= 0 && col < GridManager.Instance.gridWidth;
    }
    
    public Node GetNode(int row, int col)
    {
        return GridManager.Instance.GetNode(row, col);
    }
    
    public bool IsWalkable(int row, int col)
    {
        return GridManager.Instance.IsWalkable(row, col);
    }
    
    // Reset all nodes for new pathfinding
    public void ResetNodes()
    {
        Node[,] grid = GridManager.Instance.GetGrid();
        for (int row = 0; row < GridManager.Instance.gridHeight; row++)
        {
            for (int col = 0; col < GridManager.Instance.gridWidth; col++)
            {
                grid[row, col].gCost = 0;
                grid[row, col].hCost = 0;
                grid[row, col].parent = null;
            }
        }
    }
}
