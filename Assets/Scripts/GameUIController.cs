using System;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public sealed class GameUIController : MonoBehaviour
{
    private const string ScoreLabelName = "ScoreLabel";
    private const string HighScoreLabelName = "HighScoreLabel";
    private const string NewHighScoreLabelName = "NewHighScoreLabel";

    private const string RestartButtonName = "RestartButton";
    private const string PlayButtonName = "PlayButton";
    private const string ExitButtonName = "ExitButton";
    private const string MainMenuPanelName = "MainMenuPanel";

    private const string NewHighScoreVisibleClass = "new-high-score-text-show";
    private const string HighScoreFlashClass = "high-score-flash";

    private const int HighScoreFlashDurationMs = 250;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    private Label scoreLabel;
    private Label highScoreLabel;
    private Label newHighScoreLabel;

    private Button restartButton;
    private Button playButton;
    private Button exitButton;

    private VisualElement mainMenuPanel;

    public event Action PlayClicked;
    public event Action RestartClicked;
    public event Action ExitClicked;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        SetupUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromButtonEvents();
    }

    public void ShowStartMenu(int highScore)
    {
        SetDisplay(mainMenuPanel, DisplayStyle.Flex);

        SetDisplay(playButton, DisplayStyle.Flex);
        SetDisplay(exitButton, DisplayStyle.Flex);
        SetDisplay(restartButton, DisplayStyle.None);

        SetDisplay(scoreLabel, DisplayStyle.None);
        SetDisplay(highScoreLabel, DisplayStyle.None);
        SetDisplay(newHighScoreLabel, DisplayStyle.None);

        UpdateScore(0);
        UpdateHighScore(highScore);
        ResetNewHighScoreVisual();
    }

    public void ShowPlaying(int highScore)
    {
        SetDisplay(mainMenuPanel, DisplayStyle.None);

        SetDisplay(playButton, DisplayStyle.None);
        SetDisplay(exitButton, DisplayStyle.None);
        SetDisplay(restartButton, DisplayStyle.None);

        SetDisplay(scoreLabel, DisplayStyle.Flex);

        // Keep the high score hidden during gameplay to reduce UI clutter.
        SetDisplay(highScoreLabel, DisplayStyle.None);
        SetDisplay(newHighScoreLabel, DisplayStyle.None);

        UpdateScore(0);
        UpdateHighScore(highScore);
        ResetNewHighScoreVisual();
    }

    public void ShowGameOver(int score, int highScore, bool hasNewHighScore)
    {
        SetDisplay(mainMenuPanel, DisplayStyle.Flex);

        SetDisplay(playButton, DisplayStyle.None);
        SetDisplay(exitButton, DisplayStyle.Flex);
        SetDisplay(restartButton, DisplayStyle.Flex);

        SetDisplay(scoreLabel, DisplayStyle.Flex);
        SetDisplay(highScoreLabel, DisplayStyle.Flex);

        UpdateScore(score);
        UpdateHighScore(highScore);

        if (hasNewHighScore)
        {
            ShowNewHighScoreFeedback();
        }
        else
        {
            SetDisplay(newHighScoreLabel, DisplayStyle.None);
        }

        // The full-screen menu panel can overlap the restart button.
        // Bringing the button to front keeps it clickable.
        restartButton?.BringToFront();
    }

    public void UpdateScore(int score)
    {
        SetLabelText(scoreLabel, $"Score: {score}");
    }

    public void UpdateHighScore(int highScore)
    {
        SetLabelText(highScoreLabel, $"High Score: {highScore}");
    }

    private void SetupUI()
    {
        if (uiDocument == null)
        {
            Debug.LogWarning($"{nameof(GameUIController)}: UIDocument is not assigned.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        scoreLabel = root.Q<Label>(ScoreLabelName);
        highScoreLabel = root.Q<Label>(HighScoreLabelName);
        newHighScoreLabel = root.Q<Label>(NewHighScoreLabelName);

        restartButton = root.Q<Button>(RestartButtonName);
        playButton = root.Q<Button>(PlayButtonName);
        exitButton = root.Q<Button>(ExitButtonName);

        mainMenuPanel = root.Q<VisualElement>(MainMenuPanelName);

        SubscribeToButtonEvents();
    }

    private void SubscribeToButtonEvents()
    {
        if (playButton != null)
        {
            playButton.clicked += HandlePlayClicked;
        }

        if (restartButton != null)
        {
            restartButton.clicked += HandleRestartClicked;
        }

        if (exitButton != null)
        {
            exitButton.clicked += HandleExitClicked;
        }
    }

    private void UnsubscribeFromButtonEvents()
    {
        if (playButton != null)
        {
            playButton.clicked -= HandlePlayClicked;
        }

        if (restartButton != null)
        {
            restartButton.clicked -= HandleRestartClicked;
        }

        if (exitButton != null)
        {
            exitButton.clicked -= HandleExitClicked;
        }
    }

    private void HandlePlayClicked()
    {
        PlayClicked?.Invoke();
    }

    private void HandleRestartClicked()
    {
        RestartClicked?.Invoke();
    }

    private void HandleExitClicked()
    {
        ExitClicked?.Invoke();
    }

    private void ShowNewHighScoreFeedback()
    {
        if (newHighScoreLabel != null)
        {
            SetDisplay(newHighScoreLabel, DisplayStyle.Flex);
            newHighScoreLabel.AddToClassList(NewHighScoreVisibleClass);
        }

        if (highScoreLabel == null)
        {
            return;
        }

        highScoreLabel.AddToClassList(HighScoreFlashClass);

        // UI Toolkit schedules this visual reset after a short delay.
        highScoreLabel.schedule.Execute(() =>
        {
            highScoreLabel.RemoveFromClassList(HighScoreFlashClass);
        }).ExecuteLater(HighScoreFlashDurationMs);
    }

    private void ResetNewHighScoreVisual()
    {
        if (newHighScoreLabel != null)
        {
            newHighScoreLabel.RemoveFromClassList(NewHighScoreVisibleClass);
        }

        if (highScoreLabel != null)
        {
            highScoreLabel.RemoveFromClassList(HighScoreFlashClass);
        }
    }

    private void SetDisplay(VisualElement element, DisplayStyle displayStyle)
    {
        if (element != null)
        {
            element.style.display = displayStyle;
        }
    }

    private void SetLabelText(Label label, string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }
}