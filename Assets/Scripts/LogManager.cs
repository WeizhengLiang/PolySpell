using System.IO;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    private static LogManager _instance;
    public static LogManager Instance => _instance;

    private string logFilePath;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
            File.WriteAllText(logFilePath, ""); // Clear the file at the start
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Log(string message)
    {
        string logMessage = $"[{System.DateTime.Now}] {message}\n";
        File.AppendAllText(logFilePath, logMessage);
        Debug.Log(message);
    }
}