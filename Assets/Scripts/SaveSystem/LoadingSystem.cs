using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles loading saved game data from JSON files
/// Separate from SavingSystem for better organization
/// Place this component in your game scene (not title screen)
/// </summary>
public class LoadingSystem : MonoBehaviour
{
    [Header("Collection root")]
    public Transform slotsParent; // Reference to the grid of item slots
    
    [Header("Soldier Type Registry")]
    [Tooltip("Reference to the SoldierTypeRegistry ScriptableObject that contains all soldier types")]
    public SoldierTypeRegistry soldierTypeRegistry;

    private void Start()
    {
        // Check if we should load a saved game
        int shouldLoad = PlayerPrefs.GetInt("ShouldLoadGame", 0);
        
        if (shouldLoad == 1)
        {
            // Clear the flag
            PlayerPrefs.SetInt("ShouldLoadGame", 0);
            PlayerPrefs.Save();
            
            // Check if save file exists before attempting to load
            if (SaveFileExists())
            {
                Debug.Log("Save file found. Loading game...");
                LoadData();
            }
            else
            {
                Debug.Log("No save file found. Starting new game...");
                // Don't load anything - let the game start fresh like "Start Game" was pressed
            }
        }
    }

    /// <summary>
    /// Load game data from the most recent save file
    /// </summary>
    public void LoadData()
    {
        string projectSavesFolder = Path.Combine(Application.dataPath, "SavesJson");
        string fullPath = Path.Combine(projectSavesFolder, "MyFirstSave.json");

        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Save file not found at: {fullPath}");
            return;
        }

        GridSerializer serializer = new GridSerializer();
        
        try
        {
            // Properly call LoadFromFile with out parameter
            var grid = serializer.LoadFromFile<SoldierDataForSerialization>(fullPath, out Dictionary<string, string> meta);

            Debug.Log($"Loaded grid from: {fullPath}");

            // Start the loading coroutine to handle cleanup and instantiation safely
            StartCoroutine(LoadDataCoroutine(grid, meta));
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"Error loading save file: {ex.Message}");
        }
    }

    private IEnumerator LoadDataCoroutine(Dictionary<(int, int), SoldierDataForSerialization> grid, Dictionary<string, string> meta)
    {
        GridBuilder<ItemSlot> gridBuilder = new GridBuilder<ItemSlot>(slotsParent);

        // Step 1: Destroy all existing soldiers in slots
        List<GameObject> toDestroy = new List<GameObject>();
        
        foreach (ItemSlot slot in slotsParent.GetComponentsInChildren<ItemSlot>())
        {
            if (slot.occupyingObject != null)
            {
                toDestroy.Add(slot.occupyingObject);
                slot.ClearOccupied();
            }
        }

#if UNITY_EDITOR
        // Deselect any objects to prevent Unity Editor errors
        UnityEditor.Selection.activeObject = null;
#endif

        // Destroy all at once - ensure we're destroying GameObjects, not components
        foreach (GameObject soldierObj in toDestroy)
        {
            if (soldierObj != null)
            {
                // Make sure we're destroying the root GameObject, not a child component
                Destroy(soldierObj.gameObject);
            }
        }

        // Wait for all destroys to complete
        yield return null;
        
        // Wait an extra frame to ensure cleanup
        yield return null;

        // Step 2: Load and instantiate new soldiers
        foreach (KeyValuePair<(int, int), SoldierDataForSerialization> keyValuePair in grid)
        {
            // Get the slot at the position
            ItemSlot slot = gridBuilder.customGrid.ContainsKey(keyValuePair.Key) ? gridBuilder.customGrid[keyValuePair.Key] : null;

            if (slot != null)
            {
                SoldierDataForSerialization data = keyValuePair.Value;

                // Load SoldierType from registry
                if (soldierTypeRegistry == null)
                {
                    Debug.LogError("SoldierTypeRegistry is not assigned in LoadingSystem! Please assign it in the Inspector.");
                    yield break;
                }
                
                SoldierType soldierType = soldierTypeRegistry.GetSoldierTypeByName(data.soldierTypeName);

                if (soldierType != null && soldierType.soldierPrefab != null)
                {
                    // Instantiate the soldier prefab at the slot position (matching DragDrop behavior)
                    Vector3 spawnPosition = slot.transform.position;
                    spawnPosition.z = 0f; // Ensure correct z-depth
                    
                    GameObject soldierObj = Instantiate(soldierType.soldierPrefab, spawnPosition, Quaternion.identity);
                    
                    // Set Rigidbody to Kinematic (matching DragDrop)
                    var rb = soldierObj.GetComponent<Rigidbody2D>();
                    if (rb != null) 
                    { 
                        rb.bodyType = RigidbodyType2D.Kinematic; 
                        rb.linearVelocity = Vector2.zero; 
                    }
                    
                    SoldierBehaviour soldierBehaviour = soldierObj.GetComponent<SoldierBehaviour>();

                    if (soldierBehaviour != null)
                    {
                        // Parse the tier enum from the saved string
                        if (System.Enum.TryParse<SoldierTierList.TierEnum>(data.soldierTierName, out SoldierTierList.TierEnum tierEnum))
                        {
                            soldierBehaviour.tier = tierEnum;
                            
                            // Apply the tier color to the sprite renderer
                            if (soldierBehaviour.spriteRenderer != null && SoldierTierList.tierDictionary.ContainsKey(tierEnum))
                            {
                                Color tierColor = SoldierTierList.tierDictionary[tierEnum].tierColor;
                                soldierBehaviour.spriteRenderer.color = tierColor;
                                Debug.Log($"Loaded soldier: {data.soldierTypeName} with tier {tierEnum} and color {tierColor}");
                            }
                            else
                            {
                                Debug.Log($"Loaded soldier: {data.soldierTypeName} with tier {tierEnum}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse tier name: {data.soldierTierName}");
                            soldierBehaviour.tier = SoldierTierList.TierEnum.Common;
                        }
                    }

                    // Place the soldier in the slot
                    soldierObj.transform.SetParent(slot.transform);
                    slot.SetOccupied(soldierObj);
                    
                    // Ensure local position is correct after parenting
                    soldierObj.transform.localPosition = new Vector3(0, 0, 0);
                }
                else
                {
                    Debug.LogWarning($"Could not find SoldierType '{data.soldierTypeName}' or its prefab");
                }
            }
        }

        // Step 3: Restore the score
        if (meta != null && meta.TryGetValue("points", out string pointsStr) && int.TryParse(pointsStr, out int points))
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                int currentScore = scoreManager.CurrentScore;
                int difference = points - currentScore;

                if (difference > 0)
                {
                    scoreManager.AddPoints(difference);
                }
                else if (difference < 0)
                {
                    scoreManager.TrySpend(-difference);
                }

                Debug.Log($"Loaded score: {points} (current was {currentScore})");
            }
        }

        Debug.Log("Game data loaded successfully!");
    }

    /// <summary>
    /// Check if a save file exists
    /// </summary>
    public bool SaveFileExists()
    {
        string projectSavesFolder = Path.Combine(Application.dataPath, "SavesJson");
        string fullPath = Path.Combine(projectSavesFolder, "MyFirstSave.json");
        return File.Exists(fullPath);
    }
}
