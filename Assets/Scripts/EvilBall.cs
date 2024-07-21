using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvilBall : MonoBehaviour
{
    public float speed = 0.5f;  // Speed of the evil ball
    public Transform player;  // Reference to the player
    public GameObject Shield;

    private void Update()
    {
        // Move towards the player
        MoveTowardsPlayer();
        HandleShield();
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
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
    
    public int minHealth = 200;
    public int maxHealth = 1000;
    public ObjectPool EvilBallPool;
    public int health = 100;
    public TextMeshProUGUI healthText;
    public int healthGainAmount = 10;  // Amount of health gained by killing a normal ball
    
    private bool hasShield = false;  // Shield status
    private float shieldCooldown = 10f;  // Cooldown for gaining shield
    private float shieldTimer = 0f;  // Timer for shield activation

    void Start()
    {
        UpdateHealthText();
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
            ReturnToPool();
        }
    }
    
    public void SetRandomHealth()
    {
        health = Random.Range(minHealth, maxHealth);
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = health.ToString();
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
            col.gameObject.GetComponent<NormalBall>()?.ReturnToPool();  // Remove the normal ball
            GainHealth(healthGainAmount);  // Gain health for the evil ball
        }
    }

    private void ReturnToPool()
    {
        EvilBallPool.ReturnObject(gameObject);
    }
}