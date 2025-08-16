using UnityEngine;

public class GameOverState : IGameState
{

    public void Enter()
    {
        GameManager.Instance.ShowGameOverUI();
    }

    public void Update()
    {
    }

    public void Exit()
    {
        GameManager.Instance.HideGameOverUI();
    }
}