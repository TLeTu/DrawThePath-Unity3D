using UnityEngine;

[System.Serializable]
public class Node
{
    
    public bool wall;
    public int row, col;
    public int gCost;
    public int hCost;
    public int tile;
    public Node parent;
    
    // Unity-specific properties
    public Vector3 worldPosition;
    
    public Node(bool wall, int row, int col, int tile, Vector3 worldPosition = default)
    {
        this.wall = wall;
        this.row = row;
        this.col = col;
        this.tile = tile;
        this.worldPosition = worldPosition;
        
        gCost = 0;
        hCost = 0;
        parent = null;
    }
    
    public int FCost
    {
        get { return hCost + gCost; }
    }
    
    // Unity-specific methods
    public bool IsWalkable()
    {
        return !wall;
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
            
        Node other = (Node)obj;
        return row == other.row && col == other.col;
    }
    
    public override int GetHashCode()
    {
        return row.GetHashCode() ^ col.GetHashCode();
    }
}
