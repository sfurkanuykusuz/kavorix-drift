using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMovement2D))]
[RequireComponent(typeof(PlayerBoostEffect))]
[RequireComponent(typeof(PlayerDeathHandler2D))]
[RequireComponent(typeof(PlayerCollisionHandler2D))]
[RequireComponent(typeof(ScoreManager))]
public sealed class PlayerController : MonoBehaviour
{
    private enum GameState
    {
        StartMenu,
        Countdown,
        Playing,
        GameOver
    }

    private static readonly string[] CountdownTexts = { "3", "2", "1", "GO!" };

    [Header("UI")]
    [SerializeField] private GameUIController uiController;

    [Header("Scene References")]
    [SerializeField] private GameObject borderParent;

    [Header("Systems")]
    [SerializeField] private ObstacleTracker2D obstacleTracker;

    [Header("Power Ups")]
    [SerializeField] private PlayerShield2D playerShield;
    [SerializeField] private MissileTargetSelector2D missileTargetSelector;

    [Header("Feedback")]
    [SerializeField] private CameraShake2D cameraShake;

    [Header("Countdown")]
    [SerializeField, Min(0.1f)] private float countdownStepDuration = 1f;

    private static bool startGameAfterSceneReload;

    private PlayerMovement2D movement;
    private PlayerBoostEffect boostEffect;
    private PlayerDeathHandler2D deathHandler;
    private PlayerCollisionHandler2D collisionHandler;
    private ScoreManager scoreManager;

    private Coroutine countdownCoroutine;
    private GameState currentState = GameState.StartMenu;

