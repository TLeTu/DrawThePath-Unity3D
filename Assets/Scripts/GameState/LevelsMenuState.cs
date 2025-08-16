using UnityEngine;

public class LevelsMenuState : IGameState
{
    public void Enter()
    {
        GameManager.Instance.ShowLevelsMenu();

    }

    public void Update()
    {
    }

    public void Exit()
    {
        GameManager.Instance.HideLevelsMenu();
    }
}