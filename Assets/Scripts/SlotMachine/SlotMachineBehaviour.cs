using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class SlotMachineBehaviour : MonoBehaviour
{
    public CharactherAndProbability[] possibleResults;
    public RarityAndProbability[] possibleRarities;
    private CharactherAndRarity[] currentResults = new CharactherAndRarity[3];
    public Image[] slotMachineImages = new Image[3];
    public Image[] slotMachineBGImages = new Image[3];
    public Image rewardSlotImage;
    public Canvas dragCanvas; // Canvas for drag operations

    public SoldierType WeightedRandomSoldierSelection()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (CharactherAndProbability characther in possibleResults)
        {
            totalWeight += characther.probability;
        }

        // Generate random value between 0 and total weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // Find which item the random value falls into
        foreach (CharactherAndProbability characther in possibleResults)
        {
            cumulativeWeight += characther.probability;
            if (randomValue < cumulativeWeight)
            {
                return characther.soldierType;
            }
        }
        
        // Fallback (should never reach here if probabilities are set correctly)
        return possibleResults[possibleResults.Length - 1].soldierType;
    }

    public SoldierTier WeightedRaritySelection()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (RarityAndProbability rarity in possibleRarities)
        {
            totalWeight += rarity.probability;
        }

        // Generate random value between 0 and total weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        // Find which rarity the random value falls into
        foreach (RarityAndProbability rarity in possibleRarities)
        {
            cumulativeWeight += rarity.probability;
            if (randomValue < cumulativeWeight)
            {
                return SoldierTierList.tierDictionary[rarity.type];
            }
        }
        
        // Fallback (should never reach here if probabilities are set correctly)
        SoldierTierList.TierEnum lastType = possibleRarities[possibleRarities.Length - 1].type;
        return SoldierTierList.tierDictionary[lastType];
    }
    [NaughtyAttributes.Button]
    public void Spin()
    {
        // Use ScoreManager instead of directly reading/writing scoreText
        if (ScoreManager.instance == null)
        {
            Debug.LogError("ScoreManager.instance not found! Cannot process spin.");
            return;
        }
        
        int spinCost = 100;
        if (ScoreManager.instance.CurrentScore < spinCost)
        {
            Debug.LogWarning($"Not enough coins to spin! Need {spinCost}, have {ScoreManager.instance.CurrentScore}");
            return;
        }

        // Attempt to spend coins via ScoreManager
        if (!ScoreManager.instance.TrySpend(spinCost))
        {
            Debug.LogWarning("Failed to spend coins for slot machine spin!");
            return;
        }
        
        Debug.Log($"Slot machine spin cost {spinCost} coins. Remaining: {ScoreManager.instance.CurrentScore}");

        slotMachineImages[0].sprite = possibleResults[0].soldierType.characterSprite;
        SoldierTier newTier = WeightedRaritySelection();

        for (int i = 0; i < currentResults.Length; i++)
        {
            SoldierType newSoldier = WeightedRandomSoldierSelection();
            slotMachineImages[i].sprite = newSoldier.characterSprite;

            slotMachineBGImages[i].color = newTier.tierColor;

            currentResults[i] = new CharactherAndRarity() { soldierType = newSoldier, tier = newTier };
        }

        CharactherAndRarity rolledCharacther = GetRolledCharacther();
        Debug.Log($"{rolledCharacther.soldierType.name} - {rolledCharacther.tier.tierType}");
        
        // Display the rolled character in the reward slot and make it draggable
        if (rewardSlotImage != null)
        {
            rewardSlotImage.sprite = rolledCharacther.soldierType.characterSprite;
            rewardSlotImage.color = rolledCharacther.tier.tierColor; // Set color based on rarity
            SetupRewardSlotDragging(rolledCharacther);
        }
    }

    public CharactherAndRarity GetRolledCharacther()
    {
        CharactherAndRarity endResult = new CharactherAndRarity();
        endResult.tier = currentResults[0].tier;
        for (int i = 1; i < currentResults.Length; i++)
        {
            if (currentResults[i].tier.tierWeight < endResult.tier.tierWeight)
            {
                endResult = currentResults[i];
            }

        }
        Dictionary<SoldierType, int> frequency = new Dictionary<SoldierType, int>();
        foreach (var result in currentResults)
        {
            SoldierType soldier = result.soldierType;
            if (frequency.ContainsKey(soldier))
            {
                frequency[soldier]++;
            }
            else
            {
                frequency[soldier] = 1;
            }
        }

        SoldierType mostFrequent = null;
        int maxCount = 0;
        foreach (var pair in frequency)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
                mostFrequent = pair.Key;
            }
        }

        endResult.soldierType = mostFrequent;
        return endResult;
    }

    private void SetupRewardSlotDragging(CharactherAndRarity rolledCharacter)
    {
        if (rewardSlotImage == null) return;

        // Find or create DragCanvas (similar to shop system)
        Canvas targetCanvas = dragCanvas;
        if (targetCanvas == null)
        {
            var existingDragCanvas = GameObject.Find("DragCanvas");
            if (existingDragCanvas != null)
            {
                targetCanvas = existingDragCanvas.GetComponent<Canvas>();
            }
            else
            {
                GameObject dc = new GameObject("DragCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                targetCanvas = dc.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 1000;
                dc.transform.SetParent(null);
            }
        }

        // Remove existing DragDrop component if present
        var existingDrag = rewardSlotImage.GetComponent<DragDrop>();
        if (existingDrag != null) DestroyImmediate(existingDrag);

        // Add DragDrop component and configure it AS A SOURCE (so it stays in place and spawns clones)
        var dragDrop = rewardSlotImage.gameObject.AddComponent<DragDrop>();
        dragDrop.canvas = targetCanvas;
        dragDrop.isSource = true;  // CRITICAL: Mark as source so it creates clones instead of moving
        dragDrop.oneTimeUse = true; // CRITICAL: Only allow placing the reward once
        dragDrop.sourceSprite = rolledCharacter.soldierType.characterSprite;
        dragDrop.sourceColor = rolledCharacter.tier.tierColor; // Set the tier color for clones
        dragDrop.soldierColor = rolledCharacter.tier.tierColor; // Set the tier color for instantiated prefab
        dragDrop.soldierTier = rolledCharacter.tier.tierType; // CRITICAL: Set the tier enum for stat scaling
        
        // Assign the soldier prefab so the clone knows what to instantiate when dropped
        if (rolledCharacter.soldierType.soldierPrefab != null)
        {
            dragDrop.soldierPrefab = rolledCharacter.soldierType.soldierPrefab;
            Debug.Log($"Assigned soldierPrefab: {rolledCharacter.soldierType.soldierPrefab.name}");
        }
        else
        {
            Debug.LogWarning($"No soldierPrefab found on {rolledCharacter.soldierType.name}");
        }

        // Ensure CanvasGroup exists for drag functionality
        var canvasGroup = rewardSlotImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = rewardSlotImage.gameObject.AddComponent<CanvasGroup>();

        // Enable raycast target for dragging
        rewardSlotImage.raycastTarget = true;

        Debug.Log($"Reward slot image is now draggable (SOURCE MODE, ONE-TIME USE) for {rolledCharacter.soldierType.name}");
    }

    [System.Serializable]
    public class CharactherAndProbability
    {
        public SoldierType soldierType;
        public float probability;
    }
    [System.Serializable]
    public class RarityAndProbability
    {
        public SoldierTierList.TierEnum type;
        public float probability;
    }
    [System.Serializable]
    public class CharactherAndRarity
    {
        public SoldierType soldierType;
        public SoldierTier tier;
    }
}
