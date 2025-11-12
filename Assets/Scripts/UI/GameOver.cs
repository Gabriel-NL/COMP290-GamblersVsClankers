using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOver : MonoBehaviour
{
    public void RestartGame()
    {
        AudioManager.StopAllSounds();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ExitGame()
    {
        AudioManager.StopAllSounds();
        SceneManager.LoadScene("TitleScreen");
    }
}
