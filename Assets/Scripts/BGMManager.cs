using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    public AudioSource bgmSource;

    public SlowMotionManager SlowMotionManager;

    public float pitchTransitionSpeed = 0.5f; // Adjust this value for smoother or quicker transitions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (Mathf.Abs(bgmSource.pitch - Time.timeScale) > 0.01f && SlowMotionManager.inSlowMotion)
        {
            bgmSource.pitch = Mathf.MoveTowards(bgmSource.pitch, Time.timeScale, pitchTransitionSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            bgmSource.pitch = Time.timeScale;
        }
    }

    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource.time > 0)
        {
            bgmSource.UnPause();
        }
        else
        {
            PlayBGM();
        }
    }
}