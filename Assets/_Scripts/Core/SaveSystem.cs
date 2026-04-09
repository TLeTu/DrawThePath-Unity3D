using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    public int highestUnlockedLevel = 0; // Index of the highest unlocked level (0-based)
    public List<int> bestScores = new List<int>(); // Best score for each level by index
    public List<int> stars = new List<int>(); // Stars earned for each level (0-3)
}

public static class SaveSystem
{
    private const string PlayerProgressKey = "player_progress_v1";

    public static PlayerProgress Load()
    {
        if (PlayerPrefs.HasKey(PlayerProgressKey))
        {
            try
            {
                string json = PlayerPrefs.GetString(PlayerProgressKey);
                var data = JsonUtility.FromJson<PlayerProgress>(json);
                if (data == null)
                {
                    data = new PlayerProgress();
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load player progress: {e.Message}. Resetting progress.");
                return new PlayerProgress();
            }
        }
        // No existing progress, start fresh
        return new PlayerProgress();
    }

    public static void Save(PlayerProgress data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PlayerProgressKey, json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save player progress: {e.Message}");
        }
    }

    public static void EnsureCapacity(PlayerProgress data, int totalLevels)
    {
        if (data == null) return;
        if (totalLevels < 0) totalLevels = 0;
        while (data.bestScores.Count < totalLevels)
        {
            data.bestScores.Add(0);
        }
        while (data.stars.Count < totalLevels)
        {
            data.stars.Add(0);
        }
        // Clamp highestUnlockedLevel into valid range [0, totalLevels-1] when there are levels
        if (totalLevels > 0)
        {
            if (data.highestUnlockedLevel < 0) data.highestUnlockedLevel = 0;
            if (data.highestUnlockedLevel >= totalLevels) data.highestUnlockedLevel = totalLevels - 1;
        }
        else
        {
            data.highestUnlockedLevel = 0;
        }
    }

    public static void UpdateLevelResult(PlayerProgress data, int levelIndex, int score, int starsEarned, int totalLevels)
    {
        if (data == null) return;
        EnsureCapacity(data, totalLevels);
        if (levelIndex < 0 || levelIndex >= totalLevels) return;

        // Update best score for this level
        if (levelIndex < data.bestScores.Count)
        {
            if (score > data.bestScores[levelIndex])
            {
                data.bestScores[levelIndex] = score;
            }
        }

        // Update stars for this level
        if (levelIndex < data.stars.Count)
        {
            if (starsEarned > data.stars[levelIndex])
            {
                data.stars[levelIndex] = starsEarned;
            }
        }

        // Unlock next level (linear progression)
        int nextLevel = levelIndex + 1;
        if (totalLevels > 0)
        {
            int maxIndex = Mathf.Clamp(totalLevels - 1, 0, int.MaxValue);
            if (nextLevel > data.highestUnlockedLevel)
            {
                data.highestUnlockedLevel = Mathf.Min(nextLevel, maxIndex);
            }
        }
    }

    public static int GetTotalStars(PlayerProgress data)
    {
        if (data == null || data.stars == null) return 0;
        
        int totalStars = 0;
        foreach (int star in data.stars)
        {
            totalStars += star;
        }
        return totalStars;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(PlayerProgressKey);
    }
}
