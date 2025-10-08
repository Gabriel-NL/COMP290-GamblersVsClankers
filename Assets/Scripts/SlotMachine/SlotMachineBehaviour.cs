using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachineBehaviour : MonoBehaviour
{
    public CharactherAndProbability[] possibleResults;
    public RarityAndProbability[] possibleRarities;
    private CharactherAndRarity[] currentResults = new CharactherAndRarity[3];




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
            if (randomIndex <= characther.probability)
            {
                return characther.soldierType;
            }
        }
        return possibleResults[possibleResults.Length - 1].soldierType;
    }

    public SoldierTierList.TierEnum WeightedRaritySelection()
    {
        float randomIndex = Random.Range(0.01f, 100.00f);
        foreach (RarityAndProbability rarity in possibleRarities)
        {
            if (randomIndex <= rarity.probability)
            {
                return rarity.soldierType;
            }
        }

        return (SoldierTierList.TierEnum)(possibleRarities.Length - 1);
    }
    public void Spin()
    {
        for (int i = 0; i < currentResults.Length; i++)
        {
            currentResults[i] = new CharactherAndRarity
            {
                soldierType = WeightedRandomSoldierSelection(),
                tier = WeightedRaritySelection()
            };
        }
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
        public SoldierTierList.TierEnum soldierType;
        public float probability;
    }
    [System.Serializable]
    public class CharactherAndRarity
    {
        public SoldierType soldierType;
        public SoldierTierList.TierEnum tier;
    }
}
