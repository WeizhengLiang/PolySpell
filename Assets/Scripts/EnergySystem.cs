using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    public Slider energyBar;
    public GameObject normalBallPrefab;  // Reference to the normal ball prefab
    public TrailRenderer trailRenderer;  // Reference to the trail renderer

    private float maxEnergy = 1000f;
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
            if (trailRenderer.positionCount > 0)
            {
                ClearTrailAndSpawnBalls();
            }
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
        else
        {
            if (trailRenderer.positionCount > 0)
            {
                ClearTrailAndSpawnBalls();
            }
            return false;
        }
    }

    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        energyBar.value = currentEnergy / maxEnergy;
    }

    private void ClearTrailAndSpawnBalls()
    {
        // Calculate the total length of the trail
        float trailLength = CalculateTrailLength();

        // Determine the interval for spawning normal balls
        int numberOfBalls = Mathf.CeilToInt(trailLength);
        float interval = trailLength / numberOfBalls;

        // Spawn normal balls at regular intervals along the trail
        float distanceCovered = 0f;
        Vector3 lastPosition = trailRenderer.GetPosition(0);
        for (int i = 1; i < trailRenderer.positionCount; i++)
        {
            Vector3 currentPosition = trailRenderer.GetPosition(i);
            float segmentLength = Vector3.Distance(lastPosition, currentPosition);
            while (distanceCovered + segmentLength >= interval)
            {
                float remainingDistance = interval - distanceCovered;
                Vector3 spawnPosition = Vector3.Lerp(lastPosition, currentPosition, remainingDistance / segmentLength);
                Instantiate(normalBallPrefab, spawnPosition, Quaternion.identity);
                distanceCovered = 0f;
                segmentLength -= remainingDistance;
                lastPosition = spawnPosition;
            }
            distanceCovered += segmentLength;
            lastPosition = currentPosition;
        }
        
        // Clear the trail renderer
        trailRenderer.Clear();
        trailRenderer.enabled = false;
    }

    private float CalculateTrailLength()
    {
        float length = 0f;
        for (int i = 0; i < trailRenderer.positionCount - 1; i++)
        {
            length += Vector3.Distance(trailRenderer.GetPosition(i), trailRenderer.GetPosition(i + 1));
        }
        return length;
    }
    
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    public bool IsEnergyEmpty => currentEnergy < energyConsumptionRate;
}