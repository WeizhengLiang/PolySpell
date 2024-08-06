using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public ScoringSystem ScoringSystem;
    public PlayerController playerController;  // Reference to the PlayerController
    public BallSpawner BallSpawner;
    public GameObject mainMenu;
    public GameObject gameUI;
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI timerText;  // Reference to the timer text UI element
    public float gameDuration = 60f;
    
    //MAIN MENU CANVAS
    public Button StartBtn;
    
    //IN-GAME CANVAS
    
    //END MENU CANVAS
    public Button HomeBtn;
    public Button ReplayBtn;
    public Button NextLevelBtn;

    [HideInInspector]
    public bool isGameRunning = false;
    
    private float gameTimer = 0f;
    private int score = 0;
    private float[] spawnIntervals;
    private int spawnCount = 3;
    private int spawnedCount = 0;

    void Awake()
    {
        StartBtn.onClick.AddListener(OnClickStart);
        HomeBtn.onClick.AddListener(OnClickHome);
        ReplayBtn.onClick.AddListener(OnClickReplay);
        NextLevelBtn.onClick.AddListener(OnClickNextLevel);
    }

    void Start()
    {
        playerController.HealthSystem.onHealthZero += EndGame;  // Subscribe to the health zero event
        ShowMainMenu();
    }
    
    void OnDestroy()
    {
        playerController.HealthSystem.onHealthZero -= EndGame;  // Unsubscribe from the health zero event
        StartBtn.onClick.RemoveListener(OnClickStart);
        HomeBtn.onClick.RemoveListener(OnClickHome);
        ReplayBtn.onClick.RemoveListener(OnClickReplay);
        NextLevelBtn.onClick.RemoveListener(OnClickNextLevel);
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
            UpdateTimerUI();
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
            BallSpawner.SpawnInitialBalls(5, 1);
            spawnedCount++;
        }
    }
    
    void UpdateTimerUI()
    {
        timerText.text = Mathf.Ceil(gameTimer).ToString();
    }

    public void StartGame()
    {
        ResetGameData();
        gameTimer = gameDuration;
        isGameRunning = true;
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        endScreen.SetActive(false);
        InitializeSpawnIntervals();  // Initialize spawn intervals for the game
        BallSpawner.SpawnInitialBalls(playerController.initialSpawnCount, 3);
    }

    public void EndGame()
    {
        isGameRunning = false;
        finalScoreText.text = ScoringSystem.GetFinalScore().ToString();
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
        ResetGameData();
    }

    private void ResetGameData()
    {
        ScoringSystem.ResetScore();  // Reset score on returning to main menu
        playerController.ResetPlayer();  // Reset player state if necessary
        BallSpawner.ResetObjectPools();
        UpdateTimerUI(); 
    }

    private void OnClickStart()
    {
        StartGame();
    }

    private void OnClickHome()
    {
        ReturnToMainMenu();
    }

    private void OnClickReplay()
    {
        StartGame();
    }
    
    private void OnClickNextLevel()
    {
        
    }
    
}