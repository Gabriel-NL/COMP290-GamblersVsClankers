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
        SceneManager.LoadScene(sceneToLoad);
    }

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Application.Quit();
    }

    // Called when Options button is pressed
    public void ToggleOptions()
    {
        if (optionsPanel != null)
        {
            bool isActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Options Panel not assigned in the Inspector!");
        }
    }

    public void ToggleOptionsOff()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Options Panel not assigned in the Inspector!");
        }
    }
}
