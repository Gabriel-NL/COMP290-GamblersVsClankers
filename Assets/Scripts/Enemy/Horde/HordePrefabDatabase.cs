using UnityEngine;

public sealed class HordePrefabDatabase : MonoBehaviour
{
    [Header("Filled from Inspector. HordeDefinitions references these by index.")]
    public GameObject[] enemyPrefabs;
}