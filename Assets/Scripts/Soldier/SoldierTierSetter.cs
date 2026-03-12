using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class SoldierTierSetter : MonoBehaviour
{
    [Header("References")]
    [MustBeAssigned] public TMP_Text tierText;

    public void SetTier(SoldierTierList.TierEnum tier)
    {
        if (tierText == null) return;

        int tierNumber = (int)tier + 1;

        tierText.text = $"T{tierNumber}";

        tierText.color = SoldierTierList.tierDictionary[tier].tierColor;
    }
}
