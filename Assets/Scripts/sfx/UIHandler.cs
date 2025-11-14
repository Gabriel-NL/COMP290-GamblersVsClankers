using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class UIHandler : MonoBehaviour
{
    [SerializeField] GameObject optionsUI;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] MixerManager mixerMngr;
    [SerializeField] Slider masterVolSLDR;
    [SerializeField] Slider musicVolSLDR;
    [SerializeField] Slider sfxVolSLDR;

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
        
        // Ensure sliders are configured to work during pause
        ConfigureSliderForPause();
    }
    
    private void ConfigureSliderForPause()
    {
        // Enable all slider raycasts and canvasgroupos
        Slider[] sliders = new Slider[] { masterVolSLDR, musicVolSLDR, sfxVolSLDR };
        foreach (Slider slider in sliders)
        {
            if (slider == null) continue;
            
            // Ensure CanvasGroup is configured
            CanvasGroup cg = slider.GetComponent<CanvasGroup>();
            if (cg == null) cg = slider.gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            
            // Ensure Image component is set to raycast target
            UnityEngine.UI.Image img = slider.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.raycastTarget = true;
            
            // Configure any animators to use unscaled time
            Animator animator = slider.GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }
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
        
        // When opening options during pause, ensure time scale allows UI interaction
        if (!isActive) // Opening options
        {
            // Temporarily restore time scale to allow smooth slider interaction
            // The pause will be re-enabled when closing options
            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null && pauseMenu.IsPaused())
            {
                Debug.Log("Options menu opened during pause - restoring time for UI responsiveness");
                Time.timeScale = 1f; // Allow UI to be responsive
            }
        }
        else // Closing options
        {
            // Re-enable pause if it was paused
            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null && pauseMenu.IsPaused())
            {
                Time.timeScale = 0f; // Resume pause
                Debug.Log("Options menu closed - resuming pause");
            }
        }
    }
}
