using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SoldierType", menuName = "Soldiers/SoldierType")]
public class Soldier : ScriptableObject
{
    [Header("Soldier Details")]
    public new string name;
    //public Sprite characterSprite;
    public enum TypeOfSoldier
    {
        Pistol,
        Rifleman,
        ARSoldier,
        LaserMan
    }
    [Tooltip("Select soldier type")]
    public TypeOfSoldier type;

    // public Sprite weaponSprite;
    // //public RuntimeAnimatorController AnimController;
    // public Sprite bulletSprite;
    // public Sprite characterSprite;
    //public string shootAudioName;

    
}
