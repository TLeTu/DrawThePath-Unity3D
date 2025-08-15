using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

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

    // Method to load a level by name
    public void LoadLevel(string levelName)
    {
        Debug.Log($"Loading level: {levelName}");
        // Implement level loading logic here
        // Initialize the grid and player position
        if (GridManager.Instance != null)
        {
            GridManager.Instance.InitializeGrid(8, 8); // Example dimensions
        }
    }

    // Method to reset the current level
    public void ResetLevel()
    {
        Debug.Log("Resetting current level");
        // Implement level reset logic here
    }
}