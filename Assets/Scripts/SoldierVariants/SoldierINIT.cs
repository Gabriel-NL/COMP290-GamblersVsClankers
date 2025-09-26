using Unity.VisualScripting;
using UnityEngine;

public class SoldierINIT : MonoBehaviour
{
    public Soldier SoldierType;
    public SoldierTier SoldierTier;
    public Transform firePoint;
    public GameObject bulletPrefab;
    //public GameObject healthBar;

    [HideInInspector] public float bulletSpeed;
    [HideInInspector] public float bulletLife;
    [HideInInspector] public float attackSpeed;
    [HideInInspector] public float health;
    [HideInInspector] public float dmg;
    [HideInInspector] public string cost;

    //[HideInInspector] public string shootAudioName;

    void Start()
    {
        SetSoldierType();
    }

    private void SetSoldierType()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gameObject.name = SoldierType.name;

        switch (SoldierType.type)
        {
            case Soldier.TypeOfSoldier.Pistol:
                // Pistol specific settings
                bulletSpeed = 20f;
                attackSpeed = 2f;
                health = 50f;
                dmg = 10f;
                cost = "50";
                break;
            case Soldier.TypeOfSoldier.Rifleman:
                // Rifleman specific settings
                bulletSpeed = 30f;
                attackSpeed = 1.5f;
                health = 75f;
                dmg = 35f;
                cost = "100";
                break;
            case Soldier.TypeOfSoldier.ARSoldier:
                // ARSoldier specific settings
                bulletSpeed = 40f;
                attackSpeed = 0.5f;
                health = 100f;
                dmg = 25f;
                cost = "175";
                break;
            case Soldier.TypeOfSoldier.LaserMan:
                // LaserMan specific settings
                bulletSpeed = 60f;
                attackSpeed = 0.2f;
                health = 150f;
                dmg = 15f;
                cost = "250";
                break;
        }
        //spriteRenderer.sprite = SoldierType.characterSprite;
        //firePoint.GetComponent<SpriteRenderer>().sprite = SoldierType.weaponSprite;
        //shootAudioName = SoldierType.shootAudioName;

        Invoke("SetSoldierTier", 2f);
    }

    private void SetSoldierTier()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        gameObject.name += " - " + SoldierTier.name;

        switch (SoldierTier.tier)
        {
            case SoldierTier.TierOfSoldier.common:
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // white
                break;
            case SoldierTier.TierOfSoldier.uncommon:
                bulletSpeed += 5f;
                attackSpeed -= 0.2f;
                health += 5f;
                dmg += 5f;
                spriteRenderer.color = new Color(1f, 1f, 0.5f); // yellow
                break;
            case SoldierTier.TierOfSoldier.Rare:
                bulletSpeed += 7f;
                attackSpeed -= 0.3f;
                health += 10f;
                dmg += 7f;
                spriteRenderer.color = new Color(0.5f, 0.5f, 1f); // Light blue
                break;
            case SoldierTier.TierOfSoldier.SuperRare:
                bulletSpeed += 8f;
                attackSpeed -= 0.4f;
                health += 15f;
                dmg += 10f;
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // purple
                break;
            case SoldierTier.TierOfSoldier.Ultra:
                bulletSpeed += 10f;
                attackSpeed -= 0.6f;
                health += 30f;
                dmg += 20f;
                spriteRenderer.color = new Color(1f, 0.84f, 0f); // gold
                break;
        }

        
        // healthBar.GetComponent<Slider>().maxValue = health;
        // healthBar.GetComponent<Slider>().value = health;
    }
}