using UnityEngine;

public class MainMenuState : IGameState
{
   public void Enter()
   {
       GameManager.Instance.ShowMainMenu();
   }

   public void Update()
   {
       // Handle input or other updates while in the main menu
   }

   public void Exit()
   {
       GameManager.Instance.HideMainMenu();
   }
}