using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class UIHandler : MonoBehaviour
{
    [SerializeField] GameObject optionsUI;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] MixerManager mixerMngr;
    [SerializeField] Slider masterVolSLDR;
    [SerializeField] Slider musicVolSLDR;
    [SerializeField] Slider sfxVolSLDR;
    [SerializeField] Toggle fullscreenToggle;

    private float ogMasterVol;
    private float ogMusicVol;
    private float ogSfxVol;

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

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            Debug.Log("Fullscreen toggle set to: " + fullscreenToggle.isOn);
        }
        else
        {
            Debug.LogWarning("Fullscreen toggle reference is missing in UIHandler.");
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        
    }

    public void UpdateAudioSettings()
    {
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    private void SetMasterVolume()
    {
        mixerMngr.SetMasterVolume(masterVolSLDR.value);
        ogMasterVol = masterVolSLDR.value;
        mixerMngr.SetMixer_Master();
    }

    private void SetSFXVolume()
    {
        mixerMngr.SetSFXVolume(sfxVolSLDR.value);
        ogSfxVol = sfxVolSLDR.value;
        mixerMngr.SetMixer_SFX();
    }
    private void SetMusicVolume()
    {
        mixerMngr.SetMusicVolume(musicVolSLDR.value);
        ogMusicVol = musicVolSLDR.value;
        mixerMngr.SetMixer_Music();
    }

    public void ToggleSettings()
    {
        bool isActive = optionsUI.activeSelf;
        optionsUI.SetActive(!isActive);
        // Re-enable pause if it was paused
        PauseMenu pauseMenu = Object.FindFirstObjectByType<PauseMenu>();
        if (pauseMenu != null && pauseMenu.IsPaused())
        {
            Time.timeScale = 0f; // Resume pause
            Debug.Log("Options menu closed - resuming pause");
        }
    }
}
