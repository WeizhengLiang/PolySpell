using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;  // Speed of the normal ball
    public Sprite NormalBallPrefab;
    public Sprite SizeBallPrefab;
    public Sprite SpeedBallPrefab;
    
    public enum PowerUpType { None, Size, Speed, Trail}
    public PowerUpType powerUp;
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI Text;

    private Rigidbody2D rb;

    private Vector2 direction;
    void Start()
    {
        // Debug.Log("start");
        // Initialize();
    }

    private void OnEnable()
    {
        Debug.Log("onenable");
        rb = GetComponent<Rigidbody2D>();
        direction = Random.insideUnitCircle.normalized;
        rb.velocity = direction * speed;
    }
    

    void Update()
    {
        // Move the normal ball in the assigned direction
        // transform.Translate(direction * speed * Time.deltaTime);
    }

    public void Initialize(bool fromTrait = false)
    {
        AssignPowerUp(fromTrait);
        spriteRenderer = GetComponent<SpriteRenderer>();
        switch (powerUp)
        {
            case PowerUpType.Trail:
                spriteRenderer.color = Color.white;
                spriteRenderer.sprite = NormalBallPrefab;
                break;
            case PowerUpType.Size:
                spriteRenderer.color = new Color(0.9374589f, 1f, 0.4198113f);
                spriteRenderer.sprite = SizeBallPrefab;
                break;
            case PowerUpType.Speed:
                spriteRenderer.color = new Color(0.8780197f, 0.4196079f, 1f);
                spriteRenderer.sprite = SpeedBallPrefab;
                break;
            case PowerUpType.None:
                spriteRenderer.color = new Color(0f, 0.7149544f, 1f);
                spriteRenderer.sprite = NormalBallPrefab;
                break;
        }
    }
    
    void AssignPowerUp(bool fromTrait = false)
    {
        if (fromTrait)
        {
            powerUp = PowerUpType.Trail;
            Debug.Log("Assigned PowerUp: Trail");
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
        Debug.Log("Assigned PowerUp: " + powerUp);
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
    
    public VFX TriggerSpawnVFX(Vector2 pos)
    {
        VFX vfx;
        switch (powerUp)
        {
            case PowerUpType.Size:
                vfx = VFXManager.Instance.SpawnVFX(VFXType.YelloSpawningEffect ,VFXManager.Instance.YelloSpawningEffectPrefab, pos);
                break;
            case PowerUpType.Speed:
                vfx = VFXManager.Instance.SpawnVFX(VFXType.PurpleSpawningEffect ,VFXManager.Instance.PurpleSpawningEffectPrefab, pos);
                break;
            case PowerUpType.None:
                vfx = VFXManager.Instance.SpawnVFX(VFXType.BlueSpawningEffect ,VFXManager.Instance.BlueSpawningEffectPrefab, pos);
                break;
            case PowerUpType.Trail:
                vfx = VFXManager.Instance.SpawnVFX(VFXType.WhiteSpawningEffect ,VFXManager.Instance.WhiteSpawningEffectPrefab, pos);
                break;
            default:
                vfx = VFXManager.Instance.SpawnVFX(VFXType.BlueSpawningEffect ,VFXManager.Instance.BlueSpawningEffectPrefab, pos);
                break;
        }
        Debug.Log("detect PowerUp: " + powerUp);
        return vfx;
    }
}
