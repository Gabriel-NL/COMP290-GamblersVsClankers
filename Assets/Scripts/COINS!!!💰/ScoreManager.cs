using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
    
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // Add this line

    private int score;
    public TMP_Text scoreText;
    [Tooltip("Starting coins for the player")]
    public int startingScore = 500;

    private void Awake()
    {
        instance = this; // initialize the singleton
        // Initialize starting score
        score = startingScore;
        UpdateScoreText();
    }

    // Expose current score (useful as player money)
    public int CurrentScore => score;

    // Attempt to spend points. Returns true if spent, false if insufficient funds.
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (score >= amount)
        {
            score -= amount;
            UpdateScoreText();
            return true;
        }
        return false;
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
