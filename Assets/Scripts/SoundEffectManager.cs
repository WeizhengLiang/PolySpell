using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;
    public AudioSource[] soundEffects;
    
    public SlowMotionManager SlowMotionManager;

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

    public void PlaySoundEffect(int index)
    {
        if (index >= 0 && index < soundEffects.Length)
        {
            soundEffects[index].Play();
        }
    }

    public void StopSoundEffect(int index)
    {
        if (index >= 0 && index < soundEffects.Length)
        {
            soundEffects[index].Stop();
        }
    }

    public void SetVolume(float volume)
    {
        foreach (AudioSource sound in soundEffects)
        {
            sound.volume = volume;
        }
    }
}