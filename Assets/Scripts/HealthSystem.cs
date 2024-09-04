using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public delegate void OnHealthZero();
    public event OnHealthZero onHealthZero;
    
    public float maxHealth = 100f;
    private float currentHealth;

    private PlayerUI playerUI;

    public float CurrentHealth => currentHealth;

    public void Initialize(PlayerUI playerUI)
    {
        this.playerUI = playerUI;
        ResetHealth();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            onHealthZero?.Invoke();
            Debug.Log("Player is dead");
        }
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (playerUI != null)
            playerUI.SetHealthUI(currentHealth, maxHealth);
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Player heals {amount}");
    }
}