using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;
    public Sprite NormalBallPrefab;
    public Sprite SizeBallPrefab;
    public Sprite SpeedBallPrefab;
    
    public enum PowerUpType { None, Size, Speed, Trail}
    public PowerUpType powerUp;
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI Text;

    private Rigidbody2D rb;
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize(bool fromTrait = false)
    {
        AssignPowerUp(fromTrait);
        UpdateSprite();
        SetInitialVelocity();
    }

    void AssignPowerUp(bool fromTrait = false)
    {
        if (fromTrait)
        {
            powerUp = PowerUpType.Trail;
            return;
        }
        float chance = Random.Range(0f, 1f);
        powerUp = chance < 0.1f ? (PowerUpType)Random.Range(1, 3) : PowerUpType.None;
    }

    void UpdateSprite()
    {
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

    void SetInitialVelocity()
    {
        direction = Random.insideUnitCircle.normalized;
        rb.velocity = direction * speed;
    }

    private void OnBecameInvisible()
    {
        if (gameObject.activeSelf)
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
        VFXType effectType = VFXType.killEffectBlue;

        switch (powerUp)
        {
            case PowerUpType.Size:
                effectType = VFXType.killEffectYellow;
                break;
            case PowerUpType.Speed:
                effectType = VFXType.killEffectPurple;
                break;
            case PowerUpType.Trail:
                effectType = VFXType.killEffectWhite;
                break;
        }

        VFXManager.Instance.SpawnVFXWithFadeOut(effectType, pos, 1f);
    }
    
    public VFX TriggerSpawnVFX(Vector2 pos)
    {
        VFXType effectType = VFXType.BlueSpawningEffect;

        switch (powerUp)
        {
            case PowerUpType.Size:
                effectType = VFXType.YelloSpawningEffect;
                break;
            case PowerUpType.Speed:
                effectType = VFXType.PurpleSpawningEffect;
                break;
            case PowerUpType.Trail:
                effectType = VFXType.WhiteSpawningEffect;
                break;
        }

        return VFXManager.Instance.SpawnVFX(effectType, pos);
    }
}
