using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    public GameObject hitEffectPrefab;
    public GameObject killEffectPrefab;
    public GameObject killEffectPurplePrefab;
    public GameObject killEffectBluePrefab;
    public GameObject killEffectYellowPrefab;
    public GameObject shieldBreakingEffectPrefab;
    public GameObject BlueSpawningEffectPrefab;
    public GameObject YelloSpawningEffectPrefab;
    public GameObject PurpleSpawningEffectPrefab;
    public GameObject RedSpawningEffectPrefab;
    public GameObject EvilBallDieEffectPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnVFXWithFadeOut(GameObject prefab, Vector2 position, float fadeDuration = 1.5f)
    {
        GameObject vfx = Instantiate(prefab, position, Quaternion.identity);
        StartCoroutine(FadeOutVFX(vfx, fadeDuration));
    }
    
    public GameObject SpawnVFX(GameObject prefab, Vector2 position)
    {
        return Instantiate(prefab, position, Quaternion.identity);
    }
    
    private IEnumerator FadeOutVFX(GameObject vfx, float fadeDuration)
    {
        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            float fadeSpeed = 1.0f / fadeDuration;
            float t = 0;
            ParticleSystem.MainModule main = particleSystem.main;  // Get the main module once

            // Get the initial start color from the main module
            Color startColor = main.startColor.color;

            while (t < 1)
            {
                t += Time.deltaTime * fadeSpeed;

                // Lerp the alpha value of the start color
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(startColor.a, 0, t);

                // Apply the new color to the startColor property of the main module
                if(particleSystem != null) main.startColor = new ParticleSystem.MinMaxGradient(newColor);

                yield return null;
            }

            Destroy(vfx);
        }
    }
}