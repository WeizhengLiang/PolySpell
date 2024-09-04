using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject[] tutorialPanels;
    [SerializeField] private Toggle dontShowAgainToggle;
    [SerializeField] private Button nextTutorialPanelBtn;
    [SerializeField] private Button preTutorialPanelBtn;
    [SerializeField] private Button skipTutorialBtn;

    private int currentTutorialPanelIndex = 0;
    private bool isTutorialActive = false;

    public event Action OnTutorialStarted;
    public event Action OnTutorialEnded;

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

    private void InitializeButtonListeners()
    {
        if (nextTutorialPanelBtn != null) nextTutorialPanelBtn.onClick.AddListener(ShowNextTutorialPanel);
        if (preTutorialPanelBtn != null) preTutorialPanelBtn.onClick.AddListener(ShowPreviousTutorialPanel);
        if (skipTutorialBtn != null) skipTutorialBtn.onClick.AddListener(SkipTutorial);
        if (dontShowAgainToggle != null) dontShowAgainToggle.onValueChanged.AddListener(OnDontShowAgainToggleChanged);
    }

    public void StartTutorial()
    {
        if (PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.dontShowTutorial, 0) == 0)
        {
            isTutorialActive = true;
            if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
            currentTutorialPanelIndex = 0;
            ShowCurrentTutorialPanel();
            Time.timeScale = 0f;
            OnTutorialStarted?.Invoke();
        }
        else
        {
            EndTutorial();
        }
    }

    public void EndTutorial()
    {
        isTutorialActive = false;
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        Time.timeScale = 1f;
        OnTutorialEnded?.Invoke();
    }

    private void ShowNextTutorialPanel()
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

    private void ShowPreviousTutorialPanel()
    {
        currentTutorialPanelIndex = Mathf.Max(currentTutorialPanelIndex - 1, 0);
        ShowCurrentTutorialPanel();
    }

    private void ShowCurrentTutorialPanel()
    {
        if (tutorialPanels == null || tutorialPanels.Length == 0) return;

        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            if (tutorialPanels[i] != null)
            {
                tutorialPanels[i].SetActive(i == currentTutorialPanelIndex);
            }
        }

        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        if (preTutorialPanelBtn != null) preTutorialPanelBtn.gameObject.SetActive(currentTutorialPanelIndex > 0);
        if (nextTutorialPanelBtn != null) nextTutorialPanelBtn.gameObject.SetActive(currentTutorialPanelIndex <= tutorialPanels.Length - 1);
        if (skipTutorialBtn != null) skipTutorialBtn.gameObject.SetActive(currentTutorialPanelIndex < tutorialPanels.Length - 1);
    }

    private void OnDontShowAgainToggleChanged(bool value)
    {
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.dontShowTutorial, value ? 1 : 0);
    }

    public void SkipTutorial()
    {
        if (isTutorialActive)
        {
            EndTutorial();
        }
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    public void ResetTutorial()
    {
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.dontShowTutorial, 0);
        if (dontShowAgainToggle != null) dontShowAgainToggle.isOn = false;
    }
}
