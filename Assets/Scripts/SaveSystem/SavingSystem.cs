
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles saving game data to JSON files
/// For loading, see LoadingSystem.cs
/// </summary>
public class SavingSystem : MonoBehaviour
{
    [Header("Collection root")]
    public Transform slotsParent; // <-- target parent exposed in Inspector
    
    /// <summary>
    /// Save the current game state (soldiers, tier, score) to a JSON file
    /// </summary>
    public void SaveData()
    {
        GridBuilder<ItemSlot> gridBuilder = new GridBuilder<ItemSlot>(slotsParent);
        GridSerializer serializer = new GridSerializer();
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();

        // Create metadata (score)
        var meta = new Dictionary<string, string>
        {
            { "points", scoreManager.CurrentScore.ToString() }
        };

        // Create grid of soldier data
        var grid = new Dictionary<(int, int), SoldierDataForSerialization>();

        foreach (KeyValuePair<(int, int), ItemSlot> keyValuePair in gridBuilder.customGrid)
        {
            if (keyValuePair.Value == null || keyValuePair.Value.occupyingObject == null)
            {
                continue;
            }
            
            if (keyValuePair.Value.occupyingObject.TryGetComponent<SoldierBehaviour>(out SoldierBehaviour soldierBehaviour))
            {
                // Get the ScriptableObject asset name (not the display name) and tier enum
                // Use "as Object" to access the base ScriptableObject.name property
                string soldierTypeName = soldierBehaviour.SoldierType != null 
                    ? (soldierBehaviour.SoldierType as Object).name 
                    : "Unknown";
                string tierName = soldierBehaviour.tier.ToString();

                SoldierDataForSerialization entry = new SoldierDataForSerialization(soldierTypeName, tierName);
                grid.Add(keyValuePair.Key, entry);

                Debug.Log($"Saved soldier: {soldierTypeName} with tier {tierName} at grid position {keyValuePair.Key}");
            }
        }

        // Save to file inside the project
        string projectSavesFolder = Path.Combine(Application.dataPath, "SavesJson");
        Directory.CreateDirectory(projectSavesFolder);

        string fullPath = Path.Combine(projectSavesFolder, "MyFirstSave.json");

        serializer.SaveToFile(grid, meta, fullPath);

        Debug.Log($"Saved grid to: {fullPath}");

#if UNITY_EDITOR
        // So the file appears in the Project window immediately
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
