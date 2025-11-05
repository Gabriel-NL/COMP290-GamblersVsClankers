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

    void Start()
    {
        AudioManager.Play("MainMenuMusic");
    }
}
