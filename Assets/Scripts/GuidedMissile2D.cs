using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class GuidedMissile2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float speed = 12f;
    [SerializeField, Min(0f)] private float turnSpeed = 420f;
    [SerializeField, Min(0.1f)] private float lifetime = 5f;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField, Min(0f)] private float impactEffectLifetime = 1.5f;

    [Header("Launch Audio")]
    [SerializeField] private AudioClip launchSfx;
    [SerializeField, Range(0f, 1f)] private float launchSfxVolume = 0.75f;

    [Header("Impact Audio")]
    [SerializeField] private AudioClip impactSfx;
    [SerializeField, Range(0f, 1f)] private float impactSfxVolume = 0.8f;

    [Header("References")]
    [SerializeField] private ObstacleTracker2D obstacleTracker;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraShake2D cameraShake;

    private Rigidbody2D rb;
    private Collider2D missileCollider;

    private Obstacle targetObstacle;
    private GameObject targetIndicator;

    private bool hasImpacted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        missileCollider = GetComponent<Collider2D>();
        missileCollider.isTrigger = true;

        ResolveReferences();
    }

    private void Start()
    {
        PlayLaunchSfx();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (hasImpacted)
        {
            return;
        }

        if (ShouldCancelBecauseGameEnded())
        {
            Destroy(gameObject);
            return;
        }

        if (targetObstacle == null)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardsTarget();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasImpacted || ShouldCancelBecauseGameEnded())
        {
            return;
        }

        Obstacle hitObstacle = other.GetComponentInParent<Obstacle>();

        if (hitObstacle == null)
        {
            return;
        }

        ImpactObstacle(hitObstacle);
    }

    private void OnDestroy()
    {
        DestroyTargetIndicator();
    }

    public void SetTarget(Obstacle obstacle, GameObject indicator)
    {
        targetObstacle = obstacle;
        targetIndicator = indicator;

        if (targetObstacle == null)
        {
            Destroy(gameObject);
        }
    }

    private void ResolveReferences()
    {
        if (obstacleTracker == null)
        {
            obstacleTracker = FindAnyObjectByType<ObstacleTracker2D>();
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        if (cameraShake == null)
        {
            cameraShake = FindAnyObjectByType<CameraShake2D>();
        }
    }

    private bool ShouldCancelBecauseGameEnded()
    {
        return playerController != null && !playerController.IsGamePlaying;
    }

    private void MoveTowardsTarget()
    {
        Vector2 missilePosition = rb.position;
        Vector2 targetPosition = targetObstacle.transform.position;

        Vector2 directionToTarget = targetPosition - missilePosition;

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            ImpactObstacle(targetObstacle);
            return;
        }

        directionToTarget.Normalize();

        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newAngle);
        rb.linearVelocity = transform.up * speed;
    }

    private void ImpactObstacle(Obstacle obstacle)
    {
        if (obstacle == null || ShouldCancelBecauseGameEnded())
        {
            Destroy(gameObject);
            return;
        }

        hasImpacted = true;

        SpawnImpactEffect();
        PlayImpactSfx();
        cameraShake?.ShakeObstacleExplosion();

        obstacleTracker?.NotifyObstacleDestroyed(obstacle);

        DestroyTargetIndicator();
        obstacle.DestroyObstacle();

        Destroy(gameObject);
    }

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
        {
            return;
        }

        GameObject impactEffect = Instantiate(
            impactEffectPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(impactEffect, impactEffectLifetime);
    }

    private void PlayLaunchSfx()
    {
        PlayDetachedSfx("Missile Launch SFX", launchSfx, launchSfxVolume);
    }

    private void PlayImpactSfx()
    {
        PlayDetachedSfx("Missile Impact SFX", impactSfx, impactSfxVolume);
    }

    private void PlayDetachedSfx(string objectName, AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        GameObject sfxObject = new GameObject(objectName);
        sfxObject.transform.position = transform.position;

        AudioSource source = sfxObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;

        source.PlayOneShot(clip, volume);

        Destroy(sfxObject, clip.length);
    }

    private void DestroyTargetIndicator()
    {
        if (targetIndicator == null)
        {
            return;
        }

        Destroy(targetIndicator);
        targetIndicator = null;
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
        lifetime = Mathf.Max(0.1f, lifetime);

        impactEffectLifetime = Mathf.Max(0f, impactEffectLifetime);

        launchSfxVolume = Mathf.Clamp01(launchSfxVolume);
        impactSfxVolume = Mathf.Clamp01(impactSfxVolume);
    }
}