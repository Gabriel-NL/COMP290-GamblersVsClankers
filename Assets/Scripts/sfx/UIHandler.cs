using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class UIHandler : MonoBehaviour
{
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject gameplayUI;
    [SerializeField] MixerManager mixerMngr;
    [SerializeField] Slider masterVolSLDR;
    [SerializeField] Slider musicVolSLDR;
    [SerializeField] Slider sfxVolSLDR;

    float ogMasterVol;
    float ogMusicVol;
    float ogSfxVol;

    private void Start()
    {
        //Read original values
        ogMasterVol = mixerMngr.GetMasterVolume();
        ogMusicVol = mixerMngr.GetMusicVolume();
        ogSfxVol = mixerMngr.GetSFXVolume();

        //Set ui to show these values
        masterVolSLDR.value = ogMasterVol;
        musicVolSLDR.value = ogMusicVol;
        sfxVolSLDR.value = ogSfxVol;
    }

    public void OkButtonHandler()
    {
        //update mixer manager value
        mixerMngr.SetMasterVolume(masterVolSLDR.value);
        mixerMngr.SetMusicVolume(musicVolSLDR.value);
        mixerMngr.SetSFXVolume(sfxVolSLDR.value);

        //update original value
        ogMasterVol = masterVolSLDR.value;
        ogMusicVol = musicVolSLDR.value;
        ogSfxVol = sfxVolSLDR.value;

        //set audioMixer values
        mixerMngr.SetMixer_Master();
        mixerMngr.SetMixer_Music();
        mixerMngr.SetMixer_SFX();
    }

    public void ChooseLevel(string currentScene)
    {
        SceneManager.LoadScene(currentScene);
        OkButtonHandler();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleSettings()
    {
        pauseUI.SetActive(!pauseUI.activeSelf);
        OkButtonHandler();
    }

    public void ToggleLevelSelect()
    {
        gameplayUI.SetActive(!gameplayUI.activeSelf);

    }

    public void PauseGame()
    {
        pauseUI.SetActive(true);
        gameplayUI.SetActive(false);
    }

    public void ResumeGame()
    {
        pauseUI.SetActive(false);
        gameplayUI.SetActive(true);

        OkButtonHandler();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }
}
