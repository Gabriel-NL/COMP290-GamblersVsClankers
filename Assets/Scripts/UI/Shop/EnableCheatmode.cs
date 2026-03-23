using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnableCheatmode : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction cheatAction;

    private void OnEnable()
    {
        if (inputActionAsset == null)
        {
            Debug.LogWarning("InputActionAsset not assigned to EnableCheatmode script");
            return;
        }

        cheatAction = inputActionAsset.FindAction("UI/Cheat");
        if (cheatAction == null)
        {
            Debug.LogError("Cheat action not found in UI action map");
            return;
        }

        cheatAction.performed += OnCheatInput;
        cheatAction.Enable();
    }

    private void OnCheatInput(InputAction.CallbackContext context)
    {
        ActivateCheatmode();
    }

    public void ActivateCheatmode()
    {
        ScoreManager scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddPoints(999999);
            GameObject.Find("DeathZone").GetComponent<DeathZone>().lives = 999999;
        }
        else
        {
            Debug.LogWarning("ScoreManager instance not found.");
        }
    }
}
