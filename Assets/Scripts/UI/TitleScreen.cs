using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private String sceneToLoad;
    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
