using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class EvilBall : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public GameObject Shield;
    public ScoringSystem ScoringSystem;
    
    private Rigidbody2D rb;
    private float baseSpeed = 2f;

    private void Update()
    {
        // Move towards the player
        MoveTowardsPlayer();
        HandleShield();
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * baseSpeed * Time.deltaTime);
    }

    void HandleShield()
    {
        if (hasShield)
        {
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
        hasShield = true;
        // Add visual indicator for shield (optional)
        Shield.SetActive(hasShield);
        Debug.Log("Shield activated");
    }
    
    public void BreakShield()
    {
        hasShield = false;
        Shield.SetActive(hasShield);
        Debug.Log("Shield broken");
        
    }
    
    [FormerlySerializedAs("minHealth")] public int minHealthRandom = 200;
    [FormerlySerializedAs("maxHealth")] public int maxHealthRandom = 500;
    public ObjectPool EvilBallPool;
    public int health = 100;
    public TextMeshProUGUI healthText;
    public int healthGainAmount = 10;  // Amount of health gained by killing a normal ball

    private int maxHealth;
    private bool hasShield = false;  // Shield status
    private float shieldCooldown = 10f;  // Cooldown for gaining shield
    private float shieldTimer = 0f;  // Timer for shield activation

    void Start()
    {
        UpdateHealthText();
        ScoringSystem = FindObjectOfType<ScoringSystem>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        if (hasShield)
        {
            hasShield = false;
            Debug.Log("Shield absorbed damage");
            return;  // Absorb the damage
        }
        health -= damage;
        Debug.Log($"EvilBall takes {damage} damage");
        UpdateHealthText();
        if (health <= 0)
        {
            ScoringSystem.AddScore(maxHealth / 10);
            ReturnToPool();
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
    
    public void GainHealth(int amount)
    {
        health += amount;
        UpdateHealthText();
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("NormalBall"))
        {
            NormalBall normalBall = col.gameObject.GetComponent<NormalBall>();

            if (normalBall.powerUp == NormalBall.PowerUpType.Size)
            {
                transform.localScale *= 1.5f;  // Evil ball grows in size
            }
            else if (normalBall.powerUp == NormalBall.PowerUpType.Speed)
            {
                baseSpeed *= 1.5f;  // Evil ball gains speed
                rb.velocity *= 1.5f;
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