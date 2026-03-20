using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        float totalWeight = 0f;
        foreach (CharactherAndProbability characther in possibleResults)
        {
            totalWeight += characther.probability;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (CharactherAndProbability characther in possibleResults)
        {
            cumulativeWeight += characther.probability;
            if (randomValue < cumulativeWeight)
            {
                return characther.soldierType;
            }
        }

        return possibleResults[possibleResults.Length - 1].soldierType;
    }

    public SoldierTier WeightedRaritySelection()
    {
        float totalWeight = 0f;
        foreach (RarityAndProbability rarity in possibleRarities)
        {
            totalWeight += rarity.probability;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (RarityAndProbability rarity in possibleRarities)
        {
            cumulativeWeight += rarity.probability;
            if (randomValue < cumulativeWeight)
            {
                return SoldierTierList.tierDictionary[rarity.type];
            }
        }

        SoldierTierList.TierEnum lastType = possibleRarities[possibleRarities.Length - 1].type;
        return SoldierTierList.tierDictionary[lastType];
    }

    [NaughtyAttributes.Button]
    public void Spin()
    {
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

        if (!ScoreManager.instance.TrySpend(spinCost))
        {
            Debug.LogWarning("Failed to spend coins for slot machine spin!");
            return;
        }

        Debug.Log($"Slot machine spin cost {spinCost} coins. Remaining: {ScoreManager.instance.CurrentScore}");

        SoldierTier newTier = WeightedRaritySelection();

        for (int i = 0; i < currentResults.Length; i++)
        {
            SoldierType newSoldier = WeightedRandomSoldierSelection();
            slotMachineImages[i].sprite = newSoldier.characterSprite;
            slotMachineBGImages[i].color = newTier.tierColor;

            currentResults[i] = new CharactherAndRarity()
            {
                soldierType = newSoldier,
                tier = newTier
            };
        }

        CharactherAndRarity rolledCharacther = GetRolledCharacther();
        Debug.Log($"{rolledCharacther.soldierType.name} - {rolledCharacther.tier.tierType}");

        if (rewardSlotImage != null)
        {
            rewardSlotImage.sprite = rolledCharacther.soldierType.characterSprite;
            rewardSlotImage.color = rolledCharacther.tier.tierColor;
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

        Canvas targetCanvas = dragCanvas;
        if (targetCanvas == null)
        {
            GameObject existingDragCanvas = GameObject.Find("DragCanvas");
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

        DragDrop existingDrag = rewardSlotImage.GetComponent<DragDrop>();
        if (existingDrag != null)
        {
            DestroyImmediate(existingDrag);
        }

        DragDrop dragDrop = rewardSlotImage.gameObject.AddComponent<DragDrop>();
        dragDrop.canvas = targetCanvas;
        dragDrop.isSource = true;
        dragDrop.oneTimeUse = true;
        dragDrop.sourceSprite = rolledCharacter.soldierType.characterSprite;
        dragDrop.sourceColor = rolledCharacter.tier.tierColor;
        dragDrop.soldierColor = rolledCharacter.tier.tierColor;
        dragDrop.soldierTier = rolledCharacter.tier.tierType;

        if (rolledCharacter.soldierType.soldierPrefab != null)
        {
            dragDrop.soldierPrefab = rolledCharacter.soldierType.soldierPrefab;
            Debug.Log($"Assigned soldierPrefab: {rolledCharacter.soldierType.soldierPrefab.name}");
        }
        else
        {
            Debug.LogWarning($"No soldierPrefab found on {rolledCharacter.soldierType.name}");
        }

        // When drag starts from reward slot, clear the source UI image,
        // but only after DragDrop has already created the dragged visual.
        dragDrop.onBeginDragFromSource = () =>
        {
            rewardSlotImage.sprite = null;
            rewardSlotImage.color = Color.clear;
            rewardSlotImage.raycastTarget = false;
        };

        // If drop fails / is cancelled, restore the reward slot image.
        dragDrop.onCancelledOrInvalidDrop = () =>
        {
            rewardSlotImage.sprite = rolledCharacter.soldierType.characterSprite;
            rewardSlotImage.color = rolledCharacter.tier.tierColor;
            rewardSlotImage.raycastTarget = true;
        };

        // If drop succeeds, keep it empty because it was consumed.
        dragDrop.onSuccessfulDrop = () =>
        {
            rewardSlotImage.sprite = null;
            rewardSlotImage.color = Color.clear;
            rewardSlotImage.raycastTarget = false;
        };

        CanvasGroup canvasGroup = rewardSlotImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rewardSlotImage.gameObject.AddComponent<CanvasGroup>();
        }

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