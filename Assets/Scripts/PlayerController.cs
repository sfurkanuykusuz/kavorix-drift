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

    private bool IsPlaying => currentState == GameState.Playing;

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

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<GameUIController>(FindObjectsInactive.Include);
        }

        SubscribeToEvents();
    }

    private void Start()
    {
        bool shouldStartImmediately = startGameAfterSceneReload;
        startGameAfterSceneReload = false;

        if (shouldStartImmediately)
        {
            StartGame();
        }
        else
        {
            ShowStartMenu();
        }
    }

    private void OnDestroy()
    {
        StopCountdown();
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        scoreManager.Tick(Time.deltaTime);
        movement.TickInput();
        boostEffect.SetBoosting(movement.IsThrusting);
    }

    private void FixedUpdate()
    {
        if (!IsPlaying)
        {
            return;
        }

        movement.FixedTickMovement();
    }

    private void SubscribeToEvents()
    {
        collisionHandler.CollisionDetected += HandlePlayerCollision;

        scoreManager.ScoreChanged += HandleScoreChanged;
        scoreManager.HighScoreChanged += HandleHighScoreChanged;

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

    private void HandlePlayerCollision(Collision2D collision)
    {
        if (!IsPlaying)
        {
            return;
        }

        GameOver();
    }

    private void ShowStartMenu()
    {
        StopCountdown();

        currentState = GameState.StartMenu;

        Time.timeScale = 0f;

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        scoreManager.ResetCurrentScore();

        SetBordersActive(true);

        uiController?.HideCountdown();
        uiController?.ShowStartMenu(scoreManager.HighScore);
    }

    private void StartGame()
    {
        StopCountdown();

        currentState = GameState.Countdown;

        // Keep the game paused during countdown.
        // WaitForSecondsRealtime is used so the countdown still runs while Time.timeScale is 0.
        Time.timeScale = 0f;

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
        uiController?.ShowPlaying(scoreManager.HighScore);

        movement.SetMovementEnabled(true);
        collisionHandler.SetCollisionDetectionEnabled(true);
        boostEffect.Stop();

        SetBordersActive(true);
    }

    private void GameOver()
    {
        StopCountdown();

        currentState = GameState.GameOver;

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        bool hasNewHighScore = scoreManager.TrySaveHighScore();

        deathHandler.KillPlayer();
        SetBordersActive(false);

        uiController?.HideCountdown();

        uiController?.ShowGameOver(
            scoreManager.CurrentScore,
            scoreManager.HighScore,
            hasNewHighScore
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