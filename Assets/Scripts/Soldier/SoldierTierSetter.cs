using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class SoldierTierSetter : MonoBehaviour
{
    [Header("References")]
    [MustBeAssigned] public TMP_Text tierText;
    //[MustBeAssigned] public Canvas canvas;

    public void SetTier(SoldierTierList.TierEnum tier)
    {
        if (tierText == null) return;

        int tierNumber = (int)tier + 1;

        tierText.text = $"T{tierNumber}";

        tierText.color = SoldierTierList.tierDictionary[tier].tierColor;
    }

    // void Start()
    // {
    //     Camera mainCamera = Camera.main;
        
    //     if (mainCamera == null)
    //     {
    //         Debug.LogWarning("Main camera not found in scene.");
    //         return;
    //     }
        
    //     if (canvas == null)
    //     {
    //         Debug.LogWarning("Canvas component not found in children.");
    //         return;
    //     }

    //     canvas.worldCamera = mainCamera;
    // }
}
