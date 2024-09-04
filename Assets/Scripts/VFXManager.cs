using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CartoonFX;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [System.Serializable]
    public class VFXPrefab
    {
        public VFXType type;
        public GameObject prefab;
    }

    public List<VFXPrefab> vfxPrefabs;

    private Dictionary<VFXType, Queue<VFX>> vfxPools = new Dictionary<VFXType, Queue<VFX>>();
    private List<VFX> activeVFXs = new List<VFX>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (VFXPrefab vfxPrefab in vfxPrefabs)
        {
            vfxPools[vfxPrefab.type] = new Queue<VFX>();
        }
    }

    public VFX SpawnVFX(VFXType type, Vector2 position)
    {
        VFX vfx = GetOrCreateVFX(type, position);
        return vfx;
    }

    public void SpawnVFXWithFadeOut(VFXType type, Vector2 position, float fadeDuration = 1.5f)
    {
        VFX vfx = GetOrCreateVFX(type, position);
        StartCoroutine(FadeOutVFX(vfx, fadeDuration));
    }

    private VFX GetOrCreateVFX(VFXType type, Vector2 position)
    {
        VFX vfx;
        if (vfxPools[type].Count > 0)
        {
            vfx = vfxPools[type].Dequeue();
            vfx.go.transform.position = position;
            vfx.go.SetActive(true);
        }
        else
        {
            GameObject prefab = vfxPrefabs.Find(p => p.type == type).prefab;
            GameObject vfxObject = Instantiate(prefab, position, Quaternion.identity);
            vfx = new VFX { type = type, go = vfxObject };
        }
        vfx.inUse = true;
        activeVFXs.Add(vfx);
        return vfx;
    }

    private IEnumerator FadeOutVFX(VFX vfx, float fadeDuration)
    {
        if (vfx?.go == null || !vfx.go.activeInHierarchy) yield break;

        ParticleSystem particleSystem = vfx.go.GetComponent<ParticleSystem>();
        if (particleSystem == null || !particleSystem.isPlaying) yield break;

        // 获取或添加 ColorOverLifetimeModule
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // 创建一个新的 Gradient 来实现淡出效果
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );

        // 应用 Gradient 到 ColorOverLifetimeModule
        colorOverLifetime.color = gradient;

        // 等待淡出持续时间
        // fadeDuration = particleSystem.main.duration;
        yield return new WaitForSeconds(fadeDuration);

        if (vfx != null && vfx.go != null)
        {
            DeActivate(vfx);
        }
    }

    public IEnumerator SpawnTextVFX(string text, Color[] colors, Vector2 position)
    {
        VFX vfx = GetOrCreateVFX(VFXType.PopTextEffect, position);
        vfx.inUse = true;
        var fx = vfx.go.GetComponent<CFXR_ParticleText>();
        var ps = vfx.go.GetComponent<ParticleSystem>();

        fx.UpdateText(text, null, colors[1], colors[2], colors[0]);
        ps.Play();

        yield return new WaitUntil(() => ps.isStopped);

        DeActivate(vfx);
    }

    public void DeActivateAll()
    {
        StopAllCoroutines();
        foreach (var vfx in activeVFXs.ToList())
        {
            DeActivate(vfx);
        }
        activeVFXs.Clear();
    }

    public void DeActivate(VFX vfx)
    {
        if (vfx?.go != null)
        {
            vfx.go.SetActive(false);
            vfx.inUse = false;
            vfxPools[vfx.type].Enqueue(vfx);
            activeVFXs.Remove(vfx);
        }
    }

    private void OnDestroy()
    {
        foreach (var vfx in activeVFXs)
        {
            Destroy(vfx.go);
        }
        activeVFXs.Clear();

        foreach (var pool in vfxPools.Values)
        {
            foreach (var vfx in pool)
            {
                Destroy(vfx.go);
            }
        }
        vfxPools.Clear();
    }
}

public class VFX
{
    public VFXType type;
    public GameObject go;
    public bool inUse;
}

public enum VFXType
{
    hitEffect,
    killEffect,
    killEffectPurple,
    killEffectBlue,
    killEffectYellow,
    killEffectWhite,
    shieldBreakingEffect,
    BlueSpawningEffect,
    WhiteSpawningEffect,
    YelloSpawningEffect,
    PurpleSpawningEffect,
    RedSpawningEffect,
    EvilBallDieEffect,
    PopTextEffect,
}