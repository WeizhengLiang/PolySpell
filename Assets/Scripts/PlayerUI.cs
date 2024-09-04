using UnityEngine;
using UnityEngine.UI;
using Kalkatos.DottedArrow;

public class PlayerUI : MonoBehaviour
{
    [Header("Global UI")]
    private Slider healthBar; // 需要序列化
    private Slider energyBar; // 需要序列化

    [Header("Local UI")]
    private Image healthCircle; // 不需要序列化
    private Image energyCircle; // 不需要序列化

    [Header("Player Arrow")]
    private Arrow arrow;

    public void SetHealthUI(float currentHealth, float maxHealth)
    {
        if (healthCircle != null)
            healthCircle.fillAmount = currentHealth / maxHealth;
        
        if (healthBar != null)
            healthBar.value = currentHealth / maxHealth;
    }

    public void SetEnergyUI(float currentEnergy, float maxEnergy)
    {
        if (energyCircle != null)
            energyCircle.fillAmount = currentEnergy / maxEnergy;
        
        if (energyBar != null)
            energyBar.value = currentEnergy / maxEnergy;
    }

    public void InitializeLocalUI(Image healthCircle, Image energyCircle)
    {
        this.healthCircle = healthCircle;
        this.energyCircle = energyCircle;
    }

    public void InitializeGlobalUI(Slider healthBar, Slider energyBar, Arrow arrow)
    {
        this.healthBar = healthBar;
        this.energyBar = energyBar;
        this.arrow = arrow;
    }

    public void SetupAndActivateArrow(Transform playerTransform)
    {
        if (arrow != null)
        {
            arrow.SetupAndActivate(playerTransform);
        }
    }

    public void DeactivateArrow()
    {
        if (arrow != null)
        {
            arrow.Deactivate();
        }
    }
}