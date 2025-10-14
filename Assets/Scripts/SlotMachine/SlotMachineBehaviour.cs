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
    public TMP_Text scoreText;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public SoldierType WeightedRandomSoldierSelection()
    {
        float randomIndex = Random.Range(0.01f, 100.00f);

        foreach (CharactherAndProbability characther in possibleResults)
        {
            if (randomIndex >= characther.probability)
            {
                return characther.soldierType;
            }
        }
        return possibleResults[possibleResults.Length - 1].soldierType;
    }

    public SoldierTier WeightedRaritySelection()
    {
        float randomIndex = Random.Range(0.01f, 100.00f);
        foreach (RarityAndProbability rarity in possibleRarities)
        {
            if (randomIndex >= rarity.probability)
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
        int currentScore = int.Parse(scoreText.text);
        if (currentScore < 100)
        {
            Debug.LogWarning("Not enough coins to spin!");
            return;
        }

        currentScore -= 100;
        scoreText.text = currentScore.ToString();

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

        // Add DragDrop component and configure it
        var dragDrop = rewardSlotImage.gameObject.AddComponent<DragDrop>();
        dragDrop.canvas = targetCanvas;

        // Ensure CanvasGroup exists for drag functionality
        var canvasGroup = rewardSlotImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = rewardSlotImage.gameObject.AddComponent<CanvasGroup>();

        // Enable raycast target for dragging
        rewardSlotImage.raycastTarget = true;

        Debug.Log($"Reward slot image is now draggable for {rolledCharacter.soldierType.name}");
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
