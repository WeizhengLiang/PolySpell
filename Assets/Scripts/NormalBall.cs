using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;  // Speed of the normal ball
    
    public enum PowerUpType { None, Size, Speed, Trail}
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

    public void Initialize(bool fromTrait = false)
    {
        AssignPowerUp(fromTrait);
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        direction = Random.insideUnitCircle.normalized;
        rb.velocity = direction * speed;
        switch (powerUp)
        {
            case PowerUpType.Trail:
                spriteRenderer.color = Color.white;
                break;
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
    
    void AssignPowerUp(bool fromTrait = false)
    {
        if (fromTrait)
        {
            powerUp = PowerUpType.Trail;
            return;
        }
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
        if (gameObject.activeSelf) // only when it ran out of view
        {
            ReturnToPool();
        }
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
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.killEffectYellow ,VFXManager.Instance.killEffectYellowPrefab, pos, 1f);
                break;
            case PowerUpType.Speed:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.killEffectPurple ,VFXManager.Instance.killEffectPurplePrefab, pos, 1f);
                break;
            case PowerUpType.None:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.killEffectBlue ,VFXManager.Instance.killEffectBluePrefab, pos, 1f);
                break;
            case PowerUpType.Trail:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.killEffectWhite ,VFXManager.Instance.killEffectWhitePrefab, pos, 1f);
                break;
        }
    }
    
    public void TriggerSpawnVFX(Vector2 pos)
    {
        switch (powerUp)
        {
            case PowerUpType.Size:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.YelloSpawningEffect ,VFXManager.Instance.YelloSpawningEffectPrefab, pos, 1f);
                break;
            case PowerUpType.Speed:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.PurpleSpawningEffect ,VFXManager.Instance.PurpleSpawningEffectPrefab, pos, 1f);
                break;
            case PowerUpType.None:
                VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.BlueSpawningEffect ,VFXManager.Instance.BlueSpawningEffectPrefab, pos, 1f);
                break;
        }
    }
}
