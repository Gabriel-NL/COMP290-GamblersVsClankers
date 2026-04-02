using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class GameOver : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private GameObject gameOverPanel;
    private InputAction submitAction;
    private InputAction cancelAction;
    private bool inputActionsEnabled;

    private void Start()
    {
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }

        RefreshInputActions();
    }

    private void Update()
    {
        RefreshInputActions();
    }

    private void RefreshInputActions()
    {
        bool shouldEnable = gameOverPanel != null && gameOverPanel.activeInHierarchy;

        if (shouldEnable == inputActionsEnabled)
        {
            return;
        }

        inputActionsEnabled = shouldEnable;

        if (inputActionsEnabled)
        {
            SetupInputActions();
        }
        else
        {
            DisableInputActions();
        }
    }

    private void SetupInputActions()
    {
        if (inputActionAsset == null)
        {
            Debug.LogWarning("InputActionAsset not assigned to GameOver script");
            return;
        }

        submitAction = inputActionAsset.FindAction("UI/Submit");
        cancelAction = inputActionAsset.FindAction("UI/Cancel");

        if (submitAction == null)
        {
            Debug.LogError("Submit action not found in UI action map");
        }
        else
        {
            submitAction.performed += OnSubmit;
            submitAction.Enable();
        }

        if (cancelAction == null)
        {
            Debug.LogError("Cancel action not found in UI action map");
        }
        else
        {
            cancelAction.performed += OnCancel;
            cancelAction.Enable();
        }
    }

    private void DisableInputActions()
    {
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmit;
            submitAction.Disable();
        }

        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancel;
            cancelAction.Disable();
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        RestartGame();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        ExitGame();
    }

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

    private void OnDestroy()
    {
        DisableInputActions();
    }
}
