using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum PowerUpChoiceType
{
    GuidedMissile,
    Shield
}

[DisallowMultipleComponent]
public sealed class GameUIController : MonoBehaviour
{
    private const string ScoreLabelName = "ScoreLabel";
    private const string ObstacleCountLabelName = "ObstacleCountLabel";
    private const string HighScoreLabelName = "HighScoreLabel";
    private const string NewHighScoreLabelName = "NewHighScoreLabel";
    private const string CompletionBonusLabelName = "CompletionBonusLabel";
    private const string CountdownLabelName = "CountdownLabel";

    private const string PlayButtonName = "PlayButton";
    private const string MenuExitButtonName = "MenuExitButton";
    private const string RestartButtonName = "RestartButton";
    private const string GameOverExitButtonName = "GameOverExitButton";
    private const string MissileButtonName = "MissileButton";
    private const string ShieldButtonName = "ShieldButton";

    private const string MainMenuPanelName = "MainMenuPanel";
    private const string PowerUpChoicePanelName = "PowerUpChoicePanel";

    private const string NewHighScoreVisibleClass = "new-high-score-text-show";
    private const string HighScoreFlashClass = "high-score-flash";
    private const string CompletionBonusVisibleClass = "completion-bonus-label-show";

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
    private VisualElement mainMenuPanel;
    private VisualElement powerUpChoicePanel;

    private Label scoreLabel;
    private Label obstacleCountLabel;
    private Label highScoreLabel;
    private Label newHighScoreLabel;
    private Label completionBonusLabel;
    private Label countdownLabel;

    private Button playButton;
    private Button menuExitButton;
    private Button restartButton;
    private Button gameOverExitButton;
    private Button missileButton;
    private Button shieldButton;

    private IVisualElementScheduledItem showGameOverButtonsSchedule;
    private IVisualElementScheduledItem animateGameOverButtonsSchedule;

    public event Action PlayClicked;
    public event Action RestartClicked;
    public event Action ExitClicked;
    public event Action<PowerUpChoiceType> PowerUpChoiceSelected;

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
        ResetTemporaryUI();

        SetDisplay(mainMenuPanel, DisplayStyle.Flex);
        SetDisplay(powerUpChoicePanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, true, true);
        SetButtonVisibleAndEnabled(menuExitButton, true, true);
        SetButtonVisibleAndEnabled(restartButton, false, false);
        SetButtonVisibleAndEnabled(gameOverExitButton, false, false);

        SetScoreUIVisible(false, false, false);
        SetObstacleCountVisible(false);

