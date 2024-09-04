using System;
using System.Collections.Generic;
using Kalkatos.DottedArrow;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public event Action OnPlayerDeath;
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerUIPrefab; // PlayerUI 预制体
    [SerializeField] private Canvas inGameCanvas; // InGameCanvas 引用
    [SerializeField] private GameObject arrowPrefab; // 新增：箭头预制体
    private GameManager gameManager;
    private ScoringSystem scoringSystem;
    private SlowMotionManager slowMotionManager;
    private GameObject polygonVisualizerPrefab;
    private BallSpawner ballSpawner;

    private List<PlayerController> players = new List<PlayerController>();

    private void Start()
    {
        gameManager = GameManager.Instance;
        scoringSystem = FindObjectOfType<ScoringSystem>();
        slowMotionManager = FindObjectOfType<SlowMotionManager>();
        polygonVisualizerPrefab = Resources.Load<GameObject>("Prefabs/PolygonVisualizer");
        ballSpawner = FindObjectOfType<BallSpawner>();
    }

    public void SpawnPlayer(Vector3 position)
    {
        GameObject playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        EnergySystem energySystem = playerInstance.AddComponent<EnergySystem>();
        HealthSystem healthSystem = playerInstance.AddComponent<HealthSystem>();

        GameObject playerUIInstance = Instantiate(playerUIPrefab, inGameCanvas.transform);
        PlayerUI playerUI = playerUIInstance.GetComponent<PlayerUI>();

        Slider healthBar = inGameCanvas.transform.Find("HealthBar").GetComponentInChildren<Slider>();
        Slider energyBar = inGameCanvas.transform.Find("EnergyBar").GetComponentInChildren<Slider>();
        
        // 为每个玩家创建一个箭头
        Arrow arrow = Instantiate(arrowPrefab, inGameCanvas.transform).GetComponent<Arrow>();
        playerUI.InitializeGlobalUI(healthBar, energyBar, arrow);

        Canvas localCanvas = playerInstance.GetComponentInChildren<Canvas>();
        Image healthCircle = localCanvas.transform.Find("HealthCircle").GetComponent<Image>();
        Image energyCircle = localCanvas.transform.Find("EnergyCircle").GetComponent<Image>();

        playerUI.InitializeLocalUI(healthCircle, energyCircle);

        playerController.Initialize(gameManager, energySystem, healthSystem, scoringSystem, slowMotionManager, polygonVisualizerPrefab, ballSpawner, playerUI);
        
        players.Add(playerController);
        playerController.OnDeath += HandlePlayerDeath;

        LogManager.Instance.Log($"PlayerManager: Player spawned at position {position}");
    }

    private void HandlePlayerDeath(PlayerController player)
    {
        // players.Remove(player);
        OnPlayerDeath?.Invoke();
    }
    
    public void ResetPlayers()
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                player.gameObject.SetActive(false);
                player.ResetPlayer();
            }
        }
    }

    public Transform GetPlayerTransform()
    {
        if (players.Count > 0 && players[0] != null)
        {
            return players[0].transform;
        }
        return null;
    }

    public void InitializeOrSpawnPlayer(Vector3 position)
    {
        if (players.Count > 0 && players[0] != null)
        {
            // 如果已经有玩家，重新初始化并激活它
            PlayerController player = players[0];
            player.transform.position = position;
            player.gameObject.SetActive(true);
            player.ResetPlayer();
            LogManager.Instance.Log($"PlayerManager: Existing player reinitialized at position {position}");
        }
        else
        {
            // 如果没有玩家，创建一个新的
            SpawnPlayer(position);
        }

        // 更新 BallSpawner 中的玩家引用
        if (ballSpawner != null)
        {
            ballSpawner.SetPlayerReference(players[0].transform);
        }
    }
}