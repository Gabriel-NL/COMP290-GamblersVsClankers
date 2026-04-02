using UnityEngine;

[System.Serializable]
public struct SoldierStats
{
    public float bulletSpeed;
    public float bulletLife;
    public float attackSpeed;
    public float health;
    public float dmg;
    public float cost;
    [Header("Piercing Info")]
    public bool isShootThrough;
    [Header("Air Targeting")]
    public bool canShootAir;
    [Header("Shotgun Info")]
    public bool isShotgun;
    [Header("EMP Grenade Info")]
    public bool isEMPGrenade;
    public float stunDuration;
    public float aoeRadius;
    [Header("Water Bomb Info")]
    public bool isWaterBomb;
}
