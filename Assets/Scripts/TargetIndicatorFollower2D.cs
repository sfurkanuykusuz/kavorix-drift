using UnityEngine;

[DisallowMultipleComponent]
public sealed class TargetIndicatorFollower2D : MonoBehaviour
{
    private enum CenterMode
    {
        ColliderBounds,
        RendererBounds,
        TransformPosition
    }

    [Header("Target")]
    [SerializeField] private Obstacle targetObstacle;
    [SerializeField] private CenterMode centerMode = CenterMode.ColliderBounds;
    [SerializeField] private Vector3 offset;

    [Header("Rotation")]
    [SerializeField] private bool keepWorldRotation = true;
    [SerializeField] private Vector3 worldRotation;

    public void Initialize(Obstacle target, Vector3 targetOffset)
    {
        targetObstacle = target;
        offset = targetOffset;

        FollowTarget();
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (targetObstacle == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = GetTargetCenter() + offset;

        if (keepWorldRotation)
        {
            transform.rotation = Quaternion.Euler(worldRotation);
        }
    }

    private Vector3 GetTargetCenter()
    {
        switch (centerMode)
        {
            case CenterMode.ColliderBounds:
                if (TryGetColliderBoundsCenter(out Vector3 colliderCenter))
                {
                    return colliderCenter;
                }

                if (TryGetRendererBoundsCenter(out Vector3 rendererCenterFallback))
                {
                    return rendererCenterFallback;
                }

                return targetObstacle.transform.position;

            case CenterMode.RendererBounds:
                if (TryGetRendererBoundsCenter(out Vector3 rendererCenter))
                {
                    return rendererCenter;
                }

                if (TryGetColliderBoundsCenter(out Vector3 colliderCenterFallback))
                {
                    return colliderCenterFallback;
                }

                return targetObstacle.transform.position;

            case CenterMode.TransformPosition:
                return targetObstacle.transform.position;

            default:
                return targetObstacle.transform.position;
        }
    }

    private bool TryGetColliderBoundsCenter(out Vector3 center)
    {
        center = Vector3.zero;

        Collider2D[] colliders = targetObstacle.GetComponentsInChildren<Collider2D>();

        bool hasBounds = false;
        Bounds combinedBounds = default;

        foreach (Collider2D targetCollider in colliders)
        {
            if (targetCollider == null || !targetCollider.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = targetCollider.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(targetCollider.bounds);
            }
        }

        if (!hasBounds)
        {
            return false;
        }

        center = combinedBounds.center;
        return true;
    }

    private bool TryGetRendererBoundsCenter(out Vector3 center)
    {
        center = Vector3.zero;

        Renderer[] renderers = targetObstacle.GetComponentsInChildren<Renderer>();

        bool hasBounds = false;
        Bounds combinedBounds = default;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null || !targetRenderer.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = targetRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(targetRenderer.bounds);
            }
        }

        if (!hasBounds)
        {
            return false;
        }

        center = combinedBounds.center;
        return true;
    }
}