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
    public static Dictionary<TierEnum, Tier> tierDictionary = new()
    {
        [TierEnum.Common] = new Tier
        {
            tierColor = Color.white,
            tierChanges = s => { return s; }
        },

        [TierEnum.Uncommon] = new Tier
        {
            tierColor = Color.green,
            tierChanges = s =>
            {
                s.bulletSpeed += 5f;
                s.attackSpeed -= 0.2f;
                s.health += 5f;
                s.dmg += 5f;
                return s;
            }
        },

        [TierEnum.Rare] = new Tier
        {
            tierColor = Color.yellow,
            tierChanges = s =>
            {
                s.bulletSpeed += 7f;
                s.attackSpeed -= 0.3f;
                s.health += 10f;
                s.dmg += 7f;
                return s;
            }
        },

        [TierEnum.SuperRare] = new Tier
        {
            tierColor = new Color(1f, 0.5f, 0f), // Orange
            tierChanges = s =>
            {
                s.bulletSpeed += 8f;
                s.attackSpeed -= 0.4f;
                s.health += 15f;
                s.dmg += 10f;
                return s;
            }
        },

        [TierEnum.Ultra] = new Tier
        {
            tierColor = new Color(0.5f, 0f, 0.5f), // Purple
            tierChanges = s =>
            {
                s.bulletSpeed += 10f;
                s.attackSpeed -= 0.6f;
                s.health += 30f;
                s.dmg += 20f;
                return s;
            }
        },
    };
}

[System.Serializable]
public class Tier
{
    public Color tierColor;
    public Func<SoldierStats, SoldierStats> tierChanges { get; set; }
}
