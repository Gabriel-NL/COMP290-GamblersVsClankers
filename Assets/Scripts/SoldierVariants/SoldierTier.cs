using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SoldierTier", menuName = "Soldiers/SoldierTier")]
public class SoldierTier : ScriptableObject
{
    [Header("Tier Details")]
    public new string name;

    public enum TierOfSoldier
    {
        common,
        uncommon,
        Rare,
        SuperRare,
        Ultra,
        Epic
    }
    [Tooltip("Select soldier tier")]
    public TierOfSoldier tier;
    //public Color bgColor;

    // switch(tier)
    // {
        
    // }
}
