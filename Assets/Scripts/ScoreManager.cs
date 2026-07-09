using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScoreManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";

    [Header("Score")]
    [SerializeField, Min(0f)] private float scoreMultiplier = 10f;

    private int currentScore;
    private int highScore;
    private float elapsedTime;

    public event Action<int> ScoreChanged;
    public event Action<int> HighScoreChanged;

    public int CurrentScore => currentScore;
    public int HighScore => highScore;

    private void Awake()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
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
    }

    public void ResetCurrentScore()
    {
        elapsedTime = 0f;
        currentScore = 0;

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

    [ContextMenu("Reset High Score")]
    public void ResetHighScore()
    {
        highScore = 0;

        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.Save();

        HighScoreChanged?.Invoke(highScore);

        Debug.Log($"{nameof(ScoreManager)}: High score has been reset.");
    }

    private void OnValidate()
    {
        scoreMultiplier = Mathf.Max(0f, scoreMultiplier);
    }
}