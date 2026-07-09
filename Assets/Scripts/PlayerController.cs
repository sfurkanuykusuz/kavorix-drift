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
        Playing,
        GameOver
    }

    [Header("UI")]
    [SerializeField] private GameUIController uiController;

    [Header("Scene References")]
    [SerializeField] private GameObject borderParent;

    private PlayerMovement2D movement;
    private PlayerBoostEffect boostEffect;
    private PlayerDeathHandler2D deathHandler;
    private PlayerCollisionHandler2D collisionHandler;
    private ScoreManager scoreManager;

    private GameState currentState = GameState.StartMenu;

    private bool IsPlaying => currentState == GameState.Playing;

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
        ShowStartMenu();
    }

    private void OnDestroy()
    {
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
        uiController.RestartClicked += ReloadScene;
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
        uiController.RestartClicked -= ReloadScene;
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
        currentState = GameState.StartMenu;

        Time.timeScale = 0f;

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        scoreManager.ResetCurrentScore();

        uiController?.ShowStartMenu(scoreManager.HighScore);
    }

    private void StartGame()
    {
        currentState = GameState.Playing;

        Time.timeScale = 1f;

        scoreManager.ResetCurrentScore();

        deathHandler.ShowPlayerForNewGame();
        movement.SetMovementEnabled(true);
        collisionHandler.SetCollisionDetectionEnabled(true);
        boostEffect.Stop();

        SetBordersActive(true);

        uiController?.ShowPlaying(scoreManager.HighScore);
    }

    private void GameOver()
    {
        currentState = GameState.GameOver;

        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();

        bool hasNewHighScore = scoreManager.TrySaveHighScore();

        deathHandler.KillPlayer();
        SetBordersActive(false);

        uiController?.ShowGameOver(
            scoreManager.CurrentScore,
            scoreManager.HighScore,
            hasNewHighScore
        );
    }

    private void SetBordersActive(bool isActive)
    {
        if (borderParent != null)
        {
            borderParent.SetActive(isActive);
        }
    }

    private void ReloadScene()
    {
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