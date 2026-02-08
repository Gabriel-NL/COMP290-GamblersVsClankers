using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "ScriptableObjects/Enemies/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Enemy Info")]
    public new string name;
    public Sprite characterSprite;
    public GameObject enemyPrefab;  // Prefab for spawning the actual enemy in-game

    [Header("Enemy Stats")]
    public EnemyStats stats;  // <— all your numbers live here

    // returns a copy (struct copies by value)
    public EnemyStats GetStatsCopy() => stats;
}

[System.Serializable]
public struct EnemyStats
{
    public float health;
    public float dmg;
    public float reward;
    public float speed;
    public bool isFlying;
}
