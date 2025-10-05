using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
    
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance; // Add this line

    private int score;
    public TMP_Text scoreText;

    private void Awake()
    {
        instance = this; // Add this line to initialize the singleton
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
