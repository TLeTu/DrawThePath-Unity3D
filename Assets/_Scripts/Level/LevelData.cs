using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class IntArrayWrapper
{
    public int[] row;
}

[System.Serializable]
public class EnemyData
{
    public int startX;
    public int startY;
    public int endX;
    public int endY;
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
    public List<EnemyData> enemies;
}
