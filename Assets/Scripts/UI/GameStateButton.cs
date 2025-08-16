using UnityEngine;

public class GameStateButton : MonoBehaviour
{
    public void ChangeToMainMenu()
    {
        // Change the game state to MainMenuState
        GameManager.Instance.ChangeGameState(new MainMenuState());
    }
    public void ChangeToLevelsMenu()
    {
        // Change the game state to LevelsMenuState
        GameManager.Instance.ChangeGameState(new LevelsMenuState());
    }
    public void ChangeToInGameState(int level)
    {
        // Change the game state to InGameState
        GameManager.Instance.ChangeGameState(new InGameState(level));
    }
}