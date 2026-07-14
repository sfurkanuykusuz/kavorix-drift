using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class PowerUpPickup2D : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField, Min(0f)] private float lifetime = 12f;

    [Header("Visual Rotation")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private bool useUnscaledTime;

    [Header("Pickup SFX")]
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Range(0f, 1f)] private float pickupSfxVolume = 0.75f;

    private bool isCollected;
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
    }

    private void Start()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void Update()
    {
        RotateVisual();
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
        isCollected = true;

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        PlayPickupSfx();

        Collected?.Invoke(this);
        Destroy(gameObject);
    }

    private void RotateVisual()
    {
        if (visualTransform == null || Mathf.Approximately(rotationSpeed, 0f))
        {
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        visualTransform.Rotate(0f, 0f, rotationSpeed * deltaTime);
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

    private void OnValidate()
    {
        lifetime = Mathf.Max(0f, lifetime);
        pickupSfxVolume = Mathf.Clamp01(pickupSfxVolume);
    }
}