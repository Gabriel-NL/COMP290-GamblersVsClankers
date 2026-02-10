using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SoldierBehaviour : MonoBehaviour
{
    [Header("References")]
    public SoldierType SoldierType;
    public SoldierTierList.TierEnum tier;
    [MustBeAssigned] public Transform firePoint;
    [MustBeAssigned] public SpriteRenderer spriteRenderer;
    [MustBeAssigned] public GameObject bulletPrefab;
    public SoldierHealthBar healthBar; // Health bar component (optional)

    [Header("Detection")]
    public float detectionRange = 10f;
    public LayerMask enemyLayer;

    //[HideInInspector] public string shootAudioName;

    [Header("Soldier Stats (Read-Only)")]
    [ReadOnly] public float bulletSpeed;
    [ReadOnly] public float bulletLife;
    [ReadOnly] public float attackSpeed;
    [ReadOnly] public float maxHealth; // Maximum health
    [ReadOnly] public float currentHealth; // Current health
    [ReadOnly] public float dmg;
    [ReadOnly] public bool isShootThrough;
    [ReadOnly] public bool isShotgun;
    // legacy public timer kept for inspector visibility if needed, but firing uses attackSpeed

    [Header("Timer")]
    public float timer;
    private float cooldownTimer;
    private float bulletOffsetValue = 1f;


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

        if (IsEnemyInLane())
        {
            if (cooldownTimer <= 0f)
            {
                Fire();
                if(isShotgun)
                {                // For shotguns, we want to fire twice with a slight offset, so we call Fire() again with an offset
                    Fire(bulletOffsetValue); // Fire upper pellet
                    Fire(-bulletOffsetValue);  // Fire lower pellet
                }

                cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 1f);
            }
        }
    }

    private bool IsEnemyInLane()
    {
        // Use Raycast to detect enemies in the lane
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.up, detectionRange, enemyLayer);

        // For debugging, draw the ray in the scene view
        Debug.DrawRay(firePoint.position, firePoint.up * detectionRange, Color.green);

        return hit.collider != null;
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
        maxHealth = SoldierType.stats.health;
        currentHealth = maxHealth; // Initialize current health to max
        dmg = SoldierType.stats.dmg;
        isShootThrough = SoldierType.stats.isShootThrough;
        isShotgun = SoldierType.stats.isShotgun;
    }
    
    [NaughtyAttributes.Button("Test: Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }
    
    [NaughtyAttributes.Button("Test: Heal 10 Health")]
    private void TestHeal()
    {
        Heal(10f);
    }
    
    [NaughtyAttributes.Button("Apply Tier Changes")]
    private void ApplyTierChanges()
    {
        SoldierStats soldierModdedStats = SoldierType.GetStatsCopy();

        soldierModdedStats = SoldierTierList.tierDictionary[tier].tierChanges(soldierModdedStats);

        bulletSpeed = soldierModdedStats.bulletSpeed;
        bulletLife = soldierModdedStats.bulletLife;
        attackSpeed = soldierModdedStats.attackSpeed;
        maxHealth = soldierModdedStats.health;
        currentHealth = maxHealth; // Reset current health when applying tier changes
        dmg = soldierModdedStats.dmg;
    }

    private void Initialization()
    {
        SetSoldierType();
        ApplyTierChanges();
        cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 0f);
        
        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.Initialize(maxHealth);
        }
        else
        {
            Debug.LogWarning($"[SoldierBehaviour] No health bar assigned on '{gameObject.name}'");
        }
        
        Debug.Log($"[SoldierINIT] Initialized attackSpeed={attackSpeed}, legacy timer={timer}, cooldownTimer={cooldownTimer}, health={currentHealth}/{maxHealth} on '{gameObject.name}'");
    }
    public void Fire(float firePointOffset = 0f)
    {
        GameObject bullet = Instantiate(bulletPrefab, new Vector3(firePoint.position.x, firePoint.position.y + firePointOffset, firePoint.position.z), firePoint.rotation);

        // Set bullet damage
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.DamageAmount = dmg;
            bulletController.isShootThrough = isShootThrough;
        }
        else
        {
            Debug.LogWarning($"[SoldierBehaviour] Bullet prefab has no BulletController component on '{gameObject.name}'!");
        }

        // Apply velocity
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(firePoint.up * bulletSpeed, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning($"[SoldierBehaviour] Instantiated bullet has no Rigidbody2D on '{gameObject.name}'.");
        }

        // Draw a short debug ray showing firing direction in the Scene view
        Debug.DrawRay(firePoint.position, firePoint.up * 2f, Color.red, 0.5f);

        Destroy(bullet, bulletLife); // Destroy bullet after bulletLife seconds
    }

    /// <summary>
    /// Apply damage to the soldier
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return; // Already dead

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        Debug.Log($"[SoldierBehaviour] '{gameObject.name}' took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Check if dead
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the soldier
    /// </summary>
    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return; // Can't heal if dead

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        Debug.Log($"[SoldierBehaviour] '{gameObject.name}' healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Handle soldier death
    /// </summary>
    private void Die()
    {
        Debug.Log($"[SoldierBehaviour] '{gameObject.name}' has died!");
        
        // Optional: Play death animation or sound here
        
        // Destroy the soldier
        Destroy(gameObject);
    }

    /// <summary>
    /// Check if the soldier is alive
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
}

