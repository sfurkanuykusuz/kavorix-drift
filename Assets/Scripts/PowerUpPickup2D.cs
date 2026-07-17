using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class PowerUpPickup2D : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField, Min(0f)] private float lifetime = 12f;

    [Header("Visual")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private bool useUnscaledTime;

    [Header("Idle Pulse")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField, Min(0f)] private float pulseScaleAmount = 0.08f;
    [SerializeField, Min(0f)] private float pulseSpeed = 3f;

    [Header("Spawn Effect")]
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField, Min(0f)] private float spawnEffectLifetime = 1f;

    [Header("Idle Shine Effect")]
    [SerializeField] private GameObject idleShineEffectPrefab;
    [SerializeField, Min(0f)] private float minShineInterval = 1.8f;
    [SerializeField, Min(0f)] private float maxShineInterval = 3.4f;
    [SerializeField, Min(0f)] private float idleShineEffectLifetime = 1f;

    [Header("Pickup Effect")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField, Min(0f)] private float pickupEffectLifetime = 1f;

    [Header("Pickup SFX")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.75f;

    private bool isCollected;
    private float shineTimer;
    private Vector3 baseVisualScale;
    private Collider2D pickupCollider;

    public event Action<PowerUpPickup2D> Collected;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;

        if (visualTransform == null)
        {
            visualTransform = transform;
        }

        baseVisualScale = visualTransform.localScale;
    }

    private void Start()
    {
        PlayEffect(spawnEffectPrefab, spawnEffectLifetime);
        ResetShineTimer();

        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void Update()
    {
        if (isCollected)
        {
            return;
        }

        float deltaTime = GetDeltaTime();

        RotateVisual(deltaTime);
        PulseVisual();
        UpdateIdleShine(deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        Collect();
    }

    private void Collect()
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        PlayEffect(pickupEffectPrefab, pickupEffectLifetime);
        PlayPickupSfx();

        Collected?.Invoke(this);

        Destroy(gameObject);
    }

    private void RotateVisual(float deltaTime)
    {
        if (visualTransform == null || Mathf.Approximately(rotationSpeed, 0f))
        {
            return;
        }

        visualTransform.Rotate(0f, 0f, rotationSpeed * deltaTime);
    }

    private void PulseVisual()
    {
        if (!enablePulse || visualTransform == null || Mathf.Approximately(pulseScaleAmount, 0f) || Mathf.Approximately(pulseSpeed, 0f))
        {
            return;
        }

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float scaleOffset = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.5f * pulseScaleAmount;

        visualTransform.localScale = baseVisualScale * (1f + scaleOffset);
    }

    private void UpdateIdleShine(float deltaTime)
    {
        if (idleShineEffectPrefab == null)
        {
            return;
        }

        shineTimer -= deltaTime;

        if (shineTimer > 0f)
        {
            return;
        }

        PlayEffect(idleShineEffectPrefab, idleShineEffectLifetime);
        ResetShineTimer();
    }

    private void ResetShineTimer()
    {
        float minInterval = Mathf.Min(minShineInterval, maxShineInterval);
        float maxInterval = Mathf.Max(minShineInterval, maxShineInterval);

        shineTimer = UnityEngine.Random.Range(minInterval, maxInterval);
    }

    private void PlayEffect(GameObject effectPrefab, float effectLifetime)
    {
        if (effectPrefab == null)
        {
            return;
        }

        GameObject effectObject = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        SetParticleSystemsUnscaledTime(effectObject);

        if (effectLifetime > 0f)
        {
            Destroy(effectObject, effectLifetime);
        }
    }

    private void SetParticleSystemsUnscaledTime(GameObject effectObject)
    {
        if (!useUnscaledTime || effectObject == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = effectObject.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem.MainModule mainModule = particleSystems[i].main;
            mainModule.useUnscaledTime = true;
        }
    }

    private void PlayPickupSfx()
    {
        if (pickupSfx == null)
        {
            return;
        }

        GameObject sfxObject = new GameObject("PowerUp Pickup SFX");
        sfxObject.transform.position = transform.position;

        AudioSource source = sfxObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;

        source.PlayOneShot(pickupSfx, pickupSfxVolume);

        Destroy(sfxObject, pickupSfx.length);
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void OnValidate()
    {
        lifetime = Mathf.Max(0f, lifetime);
        pulseScaleAmount = Mathf.Max(0f, pulseScaleAmount);
        pulseSpeed = Mathf.Max(0f, pulseSpeed);

        spawnEffectLifetime = Mathf.Max(0f, spawnEffectLifetime);
        idleShineEffectLifetime = Mathf.Max(0f, idleShineEffectLifetime);
        pickupEffectLifetime = Mathf.Max(0f, pickupEffectLifetime);

        minShineInterval = Mathf.Max(0f, minShineInterval);
        maxShineInterval = Mathf.Max(0f, maxShineInterval);

        pickupSfxVolume = Mathf.Clamp01(pickupSfxVolume);
    }
}