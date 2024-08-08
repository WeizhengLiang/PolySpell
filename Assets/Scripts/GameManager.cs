using System;
using TMPro;
using UnityEditor;
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
    public GameObject tutorialCanvas;
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
    
    // tutorial
    public GameObject[] tutorialPanels;
    
    private bool isPaused = false;
    private bool isTutorialActive = false; 
    private int currentTutorialPanelIndex = 0;

    public bool IsPaused
    {
        get => isPaused;
        private set {  }
    }

    private float originalTimeScale;

    [HideInInspector]
    public bool isGameRunning = false;
    
    private float gameTimer = 0f;
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
        if (isTutorialActive && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipTutorial();  // added: Skip tutorial if player presses Esc
        }
        if (isTutorialActive && Input.GetMouseButtonDown(0))
        {
            ShowNextTutorialPanel();  // added: Advance to the next tutorial panel on left mouse click
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
        // Check if it's the player's first time playing
        if (PlayerPrefs.GetInt("FirstTimePlaying", 1) == 1)
        {
            ShowTutorial();  // added: Show tutorial if first time playing
            PlayerPrefs.SetInt("FirstTimePlaying", 0);  // Update to indicate player has seen the tutorial
        }
        else
        {
            StartGame();
        }
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
    
    private void ShowTutorial()  // modified: Method to show the tutorial panels
    {
        isTutorialActive = true;
        tutorialCanvas.SetActive(isTutorialActive);
        Time.timeScale = 0f;  // Pause the game while tutorial is active
        currentTutorialPanelIndex = 0;  // Start from the first panel
        ShowCurrentTutorialPanel();
    }

    private void SkipTutorial()  // added: Method to skip the tutorial
    {
        tutorialCanvas.SetActive(false);  // Hide tutorial panel
        Time.timeScale = 1f;  // Resume the game
        isTutorialActive = false;
    }
    
    private void ShowCurrentTutorialPanel()  // added: Method to show the current tutorial panel
    {
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            tutorialPanels[i].SetActive(i == currentTutorialPanelIndex);
        }
    }

    private void ShowNextTutorialPanel()  // added: Method to advance to the next tutorial panel
    {
        currentTutorialPanelIndex++;
        if (currentTutorialPanelIndex < tutorialPanels.Length)
        {
            ShowCurrentTutorialPanel();
        }
        else
        {
            EndTutorial();
        }
    }

    private void EndTutorial()  // added: Method to end the tutorial
    {
        foreach (GameObject panel in tutorialPanels)
        {
            panel.SetActive(false);
        }
        Time.timeScale = 1f;  // Resume the game
        isTutorialActive = false;
        
        StartGame();
    }
    
    [MenuItem("Tools/Reset Tutorial Preference")]
    private static void ResetTutorialPreference()
    {
        PlayerPrefs.SetInt("FirstTimePlaying", 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial preference reset. It will show on the next game start.");
    }
    
}