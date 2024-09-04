using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameRunning => stateManager.CurrentState == GameStateManager.GameState.Playing;
    public bool IsPaused => stateManager.CurrentState == GameStateManager.GameState.Paused;

    [SerializeField] private GameStateManager stateManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ScoringSystem scoringSystem;
    [SerializeField] private BallSpawner ballSpawner;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private SoundEffectManager soundEffectManager;
    [SerializeField] private BGMManager bgmManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeManagers();
    }

    private void InitializeManagers()
    {
        stateManager.OnGameStateChanged += HandleGameStateChanged;
        timeManager.OnTimeUp += EndGame;
        timeManager.OnTimeUpdate += UpdateTimerUI;
        playerManager.OnPlayerDeath += EndGame;
        tutorialManager.OnTutorialEnded += StartGame;
        uiManager.OnStartButtonClicked += HandleStartButtonClicked;
        uiManager.OnHomeButtonClicked += ReturnToMainMenu;
        uiManager.OnReplayButtonClicked += RestartGame;
        uiManager.OnContinueButtonClicked += ResumeGame;
        uiManager.OnSettingsButtonClicked += OpenSettings;
        uiManager.OnQuitButtonClicked += QuitGame;
        uiManager.OnBgmToggleClicked += ToggleBGM;
        scoringSystem.OnScoreChanged += uiManager.UpdateInGameScoreText;
    }

    private void Start()
    {
        bgmManager.PlayBGM();
        stateManager.ChangeState(GameStateManager.GameState.MainMenu);
    }

    private void Update()
    {
        if (stateManager.CurrentState == GameStateManager.GameState.Playing)
        {
            timeManager.UpdateTimer();
            ballSpawner.HandleSpawning();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        switch (stateManager.CurrentState)
        {
            case GameStateManager.GameState.Playing:
                PauseGame();
                break;
            case GameStateManager.GameState.Paused:
                ResumeGame();
                break;
            case GameStateManager.GameState.MainMenu:
                ToggleSettingsPanel();
                break;
            case GameStateManager.GameState.Tutorial:
                tutorialManager.SkipTutorial();
                break;
        }
    }

    private void ToggleSettingsPanel()
    {
        if (uiManager.IsSettingsPanelOpen())
        {
            uiManager.CloseSettingPanel();
        }
        else
        {
            uiManager.OpenSettingPanel();
        }
    }

    private void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.MainMenu:
                uiManager.ShowMainMenu();
                break;
            case GameStateManager.GameState.Playing:
                uiManager.ShowGameUI();
                break;
            case GameStateManager.GameState.Paused:
                uiManager.ShowPauseMenu();
                break;
            case GameStateManager.GameState.GameOver:
                uiManager.ShowEndScreen();
                break;
            default:
                Debug.LogWarning($"Unhandled game state: {newState}");
                break;
        }
    }

    private void HandleStartButtonClicked()
    {
        if (PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.dontShowTutorial, 0) == 0)
        {
            tutorialManager.StartTutorial();
        }
        else
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        ResetGameData();
        stateManager.ChangeState(GameStateManager.GameState.Playing);
        timeManager.StartTimer(60f);
        playerManager.InitializeOrSpawnPlayer(Vector3.zero);
        ballSpawner.StartSpawning();
    }

    private void EndGame()
    {
        stateManager.ChangeState(GameStateManager.GameState.GameOver);
        ballSpawner.StopSpawning();
        uiManager.UpdateFinalScore(scoringSystem.GetCurrentScore());
    }

    private void PauseGame()
    {
        stateManager.ChangeState(GameStateManager.GameState.Paused);
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        stateManager.ChangeState(GameStateManager.GameState.Playing);
        Time.timeScale = 1f;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ResetGameData();
        stateManager.ChangeState(GameStateManager.GameState.MainMenu);
    }

    private void RestartGame()
    {
        StopAllCoroutines();
        ResetGameData();
        StartGame();
    }

    private void ResetGameData()
    {
        scoringSystem.ResetScore();
        playerManager.ResetPlayers();
        ballSpawner.ResetSpawner();
        timeManager.ResetTimer();
        VFXManager.Instance.DeActivateAll(); // 添加这行来清除所有特效
    }

    private void OpenSettings()
    {
        uiManager.OpenSettingPanel();
    }

    private void UpdateTimerUI(float time)
    {
        uiManager.UpdateTimer(time);
    }

    private void ToggleBGM(bool isOn)
    {
        bgmManager.ToggleBGM(isOn);
    }

    private void ToggleSfx(bool isOn)
    {
        
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        stateManager.OnGameStateChanged -= HandleGameStateChanged;
        timeManager.OnTimeUp -= EndGame;
        timeManager.OnTimeUpdate -= UpdateTimerUI;
        playerManager.OnPlayerDeath -= EndGame;
        tutorialManager.OnTutorialEnded -= StartGame;
        uiManager.OnStartButtonClicked -= HandleStartButtonClicked;
        uiManager.OnHomeButtonClicked -= ReturnToMainMenu;
        uiManager.OnReplayButtonClicked -= RestartGame;
        uiManager.OnContinueButtonClicked += ResumeGame;
        uiManager.OnSettingsButtonClicked -= OpenSettings;
        uiManager.OnQuitButtonClicked -= QuitGame;
    }
}