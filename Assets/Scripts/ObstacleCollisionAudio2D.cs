using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Obstacle))]
public sealed class ObstacleCollisionAudio2D : MonoBehaviour
{
    private const float ImpactSpeedEpsilon = 0.01f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip obstacleHitSfx;
    [SerializeField, Range(0f, 1f)] private float volume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float minVolumeMultiplier = 0.3f;

    [Header("Impact Filtering")]
    [SerializeField, Min(0f)] private float minImpactSpeed = 0.5f;
    [SerializeField, Min(0f)] private float maxImpactSpeed = 8f;
    [SerializeField, Min(0f)] private float globalSfxCooldown = 0.05f;

    private static int nextAudioId;
    private static float nextAllowedGlobalSfxTime;

    private int audioId;
    private Obstacle obstacle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        nextAudioId = 0;
        nextAllowedGlobalSfxTime = 0f;
    }

    private void Awake()
    {
        audioId = nextAudioId++;
        obstacle = GetComponent<Obstacle>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ObstacleCollisionAudio2D otherAudio =
            collision.gameObject.GetComponentInParent<ObstacleCollisionAudio2D>();

        if (!CanPlayCollisionSfx(otherAudio))
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minImpactSpeed)
        {
            return;
        }

        PlayObstacleHitSfx(impactSpeed);
    }

    private bool CanPlayCollisionSfx(ObstacleCollisionAudio2D otherAudio)
    {
        if (obstacle == null || otherAudio == null || otherAudio == this)
        {
            return false;
        }

        Obstacle otherObstacle = otherAudio.obstacle;

        if (otherObstacle == null)
        {
            return false;
        }

        // Both obstacles receive the same collision event.
        // Only one of them should play the sound to avoid duplicate audio.
        float mySpeed = obstacle.CurrentSpeed;
        float otherSpeed = otherObstacle.CurrentSpeed;

        if (mySpeed < otherSpeed)
        {
            return false;
        }

        if (Mathf.Approximately(mySpeed, otherSpeed))
        {
            return audioId > otherAudio.audioId;
        }

        return true;
    }

    private void PlayObstacleHitSfx(float impactSpeed)
    {
        if (audioSource == null || obstacleHitSfx == null)
        {
            return;
        }

        if (Time.unscaledTime < nextAllowedGlobalSfxTime)
        {
            return;
        }

        nextAllowedGlobalSfxTime = Time.unscaledTime + globalSfxCooldown;

        float impactPercent = Mathf.InverseLerp(
            minImpactSpeed,
            maxImpactSpeed,
            impactSpeed
        );

        float finalVolume = volume * Mathf.Lerp(
            minVolumeMultiplier,
            1f,
            impactPercent
        );

        audioSource.PlayOneShot(obstacleHitSfx, finalVolume);
    }

    private void OnValidate()
    {
        volume = Mathf.Clamp01(volume);
        minVolumeMultiplier = Mathf.Clamp01(minVolumeMultiplier);

        minImpactSpeed = Mathf.Max(0f, minImpactSpeed);
        maxImpactSpeed = Mathf.Max(minImpactSpeed + ImpactSpeedEpsilon, maxImpactSpeed);

        globalSfxCooldown = Mathf.Max(0f, globalSfxCooldown);
    }
}