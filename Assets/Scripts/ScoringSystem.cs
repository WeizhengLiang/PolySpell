using TMPro;
using UnityEngine;
using System;

public class ScoringSystem : MonoBehaviour
{
    private int currentScore;
    private int highScore;

    public event Action<Tuple<int, int>> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    void Start()
    {
        currentScore = 0;
        highScore = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore, 0);
        OnScoreChanged?.Invoke(new Tuple<int, int>(currentScore, highScore));
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        OnScoreChanged?.Invoke(new Tuple<int, int>(currentScore, highScore));

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.HighScore, highScore);
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetHighScore()
    {
        return highScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(new Tuple<int, int>(currentScore, highScore));
    }

    public static string GetScoreLevel(int score)
    {
        if (score >= (int)ScoreLevel.S) return "S";
        if (score >= (int)ScoreLevel.A) return "A";
        if (score >= (int)ScoreLevel.B) return "B";
        if (score >= (int)ScoreLevel.C) return "C";
        if (score >= (int)ScoreLevel.D) return "D";
        if (score >= (int)ScoreLevel.F) return "F";
        return "F";
    }

    public void RefreshHighestScoreLevelText(TextMeshProUGUI scoreLevelText)
    {
        scoreLevelText.text = GetScoreLevel(highScore);
    }
}

public enum ScoreLevel
{
    S = 120,
    A = 100,
    B = 80,
    C = 60,
    D = 40,
    F = 20,
}