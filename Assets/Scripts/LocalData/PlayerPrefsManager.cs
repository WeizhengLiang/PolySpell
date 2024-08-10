using System;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    // Singleton Instance
    private static PlayerPrefsManager _instance;
    public static PlayerPrefsManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePlayerPrefsDefaults();
    }

    // Save an integer value
    public void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    // Load an integer value
    public int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    // Save a float value
    public void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    // Load a float value
    public float LoadFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    // Save a string value
    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    // Load a string value
    public string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    // Delete a specific key
    public void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    // Delete all saved keys
    public void DeleteAllKeys()
    {
        PlayerPrefs.DeleteAll();
    }

    // Check if a key exists
    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
    
    
    private void InitializePlayerPrefsDefaults()
    {
        // Check if the key exists, if not, set it to a default value
        if (!PlayerPrefs.HasKey(PlayerPrefsKeys.HighScore))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.HighScore, 0);
        }
        
        if (!PlayerPrefs.HasKey(PlayerPrefsKeys.FirstTimePlaying))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.FirstTimePlaying, 0);
        }
        
        if (!PlayerPrefs.HasKey(PlayerPrefsKeys.dontShowTutorial))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.dontShowTutorial, 0);
        }
        
        if (!PlayerPrefs.HasKey(PlayerPrefsKeys.BgmOn))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.BgmOn, 1);
        }
        
        if (!PlayerPrefs.HasKey(PlayerPrefsKeys.SfxOn))
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.SfxOn, 1);
        }

        // if (!PlayerPrefs.HasKey(PlayerPrefsKeys.LastScore))
        // {
        //     PlayerPrefs.SetInt(PlayerPrefsKeys.LastScore, 0);
        // }
        //
        // if (!PlayerPrefs.HasKey(PlayerPrefsKeys.PlayerName))
        // {
        //     PlayerPrefs.SetString(PlayerPrefsKeys.PlayerName, "Player");
        // }
        //
        // if (!PlayerPrefs.HasKey(PlayerPrefsKeys.BgmVolume))
        // {
        //     PlayerPrefs.SetFloat(PlayerPrefsKeys.BgmVolume, 1.0f);
        // }
        //
        // if (!PlayerPrefs.HasKey(PlayerPrefsKeys.SfxVolume))
        // {
        //     PlayerPrefs.SetFloat(PlayerPrefsKeys.SfxVolume, 1.0f);
        // }
        //
        // if (!PlayerPrefs.HasKey(PlayerPrefsKeys.DifficultyLevel))
        // {
        //     PlayerPrefs.SetInt(PlayerPrefsKeys.DifficultyLevel, 1); // 1 for easy, 2 for medium, 3 for hard
        // }

        PlayerPrefs.Save(); // Save the defaults
    }
}