    public bool IsGamePlaying => currentState == GameState.Playing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        startGameAfterSceneReload = false;
    }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement2D>();
        boostEffect = GetComponent<PlayerBoostEffect>();
        deathHandler = GetComponent<PlayerDeathHandler2D>();
        collisionHandler = GetComponent<PlayerCollisionHandler2D>();
        scoreManager = GetComponent<ScoreManager>();

        ResolveReferences();
        SubscribeToEvents();
    }

    private void Start()
    {
        bool shouldStartImmediately = startGameAfterSceneReload;
        startGameAfterSceneReload = false;

        if (shouldStartImmediately)
        {
            StartGame();
            return;
        }

        ShowStartMenu();
    }

    private void OnDestroy()
    {
        StopCountdown();
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (!IsGamePlaying)
        {
            return;
        }

        scoreManager.Tick(Time.deltaTime);
        movement.TickInput();
        boostEffect.SetBoosting(movement.IsThrusting);
    }

    private void FixedUpdate()
    {
        if (!IsGamePlaying)
        {
            return;
        }

        movement.FixedTickMovement();
    }

    public void CompleteGameWithScoreMultiplier(int finalScoreMultiplier)
    {
        if (!IsGamePlaying)
        {
            return;
        }

        scoreManager.ApplyScoreMultiplier(finalScoreMultiplier);
        GameOver(finalScoreMultiplier);
    }

    private void ResolveReferences()
    {
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<GameUIController>(FindObjectsInactive.Include);
        }

        if (obstacleTracker == null)
        {
            obstacleTracker = FindAnyObjectByType<ObstacleTracker2D>();
        }

        if (playerShield == null)
        {
            playerShield = GetComponentInChildren<PlayerShield2D>(true);
        }

        if (missileTargetSelector == null)
        {
            missileTargetSelector = GetComponent<MissileTargetSelector2D>();
        }

        if (cameraShake == null)
        {
            cameraShake = FindAnyObjectByType<CameraShake2D>();
        }
    }

    private void SubscribeToEvents()
    {
        collisionHandler.CollisionDetected += HandlePlayerCollision;

        scoreManager.ScoreChanged += HandleScoreChanged;
        scoreManager.HighScoreChanged += HandleHighScoreChanged;

        if (obstacleTracker != null)
        {
            obstacleTracker.RemainingObstacleCountChanged += HandleRemainingObstacleCountChanged;
        }
        else
        {
            Debug.LogWarning($"{nameof(PlayerController)}: ObstacleTracker2D was not found.");
        }

        if (uiController == null)
        {
            Debug.LogWarning($"{nameof(PlayerController)}: GameUIController was not found.");
            return;
        }

        uiController.PlayClicked += StartGame;
        uiController.RestartClicked += RestartGame;
        uiController.ExitClicked += ExitGame;
    }

    private void UnsubscribeFromEvents()
    {
        if (collisionHandler != null)
        {
            collisionHandler.CollisionDetected -= HandlePlayerCollision;
        }

        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= HandleScoreChanged;
            scoreManager.HighScoreChanged -= HandleHighScoreChanged;
        }

        if (obstacleTracker != null)
        {
            obstacleTracker.RemainingObstacleCountChanged -= HandleRemainingObstacleCountChanged;
        }

        if (uiController == null)
        {
            return;
        }

        uiController.PlayClicked -= StartGame;
        uiController.RestartClicked -= RestartGame;
        uiController.ExitClicked -= ExitGame;
    }

    private void HandleScoreChanged(int newScore)
    {
        uiController?.UpdateScore(newScore);
    }

    private void HandleHighScoreChanged(int newHighScore)
    {
        uiController?.UpdateHighScore(newHighScore);
    }

    private void HandleRemainingObstacleCountChanged(int remainingObstacleCount)
    {
        uiController?.UpdateObstacleCount(remainingObstacleCount);
    }

    private void HandlePlayerCollision(Collision2D collision)
    {
        if (!IsGamePlaying)
        {
            return;
        }

        bool isObstacleCollision = IsObstacleCollision(collision);
        bool isBorderCollision = IsBorderCollision(collision);

        if (ShouldIgnoreCollisionDueToShield(isObstacleCollision, isBorderCollision))
        {
            cameraShake?.ShakeShieldImpact();
            return;
        }

        if (isObstacleCollision || isBorderCollision)
        {
            cameraShake?.ShakePlayerImpact();
        }

        GameOver();
    }

    private bool ShouldIgnoreCollisionDueToShield(bool isObstacleCollision, bool isBorderCollision)
    {
        if (playerShield == null || !playerShield.IsActive)
        {
            return false;
        }

        return isObstacleCollision || isBorderCollision;
    }

    private bool IsObstacleCollision(Collision2D collision)
    {
        return collision.gameObject.GetComponentInParent<Obstacle>() != null;
    }

    private bool IsBorderCollision(Collision2D collision)
    {
        if (borderParent == null)
        {
            return false;
        }

        Transform collisionTransform = collision.gameObject.transform;
        Transform borderTransform = borderParent.transform;

        return collisionTransform == borderTransform ||
               collisionTransform.IsChildOf(borderTransform);
    }

    private void ShowStartMenu()
    {
        StopCountdown();

        currentState = GameState.StartMenu;
        Time.timeScale = 0f;

        playerShield?.Deactivate();
        missileTargetSelector?.CancelTargetSelection();

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        scoreManager.ResetCurrentScore();

        SetBordersActive(true);

        uiController?.HideCountdown();
        uiController?.HidePowerUpChoice();
        uiController?.UpdateObstacleCount(0);
        uiController?.ShowStartMenu(scoreManager.HighScore);
    }

    private void StartGame()
    {
        StopCountdown();

        currentState = GameState.Countdown;
        Time.timeScale = 0f;

        playerShield?.Deactivate();
        missileTargetSelector?.CancelTargetSelection();

        scoreManager.ResetCurrentScore();

        deathHandler.ShowPlayerForNewGame();
        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        SetBordersActive(true);

        countdownCoroutine = StartCoroutine(StartCountdownRoutine());
    }

    private IEnumerator StartCountdownRoutine()
    {
        uiController?.PlayCountdownSfx();

        for (int i = 0; i < CountdownTexts.Length; i++)
        {
            uiController?.ShowCountdownText(CountdownTexts[i]);
            yield return new WaitForSecondsRealtime(countdownStepDuration);
        }

        countdownCoroutine = null;
        BeginPlaying();
    }

    private void BeginPlaying()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        uiController?.HideCountdown();
        uiController?.HidePowerUpChoice();
        uiController?.ShowPlaying(scoreManager.HighScore);

        obstacleTracker?.RefreshObstacleList();

        if (obstacleTracker != null)
        {
            uiController?.UpdateObstacleCount(obstacleTracker.RemainingObstacleCount);
        }

        movement.SetMovementEnabled(true);
        collisionHandler.SetCollisionDetectionEnabled(true);
        boostEffect.Stop();

        SetBordersActive(true);
    }

    private void GameOver(int completionScoreMultiplier = 1)
    {
        StopCountdown();

        currentState = GameState.GameOver;

        playerShield?.Deactivate();
        missileTargetSelector?.CancelTargetSelection();

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        bool hasNewHighScore = scoreManager.TrySaveHighScore();

        deathHandler.KillPlayer();
        SetBordersActive(false);

        uiController?.HideCountdown();
        uiController?.HidePowerUpChoice();

        uiController?.ShowGameOver(
            scoreManager.CurrentScore,
            scoreManager.HighScore,
            hasNewHighScore,
            completionScoreMultiplier
        );
    }

    private void StopCountdown()
    {
        if (countdownCoroutine == null)
        {
            return;
        }

        StopCoroutine(countdownCoroutine);
        countdownCoroutine = null;

        uiController?.HideCountdown();
    }

    private void SetBordersActive(bool isActive)
    {
        if (borderParent != null)
        {
            borderParent.SetActive(isActive);
        }
    }

    private void RestartGame()
    {
        startGameAfterSceneReload = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Application.OpenURL("https://play.unity.com/");
#else
        Application.Quit();
#endif
    }
}