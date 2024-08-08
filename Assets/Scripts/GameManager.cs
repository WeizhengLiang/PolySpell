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
    public GameObject pausePanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI timerText;  // Reference to the timer text UI element
    public float gameDuration = 60f;
    
    //MAIN MENU CANVAS
    public Button StartBtn;
    
    //IN-GAME CANVAS
    
    //END MENU CANVAS
    public Button HomeBtn;
    public Button ReplayBtn;
    
    //PAUSE PANEL
    public Button pauseHomeBtn;
    public Button pauseContinueBtn;
    public Button pauseReplayBtn;
    
    private bool isPaused = false;

    public bool IsPaused
    {
        get => isPaused;
        private set {  }
    }

    private float originalTimeScale;

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
        pauseHomeBtn.onClick.AddListener(OnClickHome);
        pauseContinueBtn.onClick.AddListener(ResumeGame);
        pauseReplayBtn.onClick.AddListener(OnClickReplay);
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
        pauseHomeBtn.onClick.RemoveListener(OnClickHome);
        pauseContinueBtn.onClick.RemoveListener(ResumeGame);
        pauseReplayBtn.onClick.RemoveListener(OnClickReplay);
    }

    void Update()
    {
        if (isGameRunning)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
            
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
        // ResetGameData();
        gameTimer = gameDuration;
        isGameRunning = true;
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        endScreen.SetActive(false);
        InitializeSpawnIntervals();  // Initialize spawn intervals for the game
        BallSpawner.SpawnInitialBalls(playerController.initialSpawnCount, 1);
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
        StopAllCoroutines();
        BallSpawner.StopSpawning();
        VFXManager.Instance.DeActivateAll();
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
        if (isPaused)
        {
            ResumeGame();
        }
        ReturnToMainMenu();
    }

    private void OnClickReplay()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        ResetGameData();
        StartGame();
    }
    
    private void OnClickNextLevel()
    {
        
    }
    
    void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Stop all game activity
        isPaused = true;
        pausePanel.SetActive(true); // Show pause panel
    }

    public void ResumeGame()
    {
        Time.timeScale = originalTimeScale; // Resume original time scale
        isPaused = false;
        pausePanel.SetActive(false); // Hide pause panel
    }
    
}