using UnityEngine;

[CreateAssetMenu(fileName = "SoldierType", menuName = "ScriptableObjects/Soldiers/Soldier Type")]
public class SoldierType : ScriptableObject
{
    [Header("Soldier Info")]
    public new string name;
    public Sprite characterSprite;
    public GameObject soldierPrefab;  // Prefab for spawning the actual soldier in-game

    [Header("Soldier Stats")]
    public SoldierStats stats;  // <— all your numbers live here

    // returns a copy (struct copies by value)
    public SoldierStats GetStatsCopy() => stats;
}
