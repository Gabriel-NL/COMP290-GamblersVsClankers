using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverwriteSave : MonoBehaviour
{
    [SerializeField] GameObject overwritePanel;
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction submitAction;
    private InputAction cancelAction;

    void Start()
    {
        if (overwritePanel == null)
        {
            overwritePanel = GameObject.Find("OverwritePrompt"); // optional fallback
        }
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        if (inputActionAsset == null)
        {
            Debug.LogWarning("InputActionAsset not assigned to OverwriteSave script");
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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        YesOption();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        NoOption();
    }

    public void YesOption()
    {
        SavingSystem savingSystem = Object.FindFirstObjectByType<SavingSystem>();
        if (savingSystem != null)
        {
            savingSystem.SaveData();
            Debug.Log("Current save overwritten.");
        }
        else
        {
            Debug.LogError("SavingSystem not found in the scene.");
        }
    }
    
    public void NoOption()
    {
        overwritePanel.SetActive(false);
        Debug.Log("Save overwrite canceled by user.");
    }

    private void OnDestroy()
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
}
