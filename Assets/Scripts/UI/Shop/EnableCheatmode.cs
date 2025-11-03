using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCheatmode : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Debug.Log("Cheat mode enabled: INFINITE MONEY");
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddPoints(999999);
            }
            else
            {
                Debug.LogWarning("ScoreManager instance not found.");
            }

        }
    }
}
