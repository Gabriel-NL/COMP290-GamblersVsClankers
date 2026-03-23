using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PreMadeSummon
{
    [Min(0)] public int laneIndex;
    [Min(0f)] public float relativeDelay;
    [Min(0)] public int enemyPrefabIndex;

    public PreMadeSummon(int laneIndex, float relativeDelay, int enemyPrefabIndex)
    {
        this.laneIndex = laneIndex;
        this.relativeDelay = relativeDelay;
        this.enemyPrefabIndex = enemyPrefabIndex;
    }
}

[Serializable]
public sealed class PreMadeHorde
{
    public List<PreMadeSummon> summons = new List<PreMadeSummon>();

    public PreMadeHorde()
    {
    }

    public PreMadeHorde(params PreMadeSummon[] summons)
    {
        if (summons != null)
        {
            this.summons.AddRange(summons);
        }
    }
}

public static class HordeDefinitions
{
    // Central place for all hardcoded constants.
    public const float InterHordeCooldownSeconds = 2.0f;
    public const float LastHordeCooldownSeconds = 5.0f;

    public static Dictionary<int, PreMadeHorde> BuildDictionary()
    {
        Dictionary<int, PreMadeHorde> result = new Dictionary<int, PreMadeHorde>(OrderedHordes.Count);

        for (int i = 0; i < OrderedHordes.Count; i++)
        {
            result[i + 1] = OrderedHordes[i];
        }

        return result;
    }
    
    // Edit the hordes here
    public static readonly IReadOnlyList<PreMadeHorde> OrderedHordes = new List<PreMadeHorde>
    {
        new PreMadeHorde(
            new PreMadeSummon(laneIndex: 0, relativeDelay: 5f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 1.8f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 1, relativeDelay: 1.2f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 2.1f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 1.6f, enemyPrefabIndex: 0),

            new PreMadeSummon(laneIndex: 0, relativeDelay: 2.4f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 1.3f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 1, relativeDelay: 2.0f, enemyPrefabIndex: 2),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.7f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 2.6f, enemyPrefabIndex: 1),

            new PreMadeSummon(laneIndex: 2, relativeDelay: 1.5f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 0, relativeDelay: 2.2f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 1, relativeDelay: 1.1f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 2.8f, enemyPrefabIndex: 2),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.9f, enemyPrefabIndex: 1),

            new PreMadeSummon(laneIndex: 0, relativeDelay: 1.6f, enemyPrefabIndex: 3),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 2.3f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 1.4f, enemyPrefabIndex: 4),
            new PreMadeSummon(laneIndex: 1, relativeDelay: 2.7f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.8f, enemyPrefabIndex: 5),
            
            // CyberTruck spawn
            new PreMadeSummon(laneIndex: 2, relativeDelay: 3.0f, enemyPrefabIndex: 6)
        ),
        new PreMadeHorde(
            new PreMadeSummon(laneIndex: 1, relativeDelay: 1.3f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.7f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 0, relativeDelay: 1.2f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 2.0f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 1.5f, enemyPrefabIndex: 2),

            new PreMadeSummon(laneIndex: 1, relativeDelay: 2.2f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.4f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 0, relativeDelay: 2.1f, enemyPrefabIndex: 2),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 1.6f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 2.5f, enemyPrefabIndex: 1),

            new PreMadeSummon(laneIndex: 1, relativeDelay: 1.8f, enemyPrefabIndex: 3),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.1f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 0, relativeDelay: 2.4f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 1.9f, enemyPrefabIndex: 2),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 1.7f, enemyPrefabIndex: 4),

            new PreMadeSummon(laneIndex: 1, relativeDelay: 2.3f, enemyPrefabIndex: 0),
            new PreMadeSummon(laneIndex: 3, relativeDelay: 1.5f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 0, relativeDelay: 2.6f, enemyPrefabIndex: 2),
            new PreMadeSummon(laneIndex: 4, relativeDelay: 1.4f, enemyPrefabIndex: 1),
            new PreMadeSummon(laneIndex: 2, relativeDelay: 2.0f, enemyPrefabIndex: 5),
            
            // CyberTruck spawn
            new PreMadeSummon(laneIndex: 0, relativeDelay: 3.5f, enemyPrefabIndex: 6)
        ),
        new PreMadeHorde(
    new PreMadeSummon(laneIndex: 0, relativeDelay: 1.4f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.8f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 2.2f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 0),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.5f, enemyPrefabIndex: 3),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 2),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 2.6f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 0),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.7f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 4),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 2.0f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 1, relativeDelay: 0.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 3),

    new PreMadeSummon(laneIndex: 2, relativeDelay: 2.4f, enemyPrefabIndex: 5),
    new PreMadeSummon(laneIndex: 0, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.9f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 3),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 4),
    
    // CyberTruck spawn
    new PreMadeSummon(laneIndex: 3, relativeDelay: 3.2f, enemyPrefabIndex: 6)
),
        new PreMadeHorde(
    new PreMadeSummon(laneIndex: 0, relativeDelay: 1.2f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.6f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 2.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 1.4f, enemyPrefabIndex: 3),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 2.3f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 1, relativeDelay: 0.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 4, relativeDelay: 1.8f, enemyPrefabIndex: 4),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 1),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 1.5f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 3),

    new PreMadeSummon(laneIndex: 1, relativeDelay: 2.1f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 5),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 1.7f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 1, relativeDelay: 0.0f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 3, relativeDelay: 0.0f, enemyPrefabIndex: 4),

    new PreMadeSummon(laneIndex: 4, relativeDelay: 2.4f, enemyPrefabIndex: 2),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 3),

    new PreMadeSummon(laneIndex: 0, relativeDelay: 1.3f, enemyPrefabIndex: 1),
    new PreMadeSummon(laneIndex: 2, relativeDelay: 0.0f, enemyPrefabIndex: 0),
    new PreMadeSummon(laneIndex: 4, relativeDelay: 0.0f, enemyPrefabIndex: 5),
    
    // CyberTruck spawn
    new PreMadeSummon(laneIndex: 1, relativeDelay: 2.8f, enemyPrefabIndex: 6)
),
    };
}