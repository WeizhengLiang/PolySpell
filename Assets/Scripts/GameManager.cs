using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public ScoringSystem ScoringSystem;
    public PlayerController playerController;  // Reference to the PlayerController
    public GameObject mainMenu;
    public GameObject gameUI;
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI timerText;  // Reference to the timer text UI element
    public float gameDuration = 60f;

    [HideInInspector]
    public bool isGameRunning = false;
    
    private float gameTimer = 0f;
    private int score = 0;
    private float[] spawnIntervals;
    private int spawnCount = 3;
    private int spawnedCount = 0;

    void Start()
    {
        ShowMainMenu();
        playerController.HealthSystem.onHealthZero += EndGame;  // Subscribe to the health zero event
    }
    
    void OnDestroy()
    {
        playerController.HealthSystem.onHealthZero -= EndGame;  // Unsubscribe from the health zero event
    }

    void Update()
    {
        if (isGameRunning)
        {
            gameTimer -= Time.deltaTime;
            if (gameTimer <= 0)
            {
                EndGame();
            }
            UpdateTimerText();
            HandleSpawning();
        }
    }
    
    void InitializeSpawnIntervals()
    {
        spawnIntervals = new float[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            spawnIntervals[i] = Random.Range(0f, gameDuration);
        }
        System.Array.Sort(spawnIntervals);
    }
    
    void HandleSpawning()
    {
        if (spawnedCount < spawnCount && gameTimer <= spawnIntervals[spawnedCount])
        {
            playerController.SpawnNormalBalls(5);  // Spawn 5 normal balls
            playerController.SpawnEvilBalls(1);  // Spawn 1 evil ball with random health
            spawnedCount++;
        }
    }
    
    void UpdateTimerText()
    {
        timerText.text = "Time: " + Mathf.Ceil(gameTimer).ToString();
    }

    public void StartGame()
    {
        gameTimer = gameDuration;
        isGameRunning = true;
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        endScreen.SetActive(false);
        ScoringSystem.ResetScore();  // Reset score at the start of the game
        UpdateTimerText();  // Initialize the timer text
        InitializeSpawnIntervals();  // Initialize spawn intervals for the game
        playerController.SpawnNormalBalls(playerController.initialSpawnCount);  // Spawn initial normal balls
        playerController.SpawnEvilBalls(3);  // Spawn initial evil balls with random health
    }

    public void EndGame()
    {
        isGameRunning = false;
        finalScoreText.text = "Final Score: " + ScoringSystem.GetFinalScore();
        gameUI.SetActive(false);
        endScreen.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        isGameRunning = false;
        mainMenu.SetActive(true);
        gameUI.SetActive(false);
        endScreen.SetActive(false);
        ScoringSystem.ResetScore();  // Reset score on returning to main menu
        playerController.ResetPlayer();  // Reset player state if necessary
        ResetObjectPools();
    }
    
    private void ResetObjectPools()
    {
        // Reset all object pools
        GameObject[] normalBalls = GameObject.FindGameObjectsWithTag("NormalBall");
        foreach (GameObject ball in normalBalls)
        {
            playerController.normalBallPool.ReturnObject(ball);
        }

        GameObject[] evilBalls = GameObject.FindGameObjectsWithTag("EvilBall");
        foreach (GameObject ball in evilBalls)
        {
            playerController.evilBallPool.ReturnObject(ball);
        }
    }
}