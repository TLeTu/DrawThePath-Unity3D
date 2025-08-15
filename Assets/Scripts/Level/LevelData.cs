using UnityEngine;

[System.Serializable]
public class IntArrayWrapper
{
    public int[] row;
}

[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public IntArrayWrapper[] tiles;
    // Start tile grid coordinates
    public int startTileX;
    public int startTileY;
    // End tile grid coordinates
    public int endTileX;
    public int endTileY;
}
