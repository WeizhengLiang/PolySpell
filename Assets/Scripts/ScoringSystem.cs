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
        scoreText.text = $"{currentScore} <size=42><color=#9399a3>/ 96</size></color>";
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
}