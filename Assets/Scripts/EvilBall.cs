using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class EvilBall : MonoBehaviour
{
    public Transform player;
    public GameObject Shield;
    public ScoringSystem ScoringSystem;
    [SerializeField] private int minHealthRandom = 20;
    [SerializeField] private int maxHealthRandom = 50;
    public ObjectPool EvilBallPool;
    public float health = 10f;
    public TextMeshProUGUI healthText;
    public float healthGainAmount = 2f;
    public int DamagerMultiplier = 10;

    private Rigidbody2D rb;
    [SerializeField] private float baseSpeed = 2f;
    private float maxHealth;
    private bool hasShield = false;
    private float shieldCooldown = 10f;
    private float shieldTimer = 0f;

    private static int instanceCounter = 0;
    private int instanceId;

    void Awake()
    {
        instanceId = ++instanceCounter;
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.gravityScale = 0;
        LogManager.Instance.Log($"EvilBall {instanceId} Awake: Rigidbody2D initialized. IsKinematic: {rb.isKinematic}, GravityScale: {rb.gravityScale}");
    }

    private void OnEnable()
    {
        LogManager.Instance.Log($"EvilBall {instanceId} OnEnable");
        Initialize();
    }
    
    private void Update()
    {
        LogManager.Instance.Log($"EvilBall {instanceId} Update: Base speed: {baseSpeed}, Player null: {player == null}");
        MoveTowardsPlayer();
        HandleShield();
    }

    public void Initialize()
    {
        LogManager.Instance.Log($"EvilBall {instanceId} Initialize: Start");
        if (ScoringSystem == null)
        {
            ScoringSystem = FindObjectOfType<ScoringSystem>();
        }
        SetRandomHealth();
        UpdateHealthText();
        UpdateSize();
        Shield.SetActive(hasShield);
        LogManager.Instance.Log($"EvilBall {instanceId} Initialize: End. Player null: {player == null}, Player position: {(player != null ? player.position.ToString() : "N/A")}");
    }
    
    private void MoveTowardsPlayer()
    {
        if (player == null)
        {
            LogManager.Instance.Log($"EvilBall {instanceId} MoveTowardsPlayer: Player reference is null!");
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * baseSpeed;
        LogManager.Instance.Log($"EvilBall {instanceId} MoveTowardsPlayer: Direction: {direction}, Velocity: {rb.velocity}, Position: {transform.position}");
    }

    public int CalculateDamage(int damage)
    {
        return damage * DamagerMultiplier;
    }

    public void TakeDamage(int damage)
    {
        if (hasShield)
        {
            LogManager.Instance.Log("Shield absorbed damage");
            return;
        }
        health -= CalculateDamage(damage);
        UpdateHealthText();
        if (health <= 0)
        {
            VFXManager.Instance.SpawnVFX(VFXType.EvilBallDieEffect, transform.position);
            ScoringSystem.AddScore(Mathf.CeilToInt(maxHealth / 10));
            ReturnToPool();
        }
        else
        {
            UpdateSize();
        }
    }
    
    public void SetRandomHealth()
    {
        health = Random.Range(minHealthRandom, maxHealthRandom);
        maxHealth = health;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{health}/{maxHealth}";
        }
    }
    
    public void GainHealth(float amount)
    {
        health += amount;
        UpdateHealthText();
        UpdateSize();
    }
    
    void HandleShield()
    {
        if (hasShield)
        {
            shieldTimer = 0f;
            return;
        }
        
        shieldTimer += Time.deltaTime;
        if (shieldTimer >= shieldCooldown)
        {
            GainShield();
            shieldTimer = 0f;
        }
    }
    
    void GainShield()
    {
        if(hasShield) return;
        hasShield = true;
        Shield.SetActive(true);
    }
    
    public void BreakShield()
    {
        if (!hasShield) return;
        
        VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.shieldBreakingEffect, transform.position);
        hasShield = false;
        Shield.SetActive(false);
    }

    void UpdateSize()
    {
        float scale = Mathf.Lerp(0.5f, 2f, health / maxHealthRandom);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("NormalBall"))
        {
            NormalBall normalBall = col.gameObject.GetComponent<NormalBall>();

            switch (normalBall.powerUp)
            {
                case NormalBall.PowerUpType.Size:
                    GainHealth(150f);
                    break;
                case NormalBall.PowerUpType.Speed:
                    baseSpeed *= 1.5f;
                    rb.velocity *= 1.2f;
                    break;
            }
            
            normalBall.ReturnToPool();
            GainHealth(healthGainAmount);
        }
    }

    private void ReturnToPool()
    {
        EvilBallPool.ReturnObject(gameObject);
    }

    private void OnDisable()
    {
        LogManager.Instance.Log($"EvilBall {instanceId} OnDisable: Ball disabled");
    }
}