using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel == null)
        {
            pausePanel = GameObject.Find("PausePanel"); // optional fallback
        }
        
        // Ensure game starts unpaused
        Time.timeScale = 1f;
        isPaused = false;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePausePanel();
            }
        }

    public void TogglePausePanel()
    {
        if (pausePanel != null)
        {
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);

            // Pause or unpause the game
            if (isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        else
        {
            Debug.LogWarning("Pause Panel not assigned in the Inspector!");
        }
    }
    
    public void OpenMainMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }
    
    private void PauseGame()
    {
        Time.timeScale = 0f; // Stops all Time.deltaTime dependent operations
        isPaused = true;
        Debug.Log("Game Paused");
    }
    
    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resumes normal time
        isPaused = false;
        Debug.Log("Game Resumed");
    }
    
    /// <summary>
    /// Public method to resume game (can be called by Resume button)
    /// </summary>
    public void ResumeButton()
    {
        if (isPaused)
        {
            pausePanel.SetActive(false);
            ResumeGame();
        }
    }
    
    /// <summary>
    /// Check if game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
}
