using UnityEngine;

[DisallowMultipleComponent]
public sealed class BorderImpactAudio2D : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip borderHitSfx;

    [Header("Impact")]
    [SerializeField, Min(0f)] private float minImpactSpeed = 1f;
    [SerializeField, Min(0.01f)] private float maxImpactSpeed = 8f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float minVolume = 0.15f;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 0.65f;

    [Header("Pitch")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Header("Cooldown")]
    [SerializeField, Min(0f)] private float localCooldown = 0.06f;
    [SerializeField, Min(0f)] private float globalCooldown = 0.03f;

    private static float nextGlobalPlayTime;

    private float nextLocalPlayTime;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!CanPlayCollision(collision))
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minImpactSpeed)
        {
            return;
        }

        PlayBorderHitSfx(impactSpeed);
    }

    private void ResolveReferences()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = GetComponentInParent<AudioSource>();
        }
    }

    private bool CanPlayCollision(Collision2D collision)
    {
        if (collision.gameObject.GetComponentInParent<Obstacle>() == null)
        {
            return false;
        }

        if (Time.unscaledTime < nextLocalPlayTime)
        {
            return false;
        }

        if (Time.unscaledTime < nextGlobalPlayTime)
        {
            return false;
        }

        return true;
    }

    private void PlayBorderHitSfx(float impactSpeed)
    {
        if (!CanUseAudioSource())
        {
            return;
        }

        float normalizedImpact = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactSpeed);
        float volume = Mathf.Lerp(minVolume, maxVolume, normalizedImpact);

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(borderHitSfx, volume);

        nextLocalPlayTime = Time.unscaledTime + localCooldown;
        nextGlobalPlayTime = Time.unscaledTime + globalCooldown;
    }

    private bool CanUseAudioSource()
    {
        if (audioSource == null || borderHitSfx == null)
        {
            return false;
        }

        if (!audioSource.enabled)
        {
            return false;
        }

        if (!audioSource.gameObject.activeInHierarchy)
        {
            return false;
        }

        return true;
    }

    private void OnValidate()
    {
        minImpactSpeed = Mathf.Max(0f, minImpactSpeed);
        maxImpactSpeed = Mathf.Max(0.01f, maxImpactSpeed);

        if (maxImpactSpeed < minImpactSpeed)
        {
            maxImpactSpeed = minImpactSpeed + 0.01f;
        }

        minVolume = Mathf.Clamp01(minVolume);
        maxVolume = Mathf.Clamp01(maxVolume);

        if (maxVolume < minVolume)
        {
            maxVolume = minVolume;
        }

        pitchRange.x = Mathf.Max(0.1f, pitchRange.x);
        pitchRange.y = Mathf.Max(0.1f, pitchRange.y);

        if (pitchRange.y < pitchRange.x)
        {
            pitchRange.y = pitchRange.x;
        }

        localCooldown = Mathf.Max(0f, localCooldown);
        globalCooldown = Mathf.Max(0f, globalCooldown);
    }
}