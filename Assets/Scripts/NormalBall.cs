using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;  // Speed of the normal ball
    
    public enum PowerUpType { None, Size, Speed }
    public PowerUpType powerUp;
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI Text;

    private Rigidbody2D rb;

    private Vector2 direction;
    void Start()
    {
        Initialize();
    }

    void Update()
    {
        // Move the normal ball in the assigned direction
        // transform.Translate(direction * speed * Time.deltaTime);
    }

    public void Initialize()
    {
        AssignPowerUp();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        direction = Random.insideUnitCircle.normalized;
        rb.velocity = direction * speed;
        switch (powerUp)
        {
            case PowerUpType.Size:
                spriteRenderer.color = Color.yellow;
                break;
            case PowerUpType.Speed:
                spriteRenderer.color = Color.magenta;
                break;
            case PowerUpType.None:
                spriteRenderer.color = Color.cyan;
                break;
        }
    }
    
    void AssignPowerUp()
    {
        float chance = Random.Range(0f, 1f);
        if (chance < 0.1f)  // 20% chance to be a power-up ball
        {
            powerUp = (PowerUpType)Random.Range(1, 3);
        }
        else
        {
            powerUp = PowerUpType.None;
        }
    }

    private void OnBecameInvisible()
    {
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        NormalBallPool.ReturnObject(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            Vector2 normal = (transform.position - other.transform.position).normalized;
            rb.velocity = Vector2.Reflect(rb.velocity, normal);
        }
    }

    public void TriggerDieVFX(Vector2 pos)
    {
        switch (powerUp)
        {
            case PowerUpType.Size:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.killEffectYellowPrefab, pos, 1f);
                break;
            case PowerUpType.Speed:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.killEffectPurplePrefab, pos, 1f);
                break;
            case PowerUpType.None:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.killEffectBluePrefab, pos, 1f);
                break;
        }
    }
    
    public void TriggerSpawnVFX(Vector2 pos)
    {
        switch (powerUp)
        {
            case PowerUpType.Size:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.YelloSpawningEffectPrefab, pos, 1f);
                break;
            case PowerUpType.Speed:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.PurpleSpawningEffectPrefab, pos, 1f);
                break;
            case PowerUpType.None:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXManager.Instance.BlueSpawningEffectPrefab, pos, 1f);
                break;
        }
    }
}
