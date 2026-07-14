using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement2D))]
public sealed class MissileTargetSelector2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuidedMissile2D missilePrefab;
    [SerializeField] private Transform missileSpawnPoint;
    [SerializeField] private Camera targetCamera;

    [Header("Target Selection")]
    [SerializeField, Min(0f)] private float targetSelectionRadius = 0.5f;
    [SerializeField] private LayerMask obstacleLayerMask = ~0;

    [Header("Target Indicator")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private Vector3 targetIndicatorOffset;

    [Header("Spawn")]
    [SerializeField, Min(0f)] private float fallbackSpawnOffset = 0.75f;

    private PlayerController playerController;
    private PlayerMovement2D movement;

    private bool isSelectingTarget;

    public bool IsSelectingTarget => isSelectingTarget;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement2D>();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!isSelectingTarget)
        {
            return;
        }

        if (!CanSelectTarget())
        {
            CancelTargetSelection();
            return;
        }

        if (!WasPointerPressedThisFrame())
        {
            return;
        }

        TrySelectTargetUnderPointer();
    }

    public void BeginTargetSelection()
    {
        if (!CanSelectTarget())
        {
            return;
        }

        if (missilePrefab == null)
        {
            Debug.LogWarning($"{nameof(MissileTargetSelector2D)}: Missile prefab is not assigned.");
            return;
        }

        isSelectingTarget = true;
        Debug.Log($"{nameof(MissileTargetSelector2D)}: Select an obstacle.");
    }

    public void CancelTargetSelection()
    {
        isSelectingTarget = false;
    }

    private bool CanSelectTarget()
    {
        if (playerController == null)
        {
            Debug.LogWarning($"{nameof(MissileTargetSelector2D)}: PlayerController was not found.");
            return false;
        }

        return playerController.IsGamePlaying;
    }

    private bool WasPointerPressedThisFrame()
    {
        bool mousePressed = Mouse.current != null &&
                            Mouse.current.leftButton.wasPressedThisFrame;

        bool touchPressed = Touchscreen.current != null &&
                            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        return mousePressed || touchPressed;
    }

    private void TrySelectTargetUnderPointer()
    {
        if (!CanSelectTarget())
        {
            CancelTargetSelection();
            return;
        }

        if (!TryGetPointerWorldPosition(out Vector2 pointerWorldPosition))
        {
            return;
        }

        Obstacle target = FindObstacleAtPosition(pointerWorldPosition);

        if (target == null)
        {
            Debug.Log($"{nameof(MissileTargetSelector2D)}: No obstacle selected.");
            return;
        }

        FireMissile(target);
        isSelectingTarget = false;
    }

    private bool TryGetPointerWorldPosition(out Vector2 worldPosition)
    {
        if (movement != null && movement.TryGetPointerWorldPosition(out worldPosition))
        {
            return true;
        }

        worldPosition = Vector2.zero;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return false;
        }

        Vector2 screenPosition;

        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            return false;
        }

        Vector3 worldPoint = targetCamera.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0f;

        worldPosition = worldPoint;
        return true;
    }

    private Obstacle FindObstacleAtPosition(Vector2 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            worldPosition,
            targetSelectionRadius,
            obstacleLayerMask
        );

        Obstacle closestObstacle = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Obstacle obstacle = hit.GetComponentInParent<Obstacle>();

            if (obstacle == null)
            {
                continue;
            }

            float sqrDistance = ((Vector2)obstacle.transform.position - worldPosition).sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestObstacle = obstacle;
            }
        }

        return closestObstacle;
    }

    private void FireMissile(Obstacle target)
    {
        if (!CanSelectTarget())
        {
            return;
        }

        Vector2 spawnPosition = GetMissileSpawnPosition();
        Quaternion spawnRotation = Quaternion.LookRotation(Vector3.forward, movement.FacingDirection);

        GameObject indicator = SpawnTargetIndicator(target);

        GuidedMissile2D missile = Instantiate(
            missilePrefab,
            spawnPosition,
            spawnRotation
        );

        missile.SetTarget(target, indicator);
    }

    private GameObject SpawnTargetIndicator(Obstacle target)
    {
        if (targetIndicatorPrefab == null || target == null)
        {
            return null;
        }

        GameObject indicator = Instantiate(
            targetIndicatorPrefab,
            target.transform.position,
            Quaternion.identity
        );

        TargetIndicatorFollower2D follower = indicator.GetComponent<TargetIndicatorFollower2D>();

        if (follower == null)
        {
            follower = indicator.AddComponent<TargetIndicatorFollower2D>();
        }

        follower.Initialize(target, targetIndicatorOffset);

        return indicator;
    }

    private Vector2 GetMissileSpawnPosition()
    {
        if (missileSpawnPoint != null)
        {
            return missileSpawnPoint.position;
        }

        return (Vector2)transform.position + movement.FacingDirection.normalized * fallbackSpawnOffset;
    }

    private void OnValidate()
    {
        targetSelectionRadius = Mathf.Max(0f, targetSelectionRadius);
        fallbackSpawnOffset = Mathf.Max(0f, fallbackSpawnOffset);
    }
}