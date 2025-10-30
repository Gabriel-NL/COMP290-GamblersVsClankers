using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    void Start()
    {
        if (pausePanel == null)
        {
            pausePanel = GameObject.Find("PausePanel"); // optional fallback
        }
    }
    void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePausePanel();
            }
        }

    public void TogglePausePanel()
    {
        if (pausePanel != null)
        {
            bool isActive = pausePanel.activeSelf;
            pausePanel.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("Pause Panel not assigned in the Inspector!");
        }
    }
}
