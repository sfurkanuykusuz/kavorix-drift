using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraShake2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform shakeTarget;

    [Header("Obstacle Explosion Shake")]
    [SerializeField, Min(0f)] private float explosionDuration = 0.16f;
    [SerializeField, Min(0f)] private float explosionStrength = 0.13f;
    [SerializeField, Min(1f)] private float explosionFrequency = 32f;

    [Header("Player Impact Shake")]
    [SerializeField, Min(0f)] private float playerImpactDuration = 0.18f;
    [SerializeField, Min(0f)] private float playerImpactStrength = 0.16f;
    [SerializeField, Min(1f)] private float playerImpactFrequency = 36f;

    [Header("Shield Impact Shake")]
    [SerializeField, Min(0f)] private float shieldImpactDuration = 0.12f;
    [SerializeField, Min(0f)] private float shieldImpactStrength = 0.09f;
    [SerializeField, Min(1f)] private float shieldImpactFrequency = 34f;

    [Header("Platform Multipliers")]
    [SerializeField, Range(0.1f, 1f)] private float mobileStrengthMultiplier = 0.75f;
    [SerializeField, Range(0.1f, 1f)] private float webGlStrengthMultiplier = 0.85f;

    private Vector3 baseLocalPosition;

    private float remainingDuration;
    private float totalDuration;
    private float currentStrength;
    private float currentFrequency;

    private float noiseSeedX;
    private float noiseSeedY;

    private bool isShaking;

    private void Awake()
    {
        if (shakeTarget == null)
        {
            shakeTarget = transform;
        }

        baseLocalPosition = shakeTarget.localPosition;

        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(1000f, 2000f);
    }

    private void LateUpdate()
    {
        if (shakeTarget == null)
        {
            return;
        }

        if (remainingDuration <= 0f)
        {
            ResetShakeIfNeeded();

            // Keep following any intentional camera position changes while not shaking.
            baseLocalPosition = shakeTarget.localPosition;
            return;
        }

        isShaking = true;

        remainingDuration -= Time.unscaledDeltaTime;

        float shakePercent = Mathf.Clamp01(remainingDuration / totalDuration);
        float damping = shakePercent * shakePercent;

        float time = Time.unscaledTime * currentFrequency;

        float offsetX = (Mathf.PerlinNoise(noiseSeedX, time) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(noiseSeedY, time) - 0.5f) * 2f;

        float finalStrength = currentStrength * damping * GetPlatformStrengthMultiplier();

        Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0f) * finalStrength;
        shakeTarget.localPosition = baseLocalPosition + shakeOffset;

        if (remainingDuration <= 0f)
        {
            ResetShakeIfNeeded();
        }
    }

    public void ShakeObstacleExplosion()
    {
        Shake(explosionDuration, explosionStrength, explosionFrequency);
    }

    public void ShakePlayerImpact()
    {
        Shake(playerImpactDuration, playerImpactStrength, playerImpactFrequency);
    }

    public void ShakeShieldImpact()
    {
        Shake(shieldImpactDuration, shieldImpactStrength, shieldImpactFrequency);
    }

    public void Shake(float duration, float strength, float frequency)
    {
        if (shakeTarget == null || duration <= 0f || strength <= 0f)
        {
            return;
        }

        if (!isShaking)
        {
            baseLocalPosition = shakeTarget.localPosition;
        }

        totalDuration = Mathf.Max(duration, 0.01f);
        remainingDuration = Mathf.Max(remainingDuration, duration);

        currentStrength = Mathf.Max(currentStrength, strength);
        currentFrequency = Mathf.Max(currentFrequency, frequency);
    }

    private void ResetShakeIfNeeded()
    {
        if (!isShaking)
        {
            return;
        }

        shakeTarget.localPosition = baseLocalPosition;

        remainingDuration = 0f;
        totalDuration = 0f;
        currentStrength = 0f;
        currentFrequency = 0f;

        isShaking = false;
    }

    private float GetPlatformStrengthMultiplier()
    {
        float multiplier = 1f;

        if (Application.isMobilePlatform)
        {
            multiplier *= mobileStrengthMultiplier;
        }

#if UNITY_WEBGL
        multiplier *= webGlStrengthMultiplier;
#endif

        return multiplier;
    }

    private void OnValidate()
    {
        explosionDuration = Mathf.Max(0f, explosionDuration);
        explosionStrength = Mathf.Max(0f, explosionStrength);
        explosionFrequency = Mathf.Max(1f, explosionFrequency);

        playerImpactDuration = Mathf.Max(0f, playerImpactDuration);
        playerImpactStrength = Mathf.Max(0f, playerImpactStrength);
        playerImpactFrequency = Mathf.Max(1f, playerImpactFrequency);

        shieldImpactDuration = Mathf.Max(0f, shieldImpactDuration);
        shieldImpactStrength = Mathf.Max(0f, shieldImpactStrength);
        shieldImpactFrequency = Mathf.Max(1f, shieldImpactFrequency);

        mobileStrengthMultiplier = Mathf.Clamp(mobileStrengthMultiplier, 0.1f, 1f);
        webGlStrengthMultiplier = Mathf.Clamp(webGlStrengthMultiplier, 0.1f, 1f);
    }
}