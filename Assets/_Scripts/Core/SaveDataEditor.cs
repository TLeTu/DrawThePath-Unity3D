using UnityEditor;
using UnityEngine;

public class SaveDataEditor
{
    [MenuItem("Tools/Save System/Print Save Data")]
    public static void PrintSaveData()
    {
        string key = "player_progress_v1"; // Matches PlayerProgressKey in SaveSystem.cs
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            Debug.Log($"<b>Save Data JSON:</b>\n{json}");
        }
        else
        {
            Debug.LogWarning("No save data found!");
        }
    }

    [MenuItem("Tools/Save System/Clear Save Data")]
    public static void ClearSaveData()
    {
        SaveSystem.ResetAll();
        Debug.Log("Save data cleared!");
    }
}