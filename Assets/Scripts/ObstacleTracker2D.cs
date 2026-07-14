using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ObstacleTracker2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Completion")]
    [SerializeField, Min(1)] private int finalScoreMultiplier = 10;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;
    [SerializeField] private int debugRemainingObstacleCount;

    private readonly HashSet<Obstacle> remainingObstacles = new();

    private bool hasCompletedGame;

    public event Action<int> RemainingObstacleCountChanged;

    public int RemainingObstacleCount => remainingObstacles.Count;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    private void Start()
    {
        RefreshObstacleList();
    }

    [ContextMenu("Refresh Obstacle List")]
    public void RefreshObstacleList()
    {
        remainingObstacles.Clear();

        Obstacle[] obstacles = FindObjectsByType<Obstacle>(FindObjectsInactive.Exclude);

        foreach (Obstacle obstacle in obstacles)
        {
            if (obstacle != null)
            {
                remainingObstacles.Add(obstacle);
            }
        }

        UpdateRemainingObstacleCount();

        if (logDebugMessages)
        {
            Debug.Log($"{nameof(ObstacleTracker2D)}: Tracking {remainingObstacles.Count} obstacles.");
        }
    }

    public void NotifyObstacleDestroyed(Obstacle obstacle)
    {
        if (hasCompletedGame || obstacle == null)
        {
            return;
        }

        bool wasRemoved = remainingObstacles.Remove(obstacle);

        if (!wasRemoved)
        {
            RefreshObstacleList();
            remainingObstacles.Remove(obstacle);
        }

        UpdateRemainingObstacleCount();

        if (logDebugMessages)
        {
            Debug.Log($"{nameof(ObstacleTracker2D)}: Obstacle destroyed. Remaining: {remainingObstacles.Count}");
        }

        CheckAllObstaclesCleared();
    }

    private void CheckAllObstaclesCleared()
    {
        if (hasCompletedGame)
        {
            return;
        }

        RemoveDestroyedReferences();
        UpdateRemainingObstacleCount();

        if (remainingObstacles.Count > 0)
        {
            return;
        }

        if (playerController == null)
        {
            Debug.LogWarning($"{nameof(ObstacleTracker2D)}: PlayerController was not found.");
            return;
        }

        hasCompletedGame = true;

        if (logDebugMessages)
        {
            Debug.Log($"{nameof(ObstacleTracker2D)}: All obstacles cleared. Completing game with {finalScoreMultiplier}x score.");
        }

        playerController.CompleteGameWithScoreMultiplier(finalScoreMultiplier);
    }

    private void RemoveDestroyedReferences()
    {
        remainingObstacles.RemoveWhere(obstacle => obstacle == null);
    }

    private void UpdateRemainingObstacleCount()
    {
        debugRemainingObstacleCount = remainingObstacles.Count;
        RemainingObstacleCountChanged?.Invoke(remainingObstacles.Count);
    }

    private void OnValidate()
    {
        finalScoreMultiplier = Mathf.Max(1, finalScoreMultiplier);
    }
}