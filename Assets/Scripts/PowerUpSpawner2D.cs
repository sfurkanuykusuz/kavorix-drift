using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PowerUpSpawner2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PowerUpPickup2D pickupPrefab;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Area")]
    [SerializeField, Min(0f)] private float borderSafetyPadding = 2f;
    [SerializeField, Min(0f)] private float minDistanceFromPlayer = 2.5f;
    [SerializeField, Min(1)] private int maxSpawnAttempts = 20;

    private PowerUpPickup2D activePickup;

    public event Action PowerUpPickupCollected;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        UnsubscribeFromActivePickup();
    }

    private void ResolveReferences()
    {
        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (playerTransform == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            playerTransform = player != null ? player.transform : null;
        }
    }

    private void SubscribeToEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreMilestoneReached += HandleScoreMilestoneReached;
        }
        else
        {
            Debug.LogWarning($"{nameof(PowerUpSpawner2D)}: ScoreManager was not found.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreMilestoneReached -= HandleScoreMilestoneReached;
        }
    }

    private void HandleScoreMilestoneReached(int milestoneScore)
    {
        if (activePickup != null)
        {
            return;
        }

        SpawnPickup();
    }

    private void SpawnPickup()
    {
        if (pickupPrefab == null)
        {
            Debug.LogWarning($"{nameof(PowerUpSpawner2D)}: Pickup prefab is not assigned.");
            return;
        }

        Vector2 spawnPosition = GetValidSpawnPosition();

        activePickup = Instantiate(
            pickupPrefab,
            spawnPosition,
            Quaternion.identity
        );

        activePickup.Collected += HandlePickupCollected;
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 fallbackPosition = GetCameraCenter();

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            if (!TryGetRandomPositionInsideSafeArea(out Vector2 randomPosition))
            {
                return fallbackPosition;
            }

            if (IsFarEnoughFromPlayer(randomPosition))
            {
                return randomPosition;
            }
        }

        return fallbackPosition;
    }

    private bool TryGetRandomPositionInsideSafeArea(out Vector2 position)
    {
        position = Vector2.zero;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || !targetCamera.orthographic)
        {
            return false;
        }

        Vector2 cameraCenter = targetCamera.transform.position;

        float halfHeight = targetCamera.orthographicSize - borderSafetyPadding;
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect - borderSafetyPadding;

        if (halfWidth <= 0f || halfHeight <= 0f)
        {
            position = cameraCenter;
            return false;
        }

        float randomX = UnityEngine.Random.Range(cameraCenter.x - halfWidth, cameraCenter.x + halfWidth);
        float randomY = UnityEngine.Random.Range(cameraCenter.y - halfHeight, cameraCenter.y + halfHeight);

        position = new Vector2(randomX, randomY);
        return true;
    }

    private Vector2 GetCameraCenter()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        return targetCamera != null ? (Vector2)targetCamera.transform.position : Vector2.zero;
    }

    private bool IsFarEnoughFromPlayer(Vector2 position)
    {
        if (playerTransform == null || minDistanceFromPlayer <= 0f)
        {
            return true;
        }

        float sqrDistance = ((Vector2)playerTransform.position - position).sqrMagnitude;
        return sqrDistance >= minDistanceFromPlayer * minDistanceFromPlayer;
    }

    private void HandlePickupCollected(PowerUpPickup2D pickup)
    {
        if (pickup == activePickup)
        {
            UnsubscribeFromActivePickup();
            activePickup = null;
        }

        PowerUpPickupCollected?.Invoke();
    }

    private void UnsubscribeFromActivePickup()
    {
        if (activePickup == null)
        {
            return;
        }

        activePickup.Collected -= HandlePickupCollected;
    }

    private void OnValidate()
    {
        borderSafetyPadding = Mathf.Max(0f, borderSafetyPadding);
        minDistanceFromPlayer = Mathf.Max(0f, minDistanceFromPlayer);
        maxSpawnAttempts = Mathf.Max(1, maxSpawnAttempts);
    }
}