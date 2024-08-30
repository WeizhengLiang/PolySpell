using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private Slider energyBar;
    [SerializeField] private Image energyCircle;

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyConsumptionRate = 0.5f;
    [SerializeField] private float energyGainAmount = 10f;

    private float currentEnergy;
    private float consumedEnergy;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyConsumptionRate => energyConsumptionRate;
    public float EnergyGainAmount => energyGainAmount;
    public bool IsEnergyEmpty => currentEnergy < energyConsumptionRate;

    private void Start()
    {
        ResetEnergy();
    }

    public bool ConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            consumedEnergy += amount;
            UpdateEnergyBar();
            return true;
        }
        return false;
    }

    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateEnergyBar();
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        consumedEnergy = 0f;
        UpdateEnergyBar();
    }

    public float GetConsumedEnergy()
    {
        float temp = consumedEnergy;
        consumedEnergy = 0f;
        return temp;
    }

    private void UpdateEnergyBar()
    {
        if (energyCircle != null)
            energyCircle.fillAmount = currentEnergy / maxEnergy;
        
        if (energyBar != null)
            energyBar.value = currentEnergy / maxEnergy;
    }
}