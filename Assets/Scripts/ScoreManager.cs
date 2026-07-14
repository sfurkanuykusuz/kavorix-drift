using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    [Header("Score")]
    [SerializeField, Min(0f)] private float scoreMultiplier = 10f;

    [Header("Milestones")]
    [SerializeField, Min(1)] private int scoreMilestoneInterval = 100;

    private int currentScore;
    private int highScore;
    private int nextScoreMilestone;
    private float elapsedTime;

    public event Action<int> ScoreChanged;
    public event Action<int> HighScoreChanged;
    public event Action<int> ScoreMilestoneReached;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int ScoreMilestoneInterval => scoreMilestoneInterval;

    private void Awake()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        ResetNextScoreMilestone();
    }

    public void Tick(float deltaTime)
    {
        elapsedTime += deltaTime;

        int newScore = Mathf.FloorToInt(elapsedTime * scoreMultiplier);

        if (newScore == currentScore)
        {
            return;
        }

        currentScore = newScore;

        ScoreChanged?.Invoke(currentScore);
        CheckScoreMilestones();
    }

    public void ResetCurrentScore()
    {
        elapsedTime = 0f;
        currentScore = 0;

        ResetNextScoreMilestone();

        ScoreChanged?.Invoke(currentScore);
    }

    public bool TrySaveHighScore()
    {
        if (currentScore <= highScore)
        {
            return false;
        }

        highScore = currentScore;

        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();

        HighScoreChanged?.Invoke(highScore);

        return true;
    }

    public void ApplyScoreMultiplier(int multiplier)
    {
    if (multiplier <= 1)
    {
        return;
    }

    currentScore *= multiplier;

    if (scoreMultiplier > 0f)
    {
        elapsedTime = currentScore / scoreMultiplier;
    }

    ScoreChanged?.Invoke(currentScore);
    }

    [ContextMenu("Reset High Score")]
    public void ResetHighScore()
    {
        highScore = 0;

        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.Save();

        HighScoreChanged?.Invoke(highScore);

        Debug.Log($"{nameof(ScoreManager)}: High score has been reset.");
    }

    private void CheckScoreMilestones()
    {
        while (currentScore >= nextScoreMilestone)
        {
            ScoreMilestoneReached?.Invoke(nextScoreMilestone);
            nextScoreMilestone += scoreMilestoneInterval;
        }
    }

    private void ResetNextScoreMilestone()
    {
        nextScoreMilestone = scoreMilestoneInterval;
    }

    private void OnValidate()
    {
        scoreMultiplier = Mathf.Max(0f, scoreMultiplier);
        scoreMilestoneInterval = Mathf.Max(1, scoreMilestoneInterval);
    }
}