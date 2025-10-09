using System;
using System.Collections.Generic;
using UnityEngine;


public static class SoldierTierList
{

    public enum TierEnum
    {
        Common,
        Uncommon,
        Rare,
        SuperRare,
        Ultra
    }
    public static Dictionary<TierEnum, SoldierTier> tierDictionary = new()
    {
        [TierEnum.Common] = new SoldierTier
        {
            tierType = TierEnum.Common,
            tierColor = Color.white,
            tierChanges = s => { return s; },
            tierWeight = 0
        },

        [TierEnum.Uncommon] = new SoldierTier
        {
            tierType = TierEnum.Uncommon,
            tierColor = Color.green,
            tierChanges = s =>
            {
                s.bulletSpeed += 5f;
                s.attackSpeed -= 0.2f;
                s.health += 5f;
                s.dmg += 5f;
                return s;
            },
            tierWeight = 1
        },

        [TierEnum.Rare] = new SoldierTier
        {
            tierType = TierEnum.Rare,
            tierColor = Color.yellow,
            tierChanges = s =>
            {
                s.bulletSpeed += 7f;
                s.attackSpeed -= 0.3f;
                s.health += 10f;
                s.dmg += 7f;
                return s;
            },
            tierWeight = 2
        },

        [TierEnum.SuperRare] = new SoldierTier
        {
            tierType = TierEnum.SuperRare,
            tierColor = new Color(1f, 0.5f, 0f, 1), // Orange
            tierChanges = s =>
            {
                s.bulletSpeed += 8f;
                s.attackSpeed -= 0.4f;
                s.health += 15f;
                s.dmg += 10f;
                return s;
            },
            tierWeight = 3
        },

        [TierEnum.Ultra] = new SoldierTier
        {
            tierType = TierEnum.Ultra,
            tierColor = new Color(0.5f, 0f, 0.5f), // Purple
            tierChanges = s =>
            {
                s.bulletSpeed += 10f;
                s.attackSpeed -= 0.6f;
                s.health += 30f;
                s.dmg += 20f;
                return s;
            },
            tierWeight = 4
        },
    };
}

[System.Serializable]
public class SoldierTier
{
    public SoldierTierList.TierEnum tierType;
    [HideInInspector] public Color tierColor { get; set; }
    public Func<SoldierStats, SoldierStats> tierChanges { get; set; }
    [HideInInspector] public int tierWeight;

}
