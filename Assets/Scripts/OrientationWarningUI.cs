using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public sealed class OrientationWarningUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Behaviour")]
    [SerializeField] private bool pauseGameInPortrait = true;

    [Header("Element Names")]
    [SerializeField] private string rotateWarningOverlayName = "RotateWarningOverlay";

    [SerializeField] private string[] elementsToHideInPortrait =
    {
        "ScoreLabel",
        "HighScoreLabel",
        "NewHighScoreLabel",
        "RestartButton"
    };

    private VisualElement rotateWarningOverlay;
    private readonly List<VisualElement> gameplayElementsToHide = new List<VisualElement>();

    private bool isInitialized;
    private bool hasAppliedInitialState;
    private bool pausedByThisScript;

    private bool? lastIsPortrait;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        hasAppliedInitialState = false;
        lastIsPortrait = null;
    }

    private void OnDisable()
    {
        HideRotateWarning();
        SetGameplayUIVisible(true);
        ResumeGameIfPausedByThisScript();
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        // Apply the first state in Update instead of Awake.
        // This avoids capturing Time.timeScale before other scripts finish their Start logic.
        bool forceRefresh = !hasAppliedInitialState;

        UpdateOrientationState(forceRefresh);

        hasAppliedInitialState = true;
    }

    private void Initialize()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogWarning($"{nameof(OrientationWarningUI)}: UIDocument is not assigned.");
            return;
        }

        CacheUIElements();

        isInitialized = true;
    }

    private void CacheUIElements()
    {
        VisualElement root = uiDocument.rootVisualElement;

        rotateWarningOverlay = root.Q<VisualElement>(rotateWarningOverlayName);

        if (rotateWarningOverlay == null)
        {
            Debug.LogWarning(
                $"{nameof(OrientationWarningUI)}: UI element '{rotateWarningOverlayName}' was not found."
            );
        }
        else
        {
            // The overlay should block clicks/touches while the warning is visible.
            rotateWarningOverlay.pickingMode = PickingMode.Position;
        }

        gameplayElementsToHide.Clear();

        foreach (string elementName in elementsToHideInPortrait)
        {
            if (string.IsNullOrWhiteSpace(elementName))
            {
                continue;
            }

            VisualElement element = root.Q<VisualElement>(elementName);

            if (element != null)
            {
                gameplayElementsToHide.Add(element);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(OrientationWarningUI)}: UI element '{elementName}' was not found."
                );
            }
        }
    }

    private void UpdateOrientationState(bool forceRefresh)
    {
        bool isPortrait = IsPortrait();

        if (forceRefresh || lastIsPortrait != isPortrait)
        {
            SetRotateWarningVisible(isPortrait);
            SetGameplayUIVisible(!isPortrait);

            lastIsPortrait = isPortrait;
        }

        UpdatePauseState(isPortrait);
    }

    private bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }

    private void SetRotateWarningVisible(bool isVisible)
    {
        if (rotateWarningOverlay == null)
        {
            return;
        }

        rotateWarningOverlay.style.display = isVisible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    private void HideRotateWarning()
    {
        SetRotateWarningVisible(false);
    }

    private void SetGameplayUIVisible(bool isVisible)
    {
        Visibility visibility = isVisible
            ? Visibility.Visible
            : Visibility.Hidden;

        foreach (VisualElement element in gameplayElementsToHide)
        {
            if (element != null)
            {
                element.style.visibility = visibility;
            }
        }
    }

    private void UpdatePauseState(bool isPortrait)
    {
        if (!pauseGameInPortrait)
        {
            ResumeGameIfPausedByThisScript();
            return;
        }

        if (isPortrait)
        {
            PauseGame();
        }
        else
        {
            ResumeGameIfPausedByThisScript();
        }
    }

    private void PauseGame()
    {
        if (pausedByThisScript)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        pausedByThisScript = true;
    }

    private void ResumeGameIfPausedByThisScript()
    {
        if (!pausedByThisScript)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        pausedByThisScript = false;
    }
}