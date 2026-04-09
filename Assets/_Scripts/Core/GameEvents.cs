using System;
using UnityEngine;

public static class GameEvents
{
    // --- Game Flow Events ---
    // Fired by UI to start a specific level
    public static event Action<int> OnStartGameRequested;
    public static void TriggerStartGameRequested(int levelIndex) => OnStartGameRequested?.Invoke(levelIndex);

    // Fired by GameManager when the game/level actually begins
    public static event Action OnGameStarted;
    public static void TriggerGameStarted() => OnGameStarted?.Invoke();

    // Fired by GameManager when the player wins the level
    public static event Action<int, int> OnGameWin; // int: score
    public static void TriggerGameWin(int score, int stars) => OnGameWin?.Invoke(score, stars);

    // Fired by GameManager when the player runs out of lives or time
    public static event Action OnGameOver;
    public static void TriggerGameOver() => OnGameOver?.Invoke();

    // Fired by UI to request a level retry
    public static event Action OnRetryLevelRequested;
    public static void TriggerRetryLevelRequested() => OnRetryLevelRequested?.Invoke();

    // Fired by UI to request playing the next level
    public static event Action OnPlayNextLevelRequested;
    public static void TriggerPlayNextLevelRequested() => OnPlayNextLevelRequested?.Invoke();

    // Fired by UI to go back to the level selection menu
    public static event Action OnGoToLevelsMenuRequested;
    public static void TriggerGoToLevelsMenuRequested() => OnGoToLevelsMenuRequested?.Invoke();

    // Fired by GameManager to signal cleanup for the level
    public static event Action OnEndLevel;
    public static void TriggerEndLevel() => OnEndLevel?.Invoke();

    // --- Player Events ---
    // Fired by PlayerController on collision
    public static event Action<GameObject> OnPlayerCollision;
    public static void TriggerPlayerCollision(GameObject other) => OnPlayerCollision?.Invoke(other);

    // --- UI Events ---
    // Fired by various managers to switch UI screens and music
    public static event Action OnShowMainMenu;
    public static void TriggerShowMainMenu() => OnShowMainMenu?.Invoke();

    public static event Action OnShowLevelsMenu;
    public static void TriggerShowLevelsMenu() => OnShowLevelsMenu?.Invoke();

    public static event Action OnShowInGameHUD;
    public static void TriggerShowInGameHUD() => OnShowInGameHUD?.Invoke();

    // --- Audio Events ---
    // Fired when audio is toggled on/off
    public static event Action<bool> OnAudioToggled;
    public static void TriggerAudioToggled(bool isAudioOn) => OnAudioToggled?.Invoke(isAudioOn);
}
