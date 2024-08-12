using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class EvilBall : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public GameObject Shield;
    public ScoringSystem ScoringSystem;
    [FormerlySerializedAs("minHealth")] public int minHealthRandom = 200;
    [FormerlySerializedAs("maxHealth")] public int maxHealthRandom = 500;
    public ObjectPool EvilBallPool;
    public float health = 100f;
    public TextMeshProUGUI healthText;
    public float healthGainAmount = 10f;  // Amount of health gained by killing a normal ball

    private Rigidbody2D rb;
    private float baseSpeed = 2f;
    private float maxHealth;
    private bool hasShield = false;  // Shield status
    private float shieldCooldown = 10f;  // Cooldown for gaining shield
    private float shieldTimer = 0f;  // Timer for shield activation

    void Start()
    {
        Initialize();
    }
    
    private void Update()
    {
        // Move towards the player
        MoveTowardsPlayer();
        HandleShield();
    }

    public void Initialize()
    {
        ScoringSystem = FindObjectOfType<ScoringSystem>();
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthText();
        UpdateSize();
        Shield.SetActive(hasShield);
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * baseSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        if (hasShield)
        {
            Debug.Log("Shield absorbed damage");
            return;  // Absorb the damage
        }
        health -= damage;
        Debug.Log($"EvilBall takes {damage} damage");
        UpdateHealthText();
        if (health <= 0)
        {
            VFXManager.Instance.SpawnVFX(VFXType.EvilBallDieEffect ,VFXManager.Instance.EvilBallDieEffectPrefab, transform.position);
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
        // Add visual indicator for shield (optional)
        Shield.SetActive(hasShield);
        Debug.Log("Shield activated");
    }
    
    public void BreakShield()
    {
        if (!hasShield)
        {
            Debug.Log("no shield can be break");
            return;
        }
        
        VFXManager.Instance.SpawnVFXWithFadeOut(VFXType.shieldBreakingEffect ,VFXManager.Instance.shieldBreakingEffectPrefab, transform.position);
        hasShield = false;
        Shield.SetActive(hasShield);
        Debug.Log("Shield broken");
        
    }

    void UpdateSize()
    {
        float scale = Mathf.Lerp(0.5f, 2f, health / maxHealthRandom);  // Adjust the size range as needed
        transform.localScale = new Vector3(scale, scale, 1f);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("NormalBall"))
        {
            NormalBall normalBall = col.gameObject.GetComponent<NormalBall>();

            if (normalBall.powerUp == NormalBall.PowerUpType.Size)
            {
                GainHealth(150f);  // Heal more for size power-up
            }
            else if (normalBall.powerUp == NormalBall.PowerUpType.Speed)
            {
                baseSpeed *= 1.5f;  // Evil ball gains speed
                rb.velocity *= 1.2f;
            }
            
            normalBall.ReturnToPool();  // Remove the normal ball
            GainHealth(healthGainAmount);  // Gain health for the evil ball
        }
    }

    private void ReturnToPool()
    {
        EvilBallPool.ReturnObject(gameObject);
    }
}