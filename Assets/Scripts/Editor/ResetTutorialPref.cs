using UnityEditor;
using UnityEngine;

public class ResetTutorialPref : MonoBehaviour
{
    [MenuItem("Tools/Reset Tutorial Preference")]
    private static void ResetTutorialPreference()
    {
        PlayerPrefs.SetInt("FirstTimePlaying", 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial preference reset. It will show on the next game start.");
    }
}