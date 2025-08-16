using UnityEngine;

public class InGameState : IGameState
{
    private int _level;
    public InGameState(int level)
    {
        _level = level;
    }
    public void Enter()
    {
        GameManager.Instance.NewGame(_level);
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}