using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement2D))]
[RequireComponent(typeof(PlayerBoostEffect))]
[RequireComponent(typeof(PlayerCollisionHandler2D))]
public sealed class PlayerPowerUpController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PowerUpSpawner2D powerUpSpawner;
    [SerializeField] private GameUIController uiController;
    [SerializeField] private PlayerShield2D playerShield;
    [SerializeField] private MissileTargetSelector2D missileTargetSelector;

    [Header("Choice")]
    [SerializeField] private bool pauseGameDuringChoice = true;

    private PlayerController playerController;
    private PlayerMovement2D movement;
    private PlayerBoostEffect boostEffect;
    private PlayerCollisionHandler2D collisionHandler;

    private float previousTimeScale = 1f;
    private bool isChoosingPowerUp;

    public event Action GuidedMissileSelected;
    public event Action ShieldSelected;

    public bool IsChoosingPowerUp => isChoosingPowerUp;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement2D>();
        boostEffect = GetComponent<PlayerBoostEffect>();
        collisionHandler = GetComponent<PlayerCollisionHandler2D>();

        ResolveReferences();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        ForceEndPowerUpChoice();
    }

    private void ResolveReferences()
    {
        if (powerUpSpawner == null)
        {
            powerUpSpawner = FindAnyObjectByType<PowerUpSpawner2D>();
        }

        if (uiController == null)
        {
            uiController = FindAnyObjectByType<GameUIController>(FindObjectsInactive.Include);
        }

        if (playerShield == null)
        {
            playerShield = GetComponentInChildren<PlayerShield2D>(true);
        }

        if (missileTargetSelector == null)
        {
            missileTargetSelector = GetComponent<MissileTargetSelector2D>();
        }
    }

    private void SubscribeToEvents()
    {
        if (powerUpSpawner != null)
        {
            powerUpSpawner.PowerUpPickupCollected += BeginPowerUpChoice;
        }
        else
        {
            Debug.LogWarning($"{nameof(PlayerPowerUpController)}: PowerUpSpawner2D was not found.");
        }

        if (uiController != null)
        {
            uiController.PowerUpChoiceSelected += HandlePowerUpChoiceSelected;
        }
        else
        {
            Debug.LogWarning($"{nameof(PlayerPowerUpController)}: GameUIController was not found.");
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (powerUpSpawner != null)
        {
            powerUpSpawner.PowerUpPickupCollected -= BeginPowerUpChoice;
        }

        if (uiController != null)
        {
            uiController.PowerUpChoiceSelected -= HandlePowerUpChoiceSelected;
        }
    }

    private void BeginPowerUpChoice()
    {
        if (isChoosingPowerUp || !CanUsePowerUps())
        {
            return;
        }

        isChoosingPowerUp = true;

        DisablePlayerControlForChoice();

        if (pauseGameDuringChoice)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        uiController?.ShowPowerUpChoice();
    }

    private void HandlePowerUpChoiceSelected(PowerUpChoiceType choice)
    {
        if (!isChoosingPowerUp)
        {
            return;
        }

        EndPowerUpChoice();

        if (!CanUsePowerUps())
        {
            return;
        }

        switch (choice)
        {
            case PowerUpChoiceType.GuidedMissile:
                BeginMissileTargetSelection();
                GuidedMissileSelected?.Invoke();
                Debug.Log($"{nameof(PlayerPowerUpController)}: Guided Missile selected.");
                break;

            case PowerUpChoiceType.Shield:
                ActivateShield();
                ShieldSelected?.Invoke();
                Debug.Log($"{nameof(PlayerPowerUpController)}: Shield selected.");
                break;

            default:
                Debug.LogWarning($"{nameof(PlayerPowerUpController)}: Unknown power-up choice.");
                break;
        }
    }

    private bool CanUsePowerUps()
    {
        if (playerController == null)
        {
            Debug.LogWarning($"{nameof(PlayerPowerUpController)}: PlayerController was not found.");
            return false;
        }

        return playerController.IsGamePlaying;
    }

    private void BeginMissileTargetSelection()
    {
        if (missileTargetSelector == null)
        {
            Debug.LogWarning($"{nameof(PlayerPowerUpController)}: MissileTargetSelector2D was not found.");
            return;
        }

        if (!CanUsePowerUps())
        {
            return;
        }

        missileTargetSelector.BeginTargetSelection();
    }

    private void ActivateShield()
    {
        if (playerShield == null)
        {
            Debug.LogWarning($"{nameof(PlayerPowerUpController)}: PlayerShield2D was not found.");
            return;
        }

        if (!CanUsePowerUps())
        {
            return;
        }

        playerShield.Activate();
    }

    private void EndPowerUpChoice()
    {
        if (!isChoosingPowerUp)
        {
            return;
        }

        isChoosingPowerUp = false;

        RestoreTimeScaleIfNeeded();

        uiController?.HidePowerUpChoice();

        if (CanUsePowerUps())
        {
            EnablePlayerControlAfterChoice();
        }
    }

    private void ForceEndPowerUpChoice()
    {
        if (!isChoosingPowerUp)
        {
            return;
        }

        isChoosingPowerUp = false;

        RestoreTimeScaleIfNeeded();

        uiController?.HidePowerUpChoice();
        boostEffect.Stop();
    }

    private void DisablePlayerControlForChoice()
    {
        movement.SetMovementEnabled(false);
        collisionHandler.SetCollisionDetectionEnabled(false);
        boostEffect.Stop();
    }

    private void EnablePlayerControlAfterChoice()
    {
        movement.SetMovementEnabled(true);
        collisionHandler.SetCollisionDetectionEnabled(true);
        boostEffect.Stop();
    }

    private void RestoreTimeScaleIfNeeded()
    {
        if (!pauseGameDuringChoice)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
    }
}