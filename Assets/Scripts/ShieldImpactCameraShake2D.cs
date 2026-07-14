using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShieldImpactCameraShake2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShield2D playerShield;
    [SerializeField] private CameraShake2D cameraShake;

    [Header("Filtering")]
    [SerializeField] private bool onlyShakeWhenShieldActive = true;
    [SerializeField, Min(0f)] private float minShakeInterval = 0.08f;

    private float nextAllowedShakeTime;

    private void Awake()
    {
        if (playerShield == null)
        {
            playerShield = GetComponentInParent<PlayerShield2D>();
        }

        if (cameraShake == null)
        {
            cameraShake = FindAnyObjectByType<CameraShake2D>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryShake();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryShake();
    }

    private void TryShake()
    {
        if (!CanShake())
        {
            return;
        }

        nextAllowedShakeTime = Time.unscaledTime + minShakeInterval;
        cameraShake.ShakeShieldImpact();
    }

    private bool CanShake()
    {
        if (cameraShake == null)
        {
            return false;
        }

        if (Time.unscaledTime < nextAllowedShakeTime)
        {
            return false;
        }

        if (!onlyShakeWhenShieldActive)
        {
            return true;
        }

        return playerShield != null && playerShield.IsActive;
    }

    private void OnValidate()
    {
        minShakeInterval = Mathf.Max(0f, minShakeInterval);
    }
}