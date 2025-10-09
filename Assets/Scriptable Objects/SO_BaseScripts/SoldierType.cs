using UnityEngine;

[CreateAssetMenu(fileName = "SoldierType", menuName = "ScriptableObjects/Soldiers/Soldier Type")]
public class SoldierType : ScriptableObject
{
    [Header("Soldier Info")]
    public new string name;
    public Sprite characterSprite;

    [Header("Soldier Stats")]
    public SoldierStats stats;  // <— all your numbers live here

    // returns a copy (struct copies by value)
    public SoldierStats GetStatsCopy() => stats;
}

[System.Serializable]
public struct SoldierStats
{
    public float bulletSpeed;
    public float bulletLife;
    public float attackSpeed;
    public float health;
    public float dmg;
    public float cost;
}
