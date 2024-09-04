using UnityEngine;
using System.Collections.Generic;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }
    [SerializeField] private AudioSource[] soundEffects;

    private Dictionary<int, AudioSource> soundEffectDict = new Dictionary<int, AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            InitializeSoundEffects();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundEffects()
    {
        if (soundEffects == null || soundEffects.Length == 0)
        {
            Debug.LogWarning("No sound effects assigned to SoundEffectManager.");
            return;
        }

        for (int i = 0; i < soundEffects.Length; i++)
        {
            if (soundEffects[i] != null)
            {
                soundEffectDict[i] = soundEffects[i];
            }
        }
    }

    public void ToggleSFX(bool isOn)
    {
        PlayerPrefsManager.Instance.SaveInt(PlayerPrefsKeys.SfxOn, isOn ? 1 : 0);
        SetMute(!isOn);
    }

    public void PlaySoundEffect(int index)
    {
        if (!IsSFXEnabled() || !soundEffectDict.TryGetValue(index, out AudioSource audioSource)) return;
        audioSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (!IsSFXEnabled() || clip == null) return;
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

    public void StopSoundEffect(int index)
    {
        if (!soundEffectDict.TryGetValue(index, out AudioSource audioSource)) return;
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        foreach (var audioSource in soundEffectDict.Values)
        {
            audioSource.volume = volume;
        }
    }

    private void SetMute(bool isMuted)
    {
        foreach (var audioSource in soundEffectDict.Values)
        {
            audioSource.mute = isMuted;
        }
    }

    public bool IsSoundEffectPlaying(int index)
    {
        return soundEffectDict.TryGetValue(index, out AudioSource audioSource) && audioSource.isPlaying;
    }

    private bool IsSFXEnabled()
    {
        return PlayerPrefsManager.Instance.LoadInt(PlayerPrefsKeys.SfxOn, 1) == 1;
    }
}