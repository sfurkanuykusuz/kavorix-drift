using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class ResponsiveGameArea : MonoBehaviour
{
    private const float MinTargetSize = 0.01f;

    [Header("Target Game Area")]
    [SerializeField, Min(MinTargetSize)] private float targetWidth = 27.2f;
    [SerializeField, Min(MinTargetSize)] private float targetHeight = 14.8622f;
    [SerializeField, Min(0f)] private float padding = 0.15f;

    [Header("Background")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField, Min(0f)] private float backgroundExtraScale = 1.05f;
    [SerializeField] private Color cameraBackgroundColor = new Color(0.005f, 0.01f, 0.03f, 1f);

    [Header("Borders")]
    [SerializeField] private Transform topBorder;
    [SerializeField] private Transform bottomBorder;
    [SerializeField] private Transform leftBorder;
    [SerializeField] private Transform rightBorder;

    [Header("Border Thickness")]
    [SerializeField, Min(0f)] private float horizontalBorderThickness = 0.65f;
    [SerializeField, Min(0f)] private float verticalBorderThickness = 0.65f;

    private Camera cam;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private float lastCameraAspect = -1f;
    private Vector3 lastCameraPosition;

    private void Reset()
    {
        CacheCamera();
    }

    private void Awake()
    {
        CacheCamera();
        ApplyLayout();
    }

    private void OnEnable()
    {
        CacheCamera();
        ApplyLayout();
    }

    private void OnValidate()
    {
        ValidateValues();
        CacheCamera();
        ApplyLayout();
    }

    private void LateUpdate()
    {
        if (ShouldRefreshLayout())
        {
            ApplyLayout();
        }
    }

    private void CacheCamera()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
    }

    private bool ShouldRefreshLayout()
    {
        if (cam == null)
        {
            CacheCamera();
        }

        if (cam == null)
        {
            return false;
        }

        return Screen.width != lastScreenWidth ||
               Screen.height != lastScreenHeight ||
               !Mathf.Approximately(cam.aspect, lastCameraAspect) ||
               cam.transform.position != lastCameraPosition;
    }

    private void ApplyLayout()
    {
        if (cam == null)
        {
            return;
        }

        cam.backgroundColor = cameraBackgroundColor;

        if (!cam.orthographic)
        {
            return;
        }

        FitCameraToTargetArea();

        Vector2 cameraSize = GetCameraSize();
        Vector3 cameraPosition = cam.transform.position;

        FitBackground(cameraPosition, cameraSize);
        FitBorders(cameraPosition, cameraSize);

        CacheCurrentLayoutState();
    }

    private void FitCameraToTargetArea()
    {
        float aspect = cam.aspect;

        if (aspect <= 0f)
        {
            return;
        }

        float sizeByHeight = targetHeight / 2f;
        float sizeByWidth = targetWidth / (2f * aspect);

        // The camera must fit both the target width and target height.
        // The larger orthographic size guarantees that the full game area is visible.
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) + padding;
    }

    private Vector2 GetCameraSize()
    {
        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        return new Vector2(cameraWidth, cameraHeight);
    }

    private void FitBackground(Vector3 cameraPosition, Vector2 cameraSize)
    {
        if (backgroundRenderer == null || backgroundRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;

        if (!IsValidSpriteSize(spriteSize))
        {
            return;
        }

        float scaleX = cameraSize.x / spriteSize.x;
        float scaleY = cameraSize.y / spriteSize.y;
        float finalScale = Mathf.Max(scaleX, scaleY) * backgroundExtraScale;

        Transform backgroundTransform = backgroundRenderer.transform;

        backgroundTransform.position = new Vector3(
            cameraPosition.x,
            cameraPosition.y,
            backgroundTransform.position.z
        );

        backgroundTransform.localScale = new Vector3(
            finalScale,
            finalScale,
            backgroundTransform.localScale.z
        );
    }

    private void FitBorders(Vector3 cameraPosition, Vector2 cameraSize)
    {
        FitHorizontalBorder(topBorder, cameraPosition, cameraSize, true);
        FitHorizontalBorder(bottomBorder, cameraPosition, cameraSize, false);

        FitVerticalBorder(leftBorder, cameraPosition, cameraSize, true);
        FitVerticalBorder(rightBorder, cameraPosition, cameraSize, false);
    }

    private void FitHorizontalBorder(
        Transform border,
        Vector3 cameraPosition,
        Vector2 cameraSize,
        bool isTopBorder
    )
    {
        if (!TryGetSpriteSize(border, out Vector2 spriteSize))
        {
            return;
        }

        float lengthScale = cameraSize.x / spriteSize.x;
        float halfThickness = spriteSize.y * horizontalBorderThickness / 2f;

        float yOffset = cameraSize.y / 2f - halfThickness;

        if (!isTopBorder)
        {
            yOffset *= -1f;
        }

        border.localScale = new Vector3(
            lengthScale,
            horizontalBorderThickness,
            border.localScale.z
        );

        border.position = new Vector3(
            cameraPosition.x,
            cameraPosition.y + yOffset,
            border.position.z
        );
    }

    private void FitVerticalBorder(
        Transform border,
        Vector3 cameraPosition,
        Vector2 cameraSize,
        bool isLeftBorder
    )
    {
        if (!TryGetSpriteSize(border, out Vector2 spriteSize))
        {
            return;
        }

        // The vertical borders are expected to use their local X axis as length.
        // This works well when the border sprite/transform is rotated vertically in the scene.
        float lengthScale = cameraSize.y / spriteSize.x;
        float halfThickness = spriteSize.y * verticalBorderThickness / 2f;

        float xOffset = cameraSize.x / 2f - halfThickness;

        if (isLeftBorder)
        {
            xOffset *= -1f;
        }

        border.localScale = new Vector3(
            lengthScale,
            verticalBorderThickness,
            border.localScale.z
        );

        border.position = new Vector3(
            cameraPosition.x + xOffset,
            cameraPosition.y,
            border.position.z
        );
    }

    private bool TryGetSpriteSize(Transform targetTransform, out Vector2 spriteSize)
    {
        spriteSize = Vector2.zero;

        if (targetTransform == null)
        {
            return false;
        }

        if (!targetTransform.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            return false;
        }

        if (spriteRenderer.sprite == null)
        {
            return false;
        }

        spriteSize = spriteRenderer.sprite.bounds.size;

        return IsValidSpriteSize(spriteSize);
    }

    private bool IsValidSpriteSize(Vector2 spriteSize)
    {
        return spriteSize.x > 0f && spriteSize.y > 0f;
    }

    private void CacheCurrentLayoutState()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastCameraAspect = cam.aspect;
        lastCameraPosition = cam.transform.position;
    }

    private void ValidateValues()
    {
        targetWidth = Mathf.Max(MinTargetSize, targetWidth);
        targetHeight = Mathf.Max(MinTargetSize, targetHeight);

        padding = Mathf.Max(0f, padding);

        backgroundExtraScale = Mathf.Max(0f, backgroundExtraScale);

        horizontalBorderThickness = Mathf.Max(0f, horizontalBorderThickness);
        verticalBorderThickness = Mathf.Max(0f, verticalBorderThickness);
    }
}