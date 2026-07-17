using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class GameUIResponsiveLayout : MonoBehaviour
{
    private const string CompactClassName = "ui-compact";
    private const string UltraCompactClassName = "ui-ultra-compact";

    [Header("Compact Layout Thresholds")]
    [SerializeField] private float compactWidthThreshold = 720f;
    [SerializeField] private float compactHeightThreshold = 480f;

    [Header("Ultra Compact Layout Thresholds")]
    [SerializeField] private float ultraCompactWidthThreshold = 540f;
    [SerializeField] private float ultraCompactHeightThreshold = 360f;

    private UIDocument uiDocument;
    private VisualElement root;

    private float lastWidth = -1f;
    private float lastHeight = -1f;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        if (root == null)
        {
            return;
        }

        root.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
        ApplyLayoutClass(root.resolvedStyle.width, root.resolvedStyle.height);
    }

    private void OnDisable()
    {
        if (root == null)
        {
            return;
        }

        root.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
        root.RemoveFromClassList(CompactClassName);
        root.RemoveFromClassList(UltraCompactClassName);
    }

    private void HandleGeometryChanged(GeometryChangedEvent geometryChangedEvent)
    {
        ApplyLayoutClass(geometryChangedEvent.newRect.width, geometryChangedEvent.newRect.height);
    }

    private void ApplyLayoutClass(float width, float height)
    {
        if (root == null || width <= 0f || height <= 0f)
        {
            return;
        }

        if (Mathf.Approximately(width, lastWidth) && Mathf.Approximately(height, lastHeight))
        {
            return;
        }

        lastWidth = width;
        lastHeight = height;

        bool useUltraCompactLayout = width <= ultraCompactWidthThreshold || height <= ultraCompactHeightThreshold;
        bool useCompactLayout = useUltraCompactLayout || width <= compactWidthThreshold || height <= compactHeightThreshold;

        root.EnableInClassList(CompactClassName, useCompactLayout);
        root.EnableInClassList(UltraCompactClassName, useUltraCompactLayout);
    }
}