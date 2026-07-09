using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class Obstacle : MonoBehaviour
{
    private const float MinAllowedSize = 0.01f;
    private const float ImpactSpeedEpsilon = 0.01f;

    [Header("Size")]
    [SerializeField, Min(MinAllowedSize)] private float minSize = 0.1f;
    [SerializeField, Min(MinAllowedSize)] private float maxSize = 0.25f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float minSpeed = 50f;
    [SerializeField, Min(0f)] private float maxSpeed = 150f;
    [SerializeField, Min(0f)] private float maxSpinSpeed = 10f;

    [Header("Speed Limit")]
    [SerializeField] private bool clampMaxVelocity = true;
    [SerializeField, Min(0f)] private float maxVelocity = 12f;

    [Header("Bounce Effect")]
    [SerializeField] private GameObject bounceEffectPrefab;

    [SerializeField, Min(0f)] private float effectLifetime = 1f;

    [Header("Effect Scale")]
    [SerializeField, Min(0f)] private float minImpactSpeed = 1f;
    [SerializeField, Min(0f)] private float maxImpactSpeed = 10f;
    [SerializeField, Min(0f)] private float minEffectScale = 0.5f;
    [SerializeField, Min(0f)] private float maxEffectScale = 2f;

    private static int nextId;

    private int obstacleId;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        obstacleId = nextId++;
    }

    private void Start()
    {
        InitializeRandomSize();
        ApplyRandomMovement();
    }

    private void FixedUpdate()
    {
        ClampVelocityIfNeeded();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!CanSpawnBounceEffect(collision))
        {
            return;
        }

        SpawnBounceEffect(collision);
    }

    private void InitializeRandomSize()
    {
        float randomSize = Random.Range(minSize, maxSize);
        transform.localScale = new Vector3(randomSize, randomSize, 1f);
    }

    private void ApplyRandomMovement()
    {
        float currentSize = transform.localScale.x;
        float randomSpeed = Random.Range(minSpeed, maxSpeed) / currentSize;

        Vector2 randomDirection = GetRandomDirection();
        float randomTorque = Random.Range(-maxSpinSpeed, maxSpinSpeed);

        rb.AddForce(randomDirection * randomSpeed);
        rb.AddTorque(randomTorque);
    }

    private Vector2 GetRandomDirection()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Random.insideUnitCircle can very rarely return Vector2.zero.
        // In that case, use a safe fallback direction.
        if (randomDirection == Vector2.zero)
        {
            randomDirection = Vector2.right;
        }

        return randomDirection;
    }

    private void ClampVelocityIfNeeded()
    {
        if (!clampMaxVelocity || maxVelocity <= 0f)
        {
            return;
        }

        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }

    private bool CanSpawnBounceEffect(Collision2D collision)
    {
        if (bounceEffectPrefab == null || collision.contactCount == 0)
        {
            return false;
        }

        return !ShouldSkipDuplicateObstacleEffect(collision);
    }

    private bool ShouldSkipDuplicateObstacleEffect(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out Obstacle otherObstacle))
        {
            return false;
        }

        float mySpeed = rb.linearVelocity.magnitude;
        float otherSpeed = otherObstacle.rb.linearVelocity.magnitude;

        // When two obstacles collide, only one of them should spawn the effect.
        // The faster obstacle handles the effect. If speeds are almost equal,
        // the obstacle with the higher ID handles it.
        if (mySpeed < otherSpeed)
        {
            return true;
        }

        return Mathf.Approximately(mySpeed, otherSpeed) &&
               obstacleId < otherObstacle.obstacleId;
    }

    private void SpawnBounceEffect(Collision2D collision)
    {
        Vector2 contactPoint = collision.GetContact(0).point;

        GameObject bounceEffect = Instantiate(
            bounceEffectPrefab,
            contactPoint,
            Quaternion.identity
        );

        bounceEffect.transform.localScale = Vector3.one * CalculateEffectScale(collision);

        Destroy(bounceEffect, effectLifetime);
    }

    private float CalculateEffectScale(Collision2D collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;

        float impactPercent = Mathf.InverseLerp(
            minImpactSpeed,
            maxImpactSpeed,
            impactSpeed
        );

        return Mathf.Lerp(
            minEffectScale,
            maxEffectScale,
            impactPercent
        );
    }

    private void OnValidate()
    {
        minSize = Mathf.Max(MinAllowedSize, minSize);
        maxSize = Mathf.Max(minSize, maxSize);

        minSpeed = Mathf.Max(0f, minSpeed);
        maxSpeed = Mathf.Max(minSpeed, maxSpeed);

        maxSpinSpeed = Mathf.Max(0f, maxSpinSpeed);
        maxVelocity = Mathf.Max(0f, maxVelocity);

        effectLifetime = Mathf.Max(0f, effectLifetime);

        minImpactSpeed = Mathf.Max(0f, minImpactSpeed);
        maxImpactSpeed = Mathf.Max(minImpactSpeed + ImpactSpeedEpsilon, maxImpactSpeed);

        minEffectScale = Mathf.Max(0f, minEffectScale);
        maxEffectScale = Mathf.Max(minEffectScale, maxEffectScale);
    }
}