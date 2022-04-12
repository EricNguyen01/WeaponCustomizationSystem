using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectSpawner : MonoBehaviour
{
    [SerializeField] private bool disableEffectSpawning = false;
    [SerializeField] private bool spawnEffectOnEnable = false;
    [SerializeField] private ParticleSystem effectPrefab;
    [SerializeField] private Transform effectSpawnTransform;
    private ParticleSystem generatedEffect;

    private void Awake()
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("Effect Prefab is not assigned on EffectSpawner component: " + name);
        }
        else GenerateEffect(effectPrefab);
    }

    private void OnEnable()
    {
        if (spawnEffectOnEnable)
        {
            if (disableEffectSpawning || generatedEffect == null) return;
        }
    }

    private void OnDisable()
    {
        if (generatedEffect != null) EnableEffect(false);
    }

    private void GenerateEffect(ParticleSystem effect)
    {
        Transform spawnTransform;

        if (effectSpawnTransform == null) spawnTransform = transform;
        else spawnTransform = effectSpawnTransform;

        GameObject obj = Instantiate(effect.gameObject, spawnTransform.position, Quaternion.identity, transform);
        generatedEffect = obj.GetComponent<ParticleSystem>();
    }

    public void SetEffectToSpawn(ParticleSystem effectToSpawn)
    {
        GenerateEffect(effectToSpawn);
    }

    public void EnableEffect(bool enabled)
    {
        if (disableEffectSpawning || generatedEffect == null) return;

        if (enabled) generatedEffect.Play();
        else generatedEffect.Stop();
    }
}
