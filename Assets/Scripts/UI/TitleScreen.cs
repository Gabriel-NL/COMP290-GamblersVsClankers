using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    [Header("UI References")]
    [SerializeField] private GameObject optionsPanel;
    
    // Called when Start button is pressed
    public void StartGame()
    {
        // Clear selection before scene transition
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
        
        // Clear the load flag to start a new game
        PlayerPrefs.SetInt("ShouldLoadGame", 0);
        PlayerPrefs.Save();
        
        AudioManager.StopAllSounds();
        SceneManager.LoadScene(sceneToLoad);
    }
    
    // Called when Load button is pressed
    public void LoadGame()
    {
        // Clear selection before scene transition
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
        
        // Set a flag that tells the game scene to load saved data
        PlayerPrefs.SetInt("ShouldLoadGame", 1);
        PlayerPrefs.Save();
        
        AudioManager.StopAllSounds();
        SceneManager.LoadScene(sceneToLoad);
    }

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        AudioManager.Play("MainMenuMusic");
        
        // Ensure no objects are selected to prevent Inspector errors
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }
    
    private void OnEnable()
    {
        // Clear selection when this script enables
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif
    }
}
