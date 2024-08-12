using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public ScoringSystem ScoringSystem;
    public PlayerController playerController;  // Reference to the PlayerController
    public BallSpawner BallSpawner;

    //---------------------MAIN MENU CANVAS---------------------
    public GameObject mainMenu;
    public Button StartBtn;
    public Button SettingBtn;
    public TextMeshProUGUI ScoreLevelText;
    
    // Setting panel
    public GameObject settingsPanel; // Added: Reference to Settings panel
    public Button bgmToggle;
    public GameObject bgmOn;
    public GameObject bgmOff;
    public Button sfxToggle;
    public GameObject sfxOn;
    public GameObject sfxOff;
    public Button Quit;
    public Button CloseSetting;


    // Confirm Quit Panel
    public GameObject confirmQuitPanel; // Added: Reference to Confirm Quit panel
    public Button ConfirmQuit;
    public Button CancelQuit;
    
    //---------------------IN-GAME CANVAS---------------------
    public GameObject gameUI;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI timerText;
    
    //PAUSE PANEL
    public GameObject pausePanel;
    public Button pauseHomeBtn;
    public Button pauseContinueBtn;
    public Button pauseReplayBtn;

    //---------------------END MENU CANVAS---------------------
    public GameObject endScreen;
    public Button HomeBtn;
    public Button ReplayBtn;

    //---------------------TUTORIAL CANVAS---------------------
    public GameObject tutorialCanvas;
    public GameObject[] tutorialPanels;
    public Toggle DontShowAgainToggle;
    public Button nextTutorialPanelBtn;
    public Button preTutorialPanelBtn;
    
    
    
    public float gameDuration = 60f;
    private bool isPaused = false;
    private bool isTutorialActive = false;
    private int currentTutorialPanelIndex = 0;

    public bool IsPaused => isPaused;

    private float originalTimeScale;

    [HideInInspector]
    public bool isGameRunning = false;
    
    private float gameTimer = 0f;
    private float[] spawnIntervals;
    private int spawnCount = 2;
    private int spawnedCount = 0;

    void Awake()
    {
        StartBtn.onClick.AddListener(OnClickStart);
        HomeBtn.onClick.AddListener(OnClickHome);
        ReplayBtn.onClick.AddListener(OnClickReplay);
        pauseHomeBtn.onClick.AddListener(OnClickHome);
        pauseContinueBtn.onClick.AddListener(ResumeGame);
        pauseReplayBtn.onClick.AddListener(OnClickReplay);
        
        SettingBtn.onClick.AddListener(OnClickSettingBtn);
        CloseSetting.onClick.AddListener(OnClickCloseSetting);
        Quit.onClick.AddListener(OnClickQuitBtn);
        ConfirmQuit.onClick.AddListener(OnClickConfirmQuitBtn);
        CancelQuit.onClick.AddListener(OnClickCancelQuitBtn);
        bgmToggle.onClick.AddListener(OnClickBGMToggle);
        sfxToggle.onClick.AddListener(OnClickSfxToggle);
        DontShowAgainToggle.onValueChanged.AddListener(OnClickDontShowTutorialToggle);
        nextTutorialPanelBtn.onClick.AddListener(OnClickNextTutorialPanelBtn);
        preTutorialPanelBtn.onClick.AddListener(OnClickPreviousTutorialPanelBtn);
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
        
        SettingBtn.onClick.RemoveListener(OnClickSettingBtn);
        CloseSetting.onClick.RemoveListener(CloseSettingPanel);
        Quit.onClick.RemoveListener(OnClickQuitBtn);
        ConfirmQuit.onClick.RemoveListener(OnClickConfirmQuitBtn);
        CancelQuit.onClick.RemoveListener(OnClickCancelQuitBtn);
        bgmToggle.onClick.RemoveListener(OnClickBGMToggle);
        sfxToggle.onClick.RemoveListener(OnClickSfxToggle);
        DontShowAgainToggle.onValueChanged.RemoveListener(OnClickDontShowTutorialToggle);
        nextTutorialPanelBtn.onClick.RemoveListener(OnClickNextTutorialPanelBtn);
        preTutorialPanelBtn.onClick.RemoveListener(OnClickPreviousTutorialPanelBtn);
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
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isTutorialActive)
                {
                    SkipTutorial(); 
                } else if (mainMenu.activeSelf)
                {
                    if(settingsPanel.activeSelf) CloseSettingPanel();
                    else OpenSettingPanel();
                }
            }
        }
    }
    
    void InitializeSpawnIntervals()
    {
        spawnIntervals = new float[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            spawnIntervals[i] = Random.Range(gameDuration / (2f + i), gameDuration);
        }
        System.Array.Sort(spawnIntervals);
        System.Array.Reverse(spawnIntervals);
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
        spawnedCount = 0;
        isGameRunning = true;
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        endScreen.SetActive(false);
        InitializeSpawnIntervals();  // Initialize spawn intervals for the game
        BallSpawner.SpawnInitialBalls(10, 2);
    }

    public void EndGame()
    {
        isGameRunning = false;
        if(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore) < ScoringSystem.GetFinalScore())
            PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.HighScore, ScoringSystem.GetFinalScore());
        finalScoreText.text = ScoringSystem.GetFinalScore().ToString();
        RefreshHighestScoreLevelText();
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
        RefreshHighestScoreLevelText();
        mainMenu.SetActive(true);
        gameUI.SetActive(false);
        endScreen.SetActive(false);
        ResetGameData();
    }

    private void ResetGameData()
    {
        spawnedCount = 0;
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
        if (PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.dontShowTutorial, 0) == 0)
        {
            ShowTutorial();  // added: Show tutorial if first time playing
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
        DontShowAgainToggle.isOn = false;
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
    
    private void ShowPreviousTutorialPanel()  // added: Method to advance to the next tutorial panel
    {
        currentTutorialPanelIndex = Math.Max(currentTutorialPanelIndex - 1, 0);
        if (currentTutorialPanelIndex == 0)
        {
            
        }
        ShowCurrentTutorialPanel();
        
    }

    private void EndTutorial()  // added: Method to end the tutorial
    {
        foreach (GameObject panel in tutorialPanels)
        {
            panel.SetActive(false);
        }
        Time.timeScale = 1f;  // Resume the game
        isTutorialActive = false;
        tutorialCanvas.SetActive(isTutorialActive);
        
        StartGame();
    }

    private void CloseSettingPanel()
    {
        settingsPanel.SetActive(false);
    }
    
    private void OpenSettingPanel()
    {
        settingsPanel.SetActive(true);
        
        sfxOn.SetActive(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.SfxOn) == 1);
        sfxOff.SetActive(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.SfxOn) == 0);
        bgmOn.SetActive(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.BgmOn) == 1);
        bgmOff.SetActive(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.BgmOn) == 0);
    }

    private void OnClickCloseSetting()
    {
        CloseSettingPanel();
    }

    private void OnClickSettingBtn()
    {
        OpenSettingPanel();
    }

    private void OnClickBGMToggle()
    {
        Debug.Log("clicked bgm");
        var current = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.BgmOn);
        var become = current == 1 ? 0 : 1;
        bgmOn.SetActive(become == 1);
        bgmOff.SetActive(become == 0);
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.BgmOn, become);
    }
    
    private void OnClickSfxToggle()
    {
        Debug.Log("clicked sfx");
        var current = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.SfxOn);
        var become = current == 1 ? 0 : 1;
        sfxOn.SetActive(become == 1);
        sfxOff.SetActive(become == 0);
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.SfxOn, become);
    }

    private void OnClickQuitBtn()
    {
        confirmQuitPanel.SetActive(true);
    }

    private void OnClickConfirmQuitBtn()
    {
#if UNITY_EDITOR
        // If running in the Unity Editor, exit play mode
        EditorApplication.isPlaying = false;
#else
        // If running in a build, quit the application
        Application.Quit();
#endif
    }

    private void OnClickCancelQuitBtn()
    {
        confirmQuitPanel.SetActive(false);
    }

    private void OnClickDontShowTutorialToggle(bool arg0)
    {
        DontShowAgainToggle.isOn = arg0;
        var dontShowTutorial = arg0 ? 1 : 0;
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.dontShowTutorial, dontShowTutorial);
    }

    private void OnClickNextTutorialPanelBtn()
    {
        if (tutorialCanvas.activeSelf)
        {
            ShowNextTutorialPanel();
        }
    }
    
    private void OnClickPreviousTutorialPanelBtn()
    {
        if (tutorialCanvas.activeSelf)
        {
            ShowPreviousTutorialPanel();
        }
    }

    private void RefreshHighestScoreLevelText()
    {
        ScoreLevelText.text = ScoringSystem.GetScoreLevel(PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore));
    }

}