using System;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public sealed class GameUIController : MonoBehaviour
{
    private const string ScoreLabelName = "ScoreLabel";
    private const string HighScoreLabelName = "HighScoreLabel";
    private const string NewHighScoreLabelName = "NewHighScoreLabel";
    private const string CountdownLabelName = "CountdownLabel";

    private const string RestartButtonName = "RestartButton";
    private const string PlayButtonName = "PlayButton";
    private const string ExitButtonName = "ExitButton";
    private const string MainMenuPanelName = "MainMenuPanel";

    private const string NewHighScoreVisibleClass = "new-high-score-text-show";
    private const string HighScoreFlashClass = "high-score-flash";

    private const string GameOverButtonEnterClass = "game-over-button-enter";
    private const string GameOverButtonEnterVisibleClass = "game-over-button-enter-show";

    private const int HighScoreFlashDurationMs = 250;
    private const int GameOverButtonAnimationDelayMs = 50;

    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Button SFX")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip buttonHoverSfx;
    [SerializeField] private AudioClip buttonClickSfx;
    [SerializeField, Range(0f, 1f)] private float hoverSfxVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float clickSfxVolume = 0.7f;

    [Header("Countdown SFX")]
    [SerializeField] private AudioClip countdownSfx;
    [SerializeField, Range(0f, 1f)] private float countdownSfxVolume = 0.8f;

    [Header("Game Over")]
    [SerializeField, Min(0f)] private float gameOverButtonDelay = 0.7f;

    private VisualElement root;

    private Label scoreLabel;
    private Label highScoreLabel;
    private Label newHighScoreLabel;
    private Label countdownLabel;

    private Button restartButton;
    private Button playButton;
    private Button exitButton;

    private VisualElement mainMenuPanel;

    private IVisualElementScheduledItem showGameOverButtonsSchedule;
    private IVisualElementScheduledItem animateGameOverButtonsSchedule;

    public event Action PlayClicked;
    public event Action RestartClicked;
    public event Action ExitClicked;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
        }

        SetupUI();
    }

    private void OnDestroy()
    {
        CancelGameOverButtonSchedules();
        UnsubscribeFromButtonEvents();
    }

    public void ShowStartMenu(int highScore)
    {
        CancelGameOverButtonSchedules();
        ResetGameOverButtonVisuals();
        ResetNewHighScoreVisual();
        HideCountdown();

        SetDisplay(mainMenuPanel, DisplayStyle.Flex);

        SetButtonVisibleAndEnabled(playButton, true, true);
        SetButtonVisibleAndEnabled(exitButton, true, true);
        SetButtonVisibleAndEnabled(restartButton, false, false);

        SetScoreUIVisible(false, false, false);

        UpdateScore(0);
        UpdateHighScore(highScore);
    }

    public void ShowPlaying(int highScore)
    {
        CancelGameOverButtonSchedules();
        ResetGameOverButtonVisuals();
        ResetNewHighScoreVisual();
        HideCountdown();

        SetDisplay(mainMenuPanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(exitButton, false, false);
        SetButtonVisibleAndEnabled(restartButton, false, false);

        // Keep the high score hidden during gameplay to reduce UI clutter.
        SetScoreUIVisible(true, false, false);

        UpdateScore(0);
        UpdateHighScore(highScore);
    }

    public void ShowCountdownText(string text)
    {
        CancelGameOverButtonSchedules();
        ResetGameOverButtonVisuals();
        ResetNewHighScoreVisual();

        SetDisplay(mainMenuPanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(exitButton, false, false);
        SetButtonVisibleAndEnabled(restartButton, false, false);

        SetScoreUIVisible(false, false, false);

        if (countdownLabel == null)
        {
            return;
        }

        countdownLabel.text = text;
        SetDisplay(countdownLabel, DisplayStyle.Flex);
        countdownLabel.BringToFront();
    }

    public void HideCountdown()
    {
        SetDisplay(countdownLabel, DisplayStyle.None);
    }

    public void PlayCountdownSfx()
    {
        PlayUISfx(countdownSfx, countdownSfxVolume);
    }

    public void ShowGameOver(int score, int highScore, bool hasNewHighScore)
    {
        CancelGameOverButtonSchedules();
        ResetGameOverButtonVisuals();
        ResetNewHighScoreVisual();
        HideCountdown();

        SetDisplay(mainMenuPanel, DisplayStyle.Flex);

        SetButtonVisibleAndEnabled(playButton, false, false);

        // Keep Game Over buttons fully hidden during the safety delay.
        // They will be displayed only after the initial animation state is prepared.
        SetGameOverButtonsVisibleAndEnabled(false, false);

        SetScoreUIVisible(true, true, false);

        UpdateScore(score);
        UpdateHighScore(highScore);

        if (hasNewHighScore)
        {
            ShowNewHighScoreFeedback();
        }

        ScheduleGameOverButtons();
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

        root = uiDocument.rootVisualElement;

        scoreLabel = root.Q<Label>(ScoreLabelName);
        highScoreLabel = root.Q<Label>(HighScoreLabelName);
        newHighScoreLabel = root.Q<Label>(NewHighScoreLabelName);
        countdownLabel = root.Q<Label>(CountdownLabelName);

        restartButton = root.Q<Button>(RestartButtonName);
        playButton = root.Q<Button>(PlayButtonName);
        exitButton = root.Q<Button>(ExitButtonName);

        mainMenuPanel = root.Q<VisualElement>(MainMenuPanelName);

        SubscribeToButtonEvents();
    }

    private void SubscribeToButtonEvents()
    {
        SubscribeButton(playButton, HandlePlayClicked);
        SubscribeButton(restartButton, HandleRestartClicked);
        SubscribeButton(exitButton, HandleExitClicked);
    }

    private void UnsubscribeFromButtonEvents()
    {
        UnsubscribeButton(playButton, HandlePlayClicked);
        UnsubscribeButton(restartButton, HandleRestartClicked);
        UnsubscribeButton(exitButton, HandleExitClicked);
    }

    private void SubscribeButton(Button button, Action clickHandler)
    {
        if (button == null)
        {
            return;
        }

        button.clicked += clickHandler;
        button.RegisterCallback<PointerEnterEvent>(HandleButtonPointerEnter);
    }

    private void UnsubscribeButton(Button button, Action clickHandler)
    {
        if (button == null)
        {
            return;
        }

        button.clicked -= clickHandler;
        button.UnregisterCallback<PointerEnterEvent>(HandleButtonPointerEnter);
    }

    private void HandleButtonPointerEnter(PointerEnterEvent evt)
    {
        Button button = evt.currentTarget as Button;

        if (button == null || !button.enabledInHierarchy || button.pickingMode == PickingMode.Ignore)
        {
            return;
        }

        PlayButtonHoverSfx();
    }

    private void HandlePlayClicked()
    {
        PlayButtonClickSfx();
        PlayClicked?.Invoke();
    }

    private void HandleRestartClicked()
    {
        PlayButtonClickSfx();
        RestartClicked?.Invoke();
    }

    private void HandleExitClicked()
    {
        PlayButtonClickSfx();
        ExitClicked?.Invoke();
    }

    private void ScheduleGameOverButtons()
    {
        if (root == null || gameOverButtonDelay <= 0f)
        {
            ShowGameOverButtonsInInitialAnimationState();
            ScheduleGameOverButtonAnimation();
            return;
        }

        int delayMilliseconds = Mathf.RoundToInt(gameOverButtonDelay * 1000f);

        showGameOverButtonsSchedule = root.schedule.Execute(ShowGameOverButtonsAfterDelay);
        showGameOverButtonsSchedule.ExecuteLater(delayMilliseconds);
    }

    private void ShowGameOverButtonsAfterDelay()
    {
        ShowGameOverButtonsInInitialAnimationState();
        ScheduleGameOverButtonAnimation();

        showGameOverButtonsSchedule = null;
    }

    private void ShowGameOverButtonsInInitialAnimationState()
    {
        PrepareGameOverButtonAnimation(restartButton);
        PrepareGameOverButtonAnimation(exitButton);

        SetGameOverButtonsVisibleAndEnabled(true, false);
        BringGameOverButtonsToFront();
    }

    private void ScheduleGameOverButtonAnimation()
    {
        if (root == null)
        {
            PlayGameOverButtonAnimation();
            return;
        }

        animateGameOverButtonsSchedule = root.schedule.Execute(PlayGameOverButtonAnimationAfterDelay);
        animateGameOverButtonsSchedule.ExecuteLater(GameOverButtonAnimationDelayMs);
    }

    private void PlayGameOverButtonAnimationAfterDelay()
    {
        PlayGameOverButtonAnimation();
        animateGameOverButtonsSchedule = null;
    }

    private void PlayGameOverButtonAnimation()
    {
        AddGameOverButtonVisibleClass(restartButton);
        AddGameOverButtonVisibleClass(exitButton);

        SetGameOverButtonsInteractive(true);
        BringGameOverButtonsToFront();
    }

    private void CancelGameOverButtonSchedules()
    {
        CancelScheduledItem(ref showGameOverButtonsSchedule);
        CancelScheduledItem(ref animateGameOverButtonsSchedule);
    }

    private void CancelScheduledItem(ref IVisualElementScheduledItem scheduledItem)
    {
        if (scheduledItem == null)
        {
            return;
        }

        scheduledItem.Pause();
        scheduledItem = null;
    }

    private void PrepareGameOverButtonAnimation(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.RemoveFromClassList(GameOverButtonEnterVisibleClass);
        button.AddToClassList(GameOverButtonEnterClass);
    }

    private void AddGameOverButtonVisibleClass(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.AddToClassList(GameOverButtonEnterVisibleClass);
    }

    private void ResetGameOverButtonVisuals()
    {
        ResetGameOverButtonVisual(restartButton);
        ResetGameOverButtonVisual(exitButton);
    }

    private void ResetGameOverButtonVisual(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.RemoveFromClassList(GameOverButtonEnterClass);
        button.RemoveFromClassList(GameOverButtonEnterVisibleClass);
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

    private void SetScoreUIVisible(bool showScore, bool showHighScore, bool showNewHighScore)
    {
        SetDisplay(scoreLabel, showScore ? DisplayStyle.Flex : DisplayStyle.None);
        SetDisplay(highScoreLabel, showHighScore ? DisplayStyle.Flex : DisplayStyle.None);
        SetDisplay(newHighScoreLabel, showNewHighScore ? DisplayStyle.Flex : DisplayStyle.None);
    }

    private void SetGameOverButtonsVisibleAndEnabled(bool visible, bool enabled)
    {
        SetButtonVisibleAndEnabled(restartButton, visible, enabled);
        SetButtonVisibleAndEnabled(exitButton, visible, enabled);
    }

    private void SetGameOverButtonsInteractive(bool interactive)
    {
        SetButtonInteractive(restartButton, interactive);
        SetButtonInteractive(exitButton, interactive);
    }

    private void BringGameOverButtonsToFront()
    {
        restartButton?.BringToFront();
        exitButton?.BringToFront();
    }

    private void SetButtonVisibleAndEnabled(Button button, bool visible, bool enabled)
    {
        if (button == null)
        {
            return;
        }

        button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        SetButtonInteractive(button, visible && enabled);
    }

    private void SetButtonInteractive(Button button, bool interactive)
    {
        if (button == null)
        {
            return;
        }

        button.SetEnabled(interactive);
        button.pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;
        button.focusable = interactive;
    }

    private void PlayButtonHoverSfx()
    {
        PlayUISfx(buttonHoverSfx, hoverSfxVolume);
    }

    private void PlayButtonClickSfx()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
    }

    private void PlayUISfx(AudioClip clip, float volume)
    {
        if (uiAudioSource == null || clip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clip, volume);
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