using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    private static readonly WaitForSeconds WaitOneSecond = new WaitForSeconds(1f);

    [System.Serializable]
    public class BallTypeInfo
    {
        public BallType type;
        public ObjectPool pool;
        public int initialCount;
        public float spawnWeight;
    }

    public enum BallType { Normal, Evil }

    public List<BallTypeInfo> ballTypes;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -4.5f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 4.5f);
    [SerializeField] private float initialSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float spawnIntervalDecreaseRate = 0.05f;

    private float currentSpawnInterval;
    private float spawnTimer;
    private bool isSpawning = false;
    private float difficultyFactor = 1f;

    public event Action OnSpawningCompleted;

    public void StartSpawning()
    {
        LogManager.Instance.Log("BallSpawner: StartSpawning called");
        isSpawning = true;
        currentSpawnInterval = initialSpawnInterval;
        spawnTimer = 0f;
        SpawnInitialBalls();
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void ResetSpawner()
    {
        StopSpawning();
        StopAllCoroutines();
        ResetObjectPools();
        difficultyFactor = 1f;
    }

    public void HandleSpawning()
    {
        if (!isSpawning) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentSpawnInterval)
        {
            SpawnRandomBall();
            spawnTimer = 0f;
            IncreaseDifficulty();
        }
    }

    private void SpawnInitialBalls()
    {
        foreach (var ballInfo in ballTypes)
        {
            for (int i = 0; i < ballInfo.initialCount; i++)
            {
                SpawnBall(ballInfo.type);
            }
        }
    }

    private void SpawnRandomBall()
    {
        float totalWeight = 0f;
        foreach (var ballInfo in ballTypes)
        {
            totalWeight += ballInfo.spawnWeight * difficultyFactor;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var ballInfo in ballTypes)
        {
            currentWeight += ballInfo.spawnWeight * difficultyFactor;
            if (randomValue <= currentWeight)
            {
                SpawnBall(ballInfo.type);
                return;
            }
        }
    }

    private void SpawnBall(BallType type)
    {
        Vector2 spawnPosition = GetRandomSpawnPosition();
        BallTypeInfo ballInfo = ballTypes.Find(info => info.type == type);
        
        if (ballInfo != null)
        {
            GameObject ball = ballInfo.pool.GetObject();
            LogManager.Instance.Log($"BallSpawner: Spawning {type} ball at position {spawnPosition}");
            ball.transform.position = spawnPosition;

            switch (type)
            {
                case BallType.Normal:
                    InitializeNormalBall(ball);
                    break;
                case BallType.Evil:
                    InitializeEvilBall(ball);
                    break;
            }

            StartCoroutine(SpawnBallWithAnimation(ball, spawnPosition, type));
        }
    }

    private void InitializeNormalBall(GameObject ball)
    {
        NormalBall normalBall = ball.GetComponent<NormalBall>();
        if (normalBall != null)
        {
            normalBall.Initialize(false);  // 默认为系统生成
        }
    }

    private void InitializeEvilBall(GameObject ball)
    {
        EvilBall evilBall = ball.GetComponent<EvilBall>();
        if (evilBall != null)
        {
            LogManager.Instance.Log($"BallSpawner: Initializing EvilBall. Player null: {playerTransform == null}");
            evilBall.player = playerTransform;
            evilBall.Initialize();
        }
    }

    private IEnumerator SpawnBallWithAnimation(GameObject ball, Vector2 position, BallType type)
    {
        ball.SetActive(false);
        VFX vfx = null;

        if (type == BallType.Normal)
        {
            NormalBall normalBall = ball.GetComponent<NormalBall>();
            if (normalBall != null)
            {
                vfx = normalBall.TriggerSpawnVFX(position);
            }
        }
        else if (type == BallType.Evil)
        {
            vfx = VFXManager.Instance.SpawnVFX(VFXType.RedSpawningEffect, position);
        }

        yield return WaitOneSecond;  

        if (vfx != null)
        {
            VFXManager.Instance.DeActivate(vfx);
        }

        ball.SetActive(true);

        // 重新初始化 EvilBall
        if (type == BallType.Evil)
        {
            EvilBall evilBall = ball.GetComponent<EvilBall>();
            if (evilBall != null)
            {
                evilBall.player = playerTransform;
                evilBall.Initialize();
                LogManager.Instance.Log($"BallSpawner: Re-initialized EvilBall after animation. Player null: {playerTransform == null}");
            }
        }
    }

    private void IncreaseDifficulty()
    {
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - spawnIntervalDecreaseRate, minSpawnInterval);
        difficultyFactor += 0.05f;  // 每次生成后略微增加难度
    }

    public void AdjustDifficulty(float playerPerformance)
    {
        // 根据玩家表现调整难度
        difficultyFactor = Mathf.Clamp(difficultyFactor * playerPerformance, 0.5f, 2f);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        return new Vector2(
            UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
    }

    private void ResetObjectPools()
    {
        foreach (var ballInfo in ballTypes)
        {
            ballInfo.pool.ReturnAllObjects();
        }
    }

    public void ConvertTrailToNormalBall(Vector2 position)
    {
        GameObject ball = ballTypes.Find(info => info.type == BallType.Normal).pool.GetObject();
        ball.transform.position = position;
        NormalBall normalBall = ball.GetComponent<NormalBall>();
        if (normalBall != null)
        {
            normalBall.Initialize(true);  // 设置为由trail转化
            StartCoroutine(SpawnBallWithAnimation(ball, position, BallType.Normal));
        }
    }

    public void SetPlayerReference(Transform player)
    {
        LogManager.Instance.Log($"BallSpawner: Setting player reference. Player null: {player == null}");
        playerTransform = player;
        UpdateAllEvilBallsPlayerReference();
    }

    private void UpdateAllEvilBallsPlayerReference()
    {
        foreach (var ballInfo in ballTypes)
        {
            if (ballInfo.type == BallType.Evil)
            {
                foreach (var ball in ballInfo.pool.activeObjList)
                {
                    EvilBall evilBall = ball.GetComponent<EvilBall>();
                    if (evilBall != null)
                    {
                        evilBall.player = playerTransform;
                        LogManager.Instance.Log($"BallSpawner: Updated player reference for existing EvilBall");
                    }
                }
            }
        }
    }
}