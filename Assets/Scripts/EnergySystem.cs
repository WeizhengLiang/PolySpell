using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 200f;
    [SerializeField] private float energyConsumptionRate = 5f;
    [SerializeField] private float energyGainAmount = 10f;

    private float currentEnergy;
    private float consumedEnergy;

    private PlayerUI playerUI;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyConsumptionRate => energyConsumptionRate;
    public float EnergyGainAmount => energyGainAmount;
    public bool IsEnergyEmpty => currentEnergy < energyConsumptionRate;

    public void Initialize(PlayerUI playerUI)
    {
        this.playerUI = playerUI;
        ResetEnergy();
    }

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
        if (playerUI != null)
            playerUI.SetEnergyUI(currentEnergy, maxEnergy);
    }
}