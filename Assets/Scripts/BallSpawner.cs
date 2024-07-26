using System.Collections;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject normalBallSparklePrefab;  // Reference to the normal ball sparkle effect prefab
    public GameObject evilBallSparklePrefab;    // Reference to the evil ball sparkle effect prefab

    public ObjectPool normalBallPool;
    public ObjectPool evilBallPool;
    public Transform playerTransform;  // Reference to the player's transform

    public void SpawnInitialBalls(int normalBallCount, int evilBallCount)
    {
        SpawnNormalBalls(normalBallCount);
        SpawnEvilBalls(evilBallCount);
    }

    public IEnumerator SpawnNormalBallWithAnimation(Vector2 position)
    {
        GameObject sparkle = Instantiate(normalBallSparklePrefab, position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);  // Duration of the sparkle animation

        Destroy(sparkle);

        SpawnNormalBall(position);
    }

    public IEnumerator SpawnEvilBallWithAnimation(Vector2 position)
    {
        GameObject sparkle = Instantiate(evilBallSparklePrefab, position, Quaternion.identity);
        yield return new WaitForSeconds(1.5f);  // Duration of the sparkle animation

        Destroy(sparkle);

        SpawnEvilBall(position);
    }
    
    public void SpawnNormalBalls(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-8f, 8f), Random.Range(-4.5f, 4.5f));
            StartCoroutine(SpawnNormalBallWithAnimation(spawnPosition));
        }
    }
    
    public void SpawnNormalBall(Vector2 spawnPosition)
    {
        GameObject ball = normalBallPool.GetObject();
        ball.transform.position = spawnPosition;
        ball.GetComponent<NormalBall>().Initialize();
    }
    
    public void SpawnEvilBalls(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-8f, 8f), Random.Range(-4.5f, 4.5f));
            StartCoroutine(SpawnEvilBallWithAnimation(spawnPosition));
        }
    }

    public void SpawnEvilBall(Vector2 spawnPosition)
    {
        GameObject ball = evilBallPool.GetObject();
        ball.transform.position = spawnPosition;
            
        EvilBall evilBall = ball.GetComponent<EvilBall>();
        if (evilBall != null)
        {
            evilBall.player = playerTransform;  // Set the player reference
        }
        
        evilBall.Initialize();
    }
}