using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierBehaviour : MonoBehaviour
{
    [Header("References")]
    public SoldierType SoldierType;
    public SoldierTierList.TierEnum tier;
    [MustBeAssigned] public Transform firePoint;
    [MustBeAssigned] public SpriteRenderer spriteRenderer;
    [MustBeAssigned] public GameObject bulletPrefab;

    //[HideInInspector] public string shootAudioName;

    //public GameObject healthBar;
    [Header("Soldier Stats (Read-Only)")]
    [ReadOnlyInInspector] public float bulletSpeed;
    [ReadOnlyInInspector] public float bulletLife;
    [ReadOnlyInInspector] public float attackSpeed;
    [ReadOnlyInInspector] public float health;
    [ReadOnlyInInspector] public float dmg;
    // legacy public timer kept for inspector visibility if needed, but firing uses attackSpeed

    [Header("Timer")]
    public float timer;
    private float cooldownTimer;

    void Start()
    {
        Initialization();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        if (cooldownTimer <= 0f)
        {
            Fire();

            cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 1f);
        }
    }

    [NaughtyAttributes.Button("Import data from SoldierType SO")]
    private void SetSoldierType()
    {
        gameObject.name += " - " + SoldierType.name;
        gameObject.name = SoldierType.name;
        spriteRenderer.sprite = SoldierType.characterSprite;
        bulletSpeed = SoldierType.stats.bulletSpeed;
        bulletLife = SoldierType.stats.bulletLife;
        attackSpeed = SoldierType.stats.attackSpeed;
        health = SoldierType.stats.health;
        dmg = SoldierType.stats.dmg;
    }
    [NaughtyAttributes.Button("Apply Tier Changes")]

    private void ApplyTierChanges()
    {
        SoldierStats soldierModdedStats = SoldierType.GetStatsCopy();

        soldierModdedStats = SoldierTierList.tierDictionary[tier].tierChanges(soldierModdedStats);

        bulletSpeed = soldierModdedStats.bulletSpeed;
        bulletLife = soldierModdedStats.bulletLife;
        attackSpeed = soldierModdedStats.attackSpeed;
        health = soldierModdedStats.health;
        dmg = soldierModdedStats.dmg;
    }

    private void Initialization()
    {
        SetSoldierType();
        ApplyTierChanges();
        cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 0f);
        Debug.Log($"[SoldierINIT] Initialized attackSpeed={attackSpeed}, legacy timer={timer}, cooldownTimer={cooldownTimer} on '{gameObject.name}'");
    }
    public void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(firePoint.up * bulletSpeed, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning($"[SoldierINIT] Instantiated bullet has no Rigidbody2D on '{gameObject.name}'.");
        }
        // Log creation details to help debug why bullets might not be visible

        // Draw a short debug ray showing firing direction in the Scene view
        Debug.DrawRay(firePoint.position, firePoint.up * 2f, Color.red, 0.5f);

        Destroy(bullet, bulletLife); // Destroy bullet after bulletLife seconds
    }

}
