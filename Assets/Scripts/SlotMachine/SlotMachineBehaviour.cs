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
