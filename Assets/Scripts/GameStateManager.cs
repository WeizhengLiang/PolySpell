using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState { MainMenu, Playing, Paused, GameOver, Tutorial }
    
    public GameState CurrentState { get; private set; } = GameState.MainMenu; // 设置默认状态为 MainMenu

    public event Action<GameState> OnGameStateChanged;

    private void Start()
    {
        // 确保在游戏开始时触发状态变化事件
        OnGameStateChanged?.Invoke(CurrentState);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
    }
}