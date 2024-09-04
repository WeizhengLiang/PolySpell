using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float pitchTransitionSpeed = 0.5f;
    
    private SlowMotionManager slowMotionManager;

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
        }
    }
    
    public void Initialize(SlowMotionManager slowMotionManager)
    {
        this.slowMotionManager = slowMotionManager;
    }
    
    private void Update()
    {
        UpdateBGMPitch();
    }

    private void UpdateBGMPitch()
    {
        if (bgmSource == null || slowMotionManager == null) return;

        if (Mathf.Abs(bgmSource.pitch - Time.timeScale) > 0.01f && slowMotionManager.inSlowMotion)
        {
            bgmSource.pitch = Mathf.MoveTowards(bgmSource.pitch, Time.timeScale, pitchTransitionSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            bgmSource.pitch = Time.timeScale;
        }
    }

    public void ToggleBGM(bool isOn)
    {
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.BgmOn, isOn ? 1 : 0);
        if (isOn)
            PlayBGM();
        else
            StopBGM();
    }

    public void PlayBGM()
    {
        if (bgmSource == null) return;
        if (PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.BgmOn, 1) == 1 && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        if (bgmSource == null) return;
        bgmSource.volume = volume;
    }

    public void PauseBGM()
    {
        if (bgmSource == null) return;
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource == null) return;
        if (bgmSource.time > 0)
        {
            bgmSource.UnPause();
        }
        else
        {
            PlayBGM();
        }
    }

    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
}