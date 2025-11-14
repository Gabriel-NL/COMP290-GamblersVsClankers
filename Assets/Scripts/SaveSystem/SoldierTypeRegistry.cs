using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registry that holds references to all SoldierType assets
/// This allows us to find SoldierTypes without using Resources folders
/// </summary>
[CreateAssetMenu(fileName = "SoldierTypeRegistry", menuName = "ScriptableObjects/SoldierTypeRegistry")]
public class SoldierTypeRegistry : ScriptableObject
{
    [Header("All Available Soldier Types")]
    [Tooltip("Drag all your SoldierType ScriptableObjects here")]
    public List<SoldierType> allSoldierTypes = new List<SoldierType>();

    /// <summary>
    /// Find a SoldierType by its ScriptableObject name
    /// </summary>
    public SoldierType GetSoldierTypeByName(string soldierTypeName)
    {
        foreach (SoldierType soldierType in allSoldierTypes)
        {
            // Use the ScriptableObject's asset name (not the display name field)
            if ((soldierType as Object).name == soldierTypeName)
            {
                return soldierType;
            }
        }
        
        Debug.LogWarning($"SoldierType '{soldierTypeName}' not found in registry. Make sure it's added to the SoldierTypeRegistry asset.");
        return null;
    }
}
