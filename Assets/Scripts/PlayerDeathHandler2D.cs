using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerDeathHandler2D : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody2D rb;
    private Collider2D[] playerColliders;
    private SpriteRenderer[] playerSpriteRenderers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        playerSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void KillPlayer()
    {
        SpawnExplosionEffect();
        SetPhysicsEnabled(false);
        SetVisualsEnabled(false);
    }

    public void ShowPlayerForNewGame()
    {
        SetPhysicsEnabled(true);
        SetVisualsEnabled(true);
    }

    private void SpawnExplosionEffect()
    {
        if (explosionEffect == null)
        {
            return;
        }

        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    private void SetPhysicsEnabled(bool isEnabled)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = isEnabled;

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider != null)
            {
                playerCollider.enabled = isEnabled;
            }
        }
    }

    private void SetVisualsEnabled(bool isEnabled)
    {
        foreach (SpriteRenderer spriteRenderer in playerSpriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isEnabled;
            }
        }
    }
}