using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private TextMeshProUGUI scoreLevelText;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button bgmToggle;
    [SerializeField] private GameObject bgmOn;
    [SerializeField] private GameObject bgmOff;
    [SerializeField] private Button sfxToggle;
    [SerializeField] private GameObject sfxOn;
    [SerializeField] private GameObject sfxOff;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button closeSettingBtn;

    [Header("Confirm Quit Panel")]
    [SerializeField] private GameObject confirmQuitPanel;
    [SerializeField] private Button confirmQuitBtn;
    [SerializeField] private Button cancelQuitBtn;

    [Header("In-Game UI")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private TextMeshProUGUI InGameScoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseHomeBtn;
    [SerializeField] private Button pauseContinueBtn;
    [SerializeField] private Button pauseReplayBtn;

    [Header("End Menu")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private Button homeBtn;
    [SerializeField] private Button replayBtn;
    [SerializeField] private TextMeshProUGUI endMenuFinalScoreText;
    [SerializeField] private TextMeshProUGUI endMenuGameTimeDurationText;
    [SerializeField] private TextMeshProUGUI endMenuCoinText;

    public event Action OnStartButtonClicked;
    public event Action OnSettingsButtonClicked;
    public event Action OnHomeButtonClicked;
    public event Action OnReplayButtonClicked;
    public event Action OnContinueButtonClicked;
    public event Action OnQuitButtonClicked;
    public event Action<bool> OnBgmToggleClicked;
    public event Action OnSfxToggleClicked;

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

        InitializeButtonListeners();
    }

    private void Start()
    {
        
        InitializeUIFromPlayerPrefs();
    }

    private void InitializeButtonListeners()
    {
        if (startBtn != null) startBtn.onClick.AddListener(() => OnStartButtonClicked?.Invoke());
        if (settingBtn != null) settingBtn.onClick.AddListener(() => { OpenSettingPanel(); OnSettingsButtonClicked?.Invoke(); });
        if (closeSettingBtn != null) closeSettingBtn.onClick.AddListener(CloseSettingPanel);
        if (quitBtn != null) quitBtn.onClick.AddListener(ShowConfirmQuitPanel);
        if (confirmQuitBtn != null) confirmQuitBtn.onClick.AddListener(() => OnQuitButtonClicked?.Invoke());
        if (cancelQuitBtn != null) cancelQuitBtn.onClick.AddListener(HideConfirmQuitPanel);
        if (pauseHomeBtn != null) pauseHomeBtn.onClick.AddListener(() => OnHomeButtonClicked?.Invoke());
        if (pauseContinueBtn != null) pauseContinueBtn.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());
        if (pauseReplayBtn != null) pauseReplayBtn.onClick.AddListener(() => OnReplayButtonClicked?.Invoke());
        if (homeBtn != null) homeBtn.onClick.AddListener(() => OnHomeButtonClicked?.Invoke());
        if (replayBtn != null) replayBtn.onClick.AddListener(() => OnReplayButtonClicked?.Invoke());
        if (bgmToggle != null) bgmToggle.onClick.AddListener(() => { ToggleBGM(); OnBgmToggleClicked?.Invoke(bgmOn.activeSelf); });
        if (sfxToggle != null) sfxToggle.onClick.AddListener(()=> { ToggleSFX(); OnSfxToggleClicked?.Invoke(); });
    }

    private void InitializeUIFromPlayerPrefs()
    {
        // 初始化 BGM 开关状态
        var bgmIsOn = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.BgmOn, 1) == 1;
        SetActiveState(bgmOn, !bgmIsOn);

        // 初始化 SFX 开关状态
        var sfxIsOn = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.SfxOn, 1) == 1;
        SetActiveState(sfxOn, !sfxIsOn);

        // 更新主菜单上的最高分
        var highScore = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore, 0);
        UpdateScoreLevel(highScore);
    }

    public void ShowMainMenu()
    {
        SetActiveState(mainMenu, true);
        SetActiveState(gameUI, false);
        SetActiveState(pausePanel, false);
        SetActiveState(endScreen, false);
        SetActiveState(settingsPanel, false);
        SetActiveState(confirmQuitPanel, false);
        
        var highestScore = PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.HighScore, 0);
        UpdateScoreLevel(highestScore);
    }

    public void ShowGameUI()
    {
        SetActiveState(mainMenu, false);
        SetActiveState(gameUI, true);
        SetActiveState(pausePanel, false);
        SetActiveState(endScreen, false);
        SetActiveState(settingsPanel, false);
        SetActiveState(confirmQuitPanel, false);
    }

    public void ShowPauseMenu()
    {
        SetActiveState(pausePanel, true);
    }

    public void HidePauseMenu()
    {
        SetActiveState(pausePanel, false);
    }

    public void ShowEndScreen()
    {
        SetActiveState(endScreen, true);
        SetActiveState(gameUI, false);
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null) timerText.text = Mathf.Ceil(time).ToString();
    }

    public void UpdateFinalScore(int score)
    {
        if (endMenuFinalScoreText != null) endMenuFinalScoreText.text = score.ToString();
    }

    public void UpdateScoreLevel(int score)
    {
        if (scoreLevelText != null)
        {
            scoreLevelText.text = ScoringSystem.GetScoreLevel(score);
        }
    }
    
    public void UpdateInGameScoreText(Tuple<int, int> dataTuple)
    {
        InGameScoreText.text = $"{dataTuple.Item1} <size=42><color=#9399a3>/ {dataTuple.Item2}</size></color>";
    }

    public void OpenSettingPanel()
    {
        SetActiveState(settingsPanel, true);
    }

    public void CloseSettingPanel()
    {
        SetActiveState(settingsPanel, false);
    }

    private void ShowConfirmQuitPanel()
    {
        SetActiveState(confirmQuitPanel, true);
    }

    private void HideConfirmQuitPanel()
    {
        SetActiveState(confirmQuitPanel, false);
    }

    private void ToggleBGM()
    {
        bool isOn = !bgmOn.activeSelf;
        SetActiveState(bgmOn, isOn);
        SetActiveState(bgmOff, !isOn);
        // BGMManager.Instance.ToggleBGM(isOn);
    }

    private void ToggleSFX()
    {
        bool isOn = !sfxOn.activeSelf;
        SetActiveState(sfxOn, isOn);
        SetActiveState(sfxOff, !isOn);
        // SoundEffectManager.Instance.ToggleSFX(isOn);
    }

    private void SetActiveState(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }

    public bool IsSettingsPanelOpen()
    {
        return settingsPanel.activeSelf;
    }
}