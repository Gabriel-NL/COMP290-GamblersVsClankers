using System;
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
