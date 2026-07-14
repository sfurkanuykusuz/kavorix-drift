using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class PlayerShield2D : MonoBehaviour
{
    [Header("Shield")]
    [SerializeField, Min(0.1f)] private float duration = 10f;
    [SerializeField, Min(0f)] private float minBounceSpeed = 6f;
    [SerializeField, Min(0f)] private float bounceImpulse = 4f;

    [Header("Visual")]
    [SerializeField] private GameObject shieldVisual;

    [Header("SFX")]
    [SerializeField] private AudioSource shieldAudioSource;
    [SerializeField] private AudioClip shieldActivateSfx;
    [SerializeField] private AudioClip shieldHitSfx;
    [SerializeField, Range(0f, 1f)] private float shieldActivateSfxVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float shieldHitSfxVolume = 0.8f;
    [SerializeField, Min(0f)] private float hitSfxCooldown = 0.08f;

    private Collider2D shieldCollider;
    private Coroutine shieldRoutine;
    private float nextAllowedHitSfxTime;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        shieldCollider = GetComponent<Collider2D>();
        shieldCollider.isTrigger = true;

        if (shieldAudioSource == null)
        {
            shieldAudioSource = GetComponent<AudioSource>();
        }

        SetShieldActive(false);
    }

    private void OnDisable()
    {
        Deactivate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryBounceObstacle(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryBounceObstacle(other);
    }

    public void Activate()
    {
        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
        }

        IsActive = true;
        SetShieldActive(true);
        PlayShieldActivateSfx();

        shieldRoutine = StartCoroutine(ShieldRoutine());
    }

    public void Deactivate()
    {
        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
            shieldRoutine = null;
        }

        IsActive = false;
        SetShieldActive(false);
    }

    private IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(duration);

        shieldRoutine = null;
        IsActive = false;
        SetShieldActive(false);
    }

    private void TryBounceObstacle(Collider2D other)
    {
        if (!IsActive)
        {
            return;
        }

        Obstacle obstacle = other.GetComponentInParent<Obstacle>();

        if (obstacle == null || obstacle.Body == null)
        {
            return;
        }

        BounceObstacle(obstacle);
        PlayShieldHitSfx();
    }

    private void BounceObstacle(Obstacle obstacle)
    {
        Vector2 shieldCenter = transform.position;
        Vector2 obstaclePosition = obstacle.transform.position;

        Vector2 bounceDirection = obstaclePosition - shieldCenter;

        if (bounceDirection.sqrMagnitude <= 0.001f)
        {
            bounceDirection = obstacle.Velocity.sqrMagnitude > 0.001f
                ? -obstacle.Velocity.normalized
                : Vector2.up;
        }
        else
        {
            bounceDirection.Normalize();
        }

        float bounceSpeed = Mathf.Max(obstacle.CurrentSpeed, minBounceSpeed);

        obstacle.Body.linearVelocity = bounceDirection * bounceSpeed;
        obstacle.ApplyImpulse(bounceDirection * bounceImpulse);
    }

    private void PlayShieldActivateSfx()
    {
        PlayShieldSfx(shieldActivateSfx, shieldActivateSfxVolume);
    }

    private void PlayShieldHitSfx()
    {
        if (Time.unscaledTime < nextAllowedHitSfxTime)
        {
            return;
        }

        nextAllowedHitSfxTime = Time.unscaledTime + hitSfxCooldown;
        PlayShieldSfx(shieldHitSfx, shieldHitSfxVolume);
    }

    private void PlayShieldSfx(AudioClip clip, float volume)
    {
        if (shieldAudioSource == null || clip == null)
        {
            return;
        }

        shieldAudioSource.PlayOneShot(clip, volume);
    }

    private void SetShieldActive(bool isActive)
    {
        if (shieldCollider != null)
        {
            shieldCollider.enabled = isActive;
        }

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(isActive);
        }
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.1f, duration);
        minBounceSpeed = Mathf.Max(0f, minBounceSpeed);
        bounceImpulse = Mathf.Max(0f, bounceImpulse);

        shieldActivateSfxVolume = Mathf.Clamp01(shieldActivateSfxVolume);
        shieldHitSfxVolume = Mathf.Clamp01(shieldHitSfxVolume);
        hitSfxCooldown = Mathf.Max(0f, hitSfxCooldown);
    }
}