        UpdateScore(0);
        UpdateHighScore(highScore);
        UpdateObstacleCount(0);
    }

    public void ShowPlaying(int highScore)
    {
        ResetTemporaryUI();

        SetDisplay(mainMenuPanel, DisplayStyle.None);
        SetDisplay(powerUpChoicePanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(menuExitButton, false, false);
        SetButtonVisibleAndEnabled(restartButton, false, false);
        SetButtonVisibleAndEnabled(gameOverExitButton, false, false);

        SetScoreUIVisible(true, false, false);
        SetObstacleCountVisible(true);

        UpdateScore(0);
        UpdateHighScore(highScore);
    }

    public void ShowCountdownText(string text)
    {
        ResetTemporaryUI();

        SetDisplay(mainMenuPanel, DisplayStyle.None);
        SetDisplay(powerUpChoicePanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(menuExitButton, false, false);
        SetButtonVisibleAndEnabled(restartButton, false, false);
        SetButtonVisibleAndEnabled(gameOverExitButton, false, false);

        SetScoreUIVisible(false, false, false);
        SetObstacleCountVisible(false);

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

    public void ShowPowerUpChoice()
    {
        ResetTemporaryUI();

        SetDisplay(mainMenuPanel, DisplayStyle.None);
        SetDisplay(powerUpChoicePanel, DisplayStyle.Flex);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(menuExitButton, false, false);
        SetButtonVisibleAndEnabled(restartButton, false, false);
        SetButtonVisibleAndEnabled(gameOverExitButton, false, false);
        SetButtonVisibleAndEnabled(missileButton, true, true);
        SetButtonVisibleAndEnabled(shieldButton, true, true);

        SetScoreUIVisible(true, false, false);
        SetObstacleCountVisible(true);

        powerUpChoicePanel?.BringToFront();
    }

    public void HidePowerUpChoice()
    {
        SetDisplay(powerUpChoicePanel, DisplayStyle.None);
        SetButtonVisibleAndEnabled(missileButton, false, false);
        SetButtonVisibleAndEnabled(shieldButton, false, false);
    }

    public void ShowGameOver(int score, int highScore, bool hasNewHighScore, int completionScoreMultiplier = 1)
    {
        ResetTemporaryUI();

        SetDisplay(mainMenuPanel, DisplayStyle.None);
        SetDisplay(powerUpChoicePanel, DisplayStyle.None);

        SetButtonVisibleAndEnabled(playButton, false, false);
        SetButtonVisibleAndEnabled(menuExitButton, false, false);
        SetGameOverButtonsVisibleAndEnabled(false, false);

        SetScoreUIVisible(true, true, false);
        SetObstacleCountVisible(false);

        UpdateScore(score);
        UpdateHighScore(highScore);

        if (hasNewHighScore)
        {
            ShowNewHighScoreFeedback();
        }

        if (completionScoreMultiplier > 1)
        {
            ShowCompletionBonus(completionScoreMultiplier);
        }

        ScheduleGameOverButtons();
    }

    public void UpdateScore(int score)
    {
        SetLabelText(scoreLabel, $"Score: {score}");
    }

    public void UpdateObstacleCount(int obstacleCount)
    {
        SetLabelText(obstacleCountLabel, $"Obstacles: {obstacleCount}");
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
        obstacleCountLabel = root.Q<Label>(ObstacleCountLabelName);
        highScoreLabel = root.Q<Label>(HighScoreLabelName);
        newHighScoreLabel = root.Q<Label>(NewHighScoreLabelName);
        completionBonusLabel = root.Q<Label>(CompletionBonusLabelName);
        countdownLabel = root.Q<Label>(CountdownLabelName);

        playButton = root.Q<Button>(PlayButtonName);
        menuExitButton = root.Q<Button>(MenuExitButtonName);
        restartButton = root.Q<Button>(RestartButtonName);
        gameOverExitButton = root.Q<Button>(GameOverExitButtonName);
        missileButton = root.Q<Button>(MissileButtonName);
        shieldButton = root.Q<Button>(ShieldButtonName);

        mainMenuPanel = root.Q<VisualElement>(MainMenuPanelName);
        powerUpChoicePanel = root.Q<VisualElement>(PowerUpChoicePanelName);

        SubscribeToButtonEvents();
    }

    private void SubscribeToButtonEvents()
    {
        SubscribeButton(playButton, HandlePlayClicked);
        SubscribeButton(menuExitButton, HandleExitClicked);
        SubscribeButton(restartButton, HandleRestartClicked);
        SubscribeButton(gameOverExitButton, HandleExitClicked);
        SubscribeButton(missileButton, HandleMissileButtonClicked);
        SubscribeButton(shieldButton, HandleShieldButtonClicked);
    }

    private void UnsubscribeFromButtonEvents()
    {
        UnsubscribeButton(playButton, HandlePlayClicked);
        UnsubscribeButton(menuExitButton, HandleExitClicked);
        UnsubscribeButton(restartButton, HandleRestartClicked);
        UnsubscribeButton(gameOverExitButton, HandleExitClicked);
        UnsubscribeButton(missileButton, HandleMissileButtonClicked);
        UnsubscribeButton(shieldButton, HandleShieldButtonClicked);
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

        PlayUISfx(buttonHoverSfx, hoverSfxVolume);
    }

    private void HandlePlayClicked()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
        PlayClicked?.Invoke();
    }

    private void HandleRestartClicked()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
        RestartClicked?.Invoke();
    }

    private void HandleExitClicked()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
        ExitClicked?.Invoke();
    }

    private void HandleMissileButtonClicked()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
        HidePowerUpChoice();
        PowerUpChoiceSelected?.Invoke(PowerUpChoiceType.GuidedMissile);
    }

    private void HandleShieldButtonClicked()
    {
        PlayUISfx(buttonClickSfx, clickSfxVolume);
        HidePowerUpChoice();
        PowerUpChoiceSelected?.Invoke(PowerUpChoiceType.Shield);
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
        PrepareGameOverButtonAnimation(gameOverExitButton);

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
        AddGameOverButtonVisibleClass(gameOverExitButton);

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

    private void ResetTemporaryUI()
    {
        CancelGameOverButtonSchedules();
        ResetGameOverButtonVisuals();
        ResetNewHighScoreVisual();
        ResetCompletionBonusVisual();
        HideCountdown();
        HidePowerUpChoice();
        SetObstacleCountVisible(false);
    }

    private void ResetGameOverButtonVisuals()
    {
        ResetGameOverButtonVisual(restartButton);
        ResetGameOverButtonVisual(gameOverExitButton);
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
            SetDisplay(newHighScoreLabel, DisplayStyle.None);
        }

        if (highScoreLabel != null)
        {
            highScoreLabel.RemoveFromClassList(HighScoreFlashClass);
        }
    }

    private void ShowCompletionBonus(int multiplier)
    {
        if (completionBonusLabel == null)
        {
            return;
        }

        completionBonusLabel.text = $"ALL CLEAR! SCORE x{multiplier}";
        SetDisplay(completionBonusLabel, DisplayStyle.Flex);
        completionBonusLabel.AddToClassList(CompletionBonusVisibleClass);
        completionBonusLabel.BringToFront();
    }

    private void ResetCompletionBonusVisual()
    {
        if (completionBonusLabel == null)
        {
            return;
        }

        completionBonusLabel.RemoveFromClassList(CompletionBonusVisibleClass);
        SetDisplay(completionBonusLabel, DisplayStyle.None);
    }

    private void SetScoreUIVisible(bool showScore, bool showHighScore, bool showNewHighScore)
    {
        SetDisplay(scoreLabel, showScore ? DisplayStyle.Flex : DisplayStyle.None);
        SetDisplay(highScoreLabel, showHighScore ? DisplayStyle.Flex : DisplayStyle.None);
        SetDisplay(newHighScoreLabel, showNewHighScore ? DisplayStyle.Flex : DisplayStyle.None);
    }

    private void SetObstacleCountVisible(bool isVisible)
    {
        SetDisplay(obstacleCountLabel, isVisible ? DisplayStyle.Flex : DisplayStyle.None);
    }

    private void SetGameOverButtonsVisibleAndEnabled(bool visible, bool enabled)
    {
        SetButtonVisibleAndEnabled(restartButton, visible, enabled);
        SetButtonVisibleAndEnabled(gameOverExitButton, visible, enabled);
    }

    private void SetGameOverButtonsInteractive(bool interactive)
    {
        SetButtonInteractive(restartButton, interactive);
        SetButtonInteractive(gameOverExitButton, interactive);
    }

    private void BringGameOverButtonsToFront()
    {
        restartButton?.BringToFront();
        gameOverExitButton?.BringToFront();
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