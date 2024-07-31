using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public delegate void OnHealthZero();
    public event OnHealthZero onHealthZero;
    
    public Image healthCircle;
    public float maxHealth = 100f;
    private float currentHealth;

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
            // Handle Bob's death (if necessary)
            onHealthZero?.Invoke();  // Notify listeners when health reaches zero
            Debug.Log("Bob is dead");
        }
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthCircle.fillAmount = currentHealth / maxHealth;
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
    
    public void Heal(float amount)  // changed: New method to heal Bob
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthBar();
        Debug.Log($"Bob heals {amount}");
    }
}