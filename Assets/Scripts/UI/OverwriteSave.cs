using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwriteSave : MonoBehaviour
{
    [SerializeField] GameObject overwritePanel;
    // Start is called before the first frame update
    void Start()
    {
        if (overwritePanel == null)
        {
            overwritePanel = GameObject.Find("OverwritePrompt"); // optional fallback
        }
    }

    // Update is called once per frame
    void Update()
    {

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
}
