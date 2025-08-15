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
}
