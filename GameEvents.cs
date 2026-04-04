using System;
using UnityEngine;

public static class GameEvents
{
    // --- Game Flow Events ---
    // Fired by UI to start a specific level
    public static event Action<int> OnStartGameRequested;
    // Fired by GameManager when the game/level actually begins
    public static event Action OnGameStarted;
    // Fired by GameManager when the player wins the level
    public static event Action<int> OnGameWin; // int: score
    // Fired by GameManager when the player runs out of lives or time
    public static event Action OnGameOver;
    // Fired by UI to request a level retry
    public static event Action OnRetryLevelRequested;
    // Fired by UI to request playing the next level
    public static event Action OnPlayNextLevelRequested;
    // Fired by UI to go back to the level selection menu
    public static event Action OnGoToLevelsMenuRequested;
    // Fired by GameManager to signal cleanup for the level
    public static event Action OnEndLevel;

    // --- Player Events ---
    // Fired by PlayerController on collision
    public static event Action<GameObject> OnPlayerCollision;

    // --- UI Events ---
    // Fired by various managers to switch UI screens and music
    public static event Action OnShowMainMenu;
    public static event Action OnShowLevelsMenu;
    public static event Action OnShowInGameHUD;
}