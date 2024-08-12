using TMPro;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private int currentScore;

    void Start()
    {
        currentScore = 0;
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"{currentScore} <size=42><color=#9399a3>/ {PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore, 0)}</size></color>";
        
    }
    
    public int GetFinalScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
    }

    public string GetScoreLevel(int score)
    {
        if (score >= (int)ScoreLevel.S)
        {
            return "S";
        }
        if (score >= (int)ScoreLevel.A)
        {
            return "A";
        }
        if (score >= (int)ScoreLevel.B)
        {
            return "B";
        }
        if (score >= (int)ScoreLevel.C)
        {
            return "C";
        }
        if (score >= (int)ScoreLevel.D)
        {
            return "D";
        }
        if (score >= (int)ScoreLevel.F)
        {
            return "F";
        }

        return "F";
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