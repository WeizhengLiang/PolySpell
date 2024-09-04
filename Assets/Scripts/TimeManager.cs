using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float GameDuration { get; private set; }
    public float CurrentTime { get; private set; }

    public event Action OnTimeUp;
    public event Action<float> OnTimeUpdate;

    public void StartTimer(float duration)
    {
        GameDuration = duration;
        CurrentTime = duration;
    }

    public void UpdateTimer()
    {
        if (CurrentTime > 0)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                OnTimeUp?.Invoke();
            }
            OnTimeUpdate.Invoke(CurrentTime);
        }
    }

    public void ResetTimer()
    {   
        CurrentTime = 0;
    }
}