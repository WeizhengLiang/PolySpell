using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    public Slider energyBar;
    public Image energyCircle;

    private float maxEnergy = 100f;
    public float energyConsumptionRate = 0.5f;  // Energy consumed per second while drawing
    public float energyGainAmount = 10f;  // Energy gained by killing a normal ball
    private float currentEnergy;

    void Start()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    void Update()
    {
        if (currentEnergy <= 0)
        {
            currentEnergy = 0;
            UpdateEnergyBar();
            // if (trailRenderer.positionCount > 0)
            // {
            //     ClearTrailAndSpawnBalls();
            // }
        }
    }

    public bool ConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
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

    private void UpdateEnergyBar()
    {
        energyCircle.fillAmount = currentEnergy / maxEnergy;
        energyBar.value = currentEnergy / maxEnergy;
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    public bool IsEnergyEmpty => currentEnergy < energyConsumptionRate;
}