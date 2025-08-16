using UnityEngine;

public class GameWinState : IGameState
{

    public void Enter()
    {
        GameManager.Instance.ShowGameWinUI();
    }

    public void Update()
    {
    }

    public void Exit()
    {
        GameManager.Instance.HideGameWinUI();
    }
}