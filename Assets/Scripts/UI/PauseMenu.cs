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
            
            // Ensure pause panel UI works during pause (uses unscaled time)
            EnsurePauseUIWorksWithTimeScale();
        }
    }
    
    /// <summary>
    /// Ensures all UI elements in the pause menu work properly when Time.timeScale = 0
    /// </summary>
    private void EnsurePauseUIWorksWithTimeScale()
    {
        if (pausePanel == null) return;
        
        // Ensure canvas group is properly configured
        CanvasGroup cg = pausePanel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = pausePanel.AddComponent<CanvasGroup>();
        }
        
        // Set all Animators in pause panel to use unscaled time
        Animator[] animators = pausePanel.GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        
        Debug.Log($"Pause menu UI configured with {animators.Length} animators set to UnscaledTime");
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
        // Ensure time is restored before loading new scene
        Time.timeScale = 1f;
        AudioManager.StopAllSounds();
        SceneManager.LoadScene("TitleScreen");
    }
    
    public void PauseGame()
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
