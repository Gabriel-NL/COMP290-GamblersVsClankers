using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    public EnemyType enemyType;
    [MustBeAssigned] public SpriteRenderer spriteRenderer;

    //[HideInInspector] public string shootAudioName;

    //public GameObject healthBar;
    [Header("Enemy Stats (Read-Only)")]
    [ReadOnly] public float speed;
    [ReadOnly] public float maxHealth;
    [ReadOnly] public float currentHealth;
    [ReadOnly] public float dmg;
    [ReadOnly] public float reward;

    [Header("Combat")]
    [Tooltip("Time between attacks in seconds")]
    public float attackCooldown = 1f;
    private float attackTimer = 0f;
    private SoldierBehaviour targetSoldier; // Current soldier being attacked
    
    [Header("Attack Animation")]
    [Tooltip("How far forward the sprite lunges during attack")]
    public float attackLungeDistance = 0.3f;
    [Tooltip("How fast the sprite lunges forward")]
    public float attackLungeSpeed = 10f;
    
    private Vector3 spriteRestPosition; // Original local position of sprite
    private bool isLunging = false;
    private bool isReturning = false;
    
    [Header("Timer")]
    public float timer;
    private float cooldownTimer;
    
    [Header("Jitter Animation")]
    [Tooltip("How far (units) the sprite can jitter from its resting position")]
    public float jitterAmplitude = 0.05f;
    [Tooltip("Speed multiplier for jitter movement")]
    public float jitterFrequency = 8f; // increased to make jitter much faster
    [Tooltip("Subtle rotation applied along with position jitter")]
    public float jitterRotationAmplitude = 2f;

    // internal state for smooth Perlin noise based jitter
    private Vector3 spriteOriginalLocalPos;
    private Quaternion spriteOriginalLocalRot;
    private float jitterSeed;

    void Start()
    {
        Initialization();
        // capture the sprite's original local transform so jitter is additive
        if (spriteRenderer != null)
        {
            spriteOriginalLocalPos = spriteRenderer.transform.localPosition;
            spriteOriginalLocalRot = spriteRenderer.transform.localRotation;
            spriteRestPosition = spriteRenderer.transform.localPosition;
        }
        jitterSeed = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        // Enemy movement logic would go here
        //ApplyJitter();
        
        // Handle attack animation
        UpdateAttackAnimation();
        
        // Handle attacking
        if (targetSoldier != null)
        {
            // Check if target is still alive
            if (!targetSoldier.IsAlive())
            {
                targetSoldier = null;
                return;
            }
            
            // Attack cooldown
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                AttackSoldier(targetSoldier);
                attackTimer = attackCooldown;
            }
        }
    }
    
    private void UpdateAttackAnimation()
    {
        if (spriteRenderer == null) return;
        
        Vector3 targetPosition = spriteRestPosition;
        
        if (isLunging)
        {
            // Lunge forward (move left for enemies moving left)
            targetPosition = spriteRestPosition + new Vector3(-attackLungeDistance, 0f, 0f);
            spriteRenderer.transform.localPosition = Vector3.Lerp(
                spriteRenderer.transform.localPosition,
                targetPosition,
                attackLungeSpeed * Time.deltaTime
            );
            
            // Check if lunge is complete
            if (Vector3.Distance(spriteRenderer.transform.localPosition, targetPosition) < 0.01f)
            {
                isLunging = false;
                isReturning = true;
            }
        }
        else if (isReturning)
        {
            // Return to rest position
            spriteRenderer.transform.localPosition = Vector3.Lerp(
                spriteRenderer.transform.localPosition,
                spriteRestPosition,
                attackLungeSpeed * Time.deltaTime
            );
            
            // Check if return is complete
            if (Vector3.Distance(spriteRenderer.transform.localPosition, spriteRestPosition) < 0.01f)
            {
                spriteRenderer.transform.localPosition = spriteRestPosition;
                isReturning = false;
            }
        }
    }

    // private void ApplyJitter()
    // {
    //     if (spriteRenderer == null) return;

    //     // Use Perlin noise for smooth, natural movement
    //     float t = Time.time * jitterFrequency + jitterSeed;
    //     float nx = Mathf.PerlinNoise(t, 0f) - 0.5f;
    //     float ny = Mathf.PerlinNoise(0f, t) - 0.5f;

    //     Vector3 offset = new Vector3(nx * 2f * jitterAmplitude, ny * 2f * jitterAmplitude, 0f);
    //     spriteRenderer.transform.localPosition = spriteOriginalLocalPos + offset;

    //     // subtle rotation jitter
    //     float nr = (Mathf.PerlinNoise(t + 37.1f, t + 12.3f) - 0.5f) * 2f;
    //     float angle = nr * jitterRotationAmplitude;
    //     spriteRenderer.transform.localRotation = spriteOriginalLocalRot * Quaternion.Euler(0f, 0f, angle);
    // }

    [NaughtyAttributes.Button("Import data from EnemyType SO")]
    private void SetEnemyType()
    {
        //gameObject.name += " - " + enemyType.name;
        //gameObject.name = enemyType.name;
        spriteRenderer.sprite = enemyType.characterSprite;
        speed = enemyType.stats.speed;
        
        // Apply difficulty scaling if DifficultyManager exists
        float baseHealth = enemyType.stats.health;
        float baseDamage = enemyType.stats.dmg;
        float baseAttackCooldown = attackCooldown;
        
        if (DifficultyManager.instance != null)
        {
            maxHealth = DifficultyManager.instance.GetScaledHealth(baseHealth);
            dmg = DifficultyManager.instance.GetScaledDamage(baseDamage);
            attackCooldown = DifficultyManager.instance.GetScaledCooldown(baseAttackCooldown);
        }
        else
        {
            maxHealth = baseHealth;
            dmg = baseDamage;
        }
        
        currentHealth = maxHealth; // Initialize current health to max
        reward = enemyType.stats.reward;
    }
    
    [NaughtyAttributes.Button("Test: Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }
    
    private void Initialization()
    {
        SetEnemyType();
        cooldownTimer = (speed > 0f) ? speed : ((timer > 0f) ? timer : 0f);
        Debug.Log($"[EnemyBehaviour] Initialized speed={speed}, health={currentHealth}/{maxHealth}, dmg={dmg}, reward={reward} on '{gameObject.name}'");
    }
    
    /// <summary>
    /// Apply damage to the enemy
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f) return; // Already dead

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Check if dead
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void Die()
    {
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' has died! Rewarding {reward} coins.");
        
        // Award coins to player
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddPoints((int)reward);
        }
        
        // Notify difficulty manager
        if (DifficultyManager.instance != null)
        {
            DifficultyManager.instance.OnEnemyKilled();
        }
        
        // Optional: Play death animation or sound here
        
        // Destroy the enemy
        Destroy(gameObject);
    }

    /// <summary>
    /// Check if the enemy is alive
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
    
    /// <summary>
    /// Attack a soldier
    /// </summary>
    private void AttackSoldier(SoldierBehaviour soldier)
    {
        if (soldier == null || !soldier.IsAlive()) return;
        
        // Trigger attack animation
        if (!isLunging && !isReturning)
        {
            isLunging = true;
        }
        
        soldier.TakeDamage(dmg);
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' attacked '{soldier.gameObject.name}' for {dmg} damage");
    }
    
    /// <summary>
    /// Called when enemy collides with a soldier
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Soldier"))
        {
            SoldierBehaviour soldier = collision.gameObject.GetComponent<SoldierBehaviour>();
            if (soldier != null && soldier.IsAlive())
            {
                targetSoldier = soldier;
                attackTimer = 0f; // Attack immediately on first contact
                Debug.Log($"[EnemyBehaviour] '{gameObject.name}' started attacking '{soldier.gameObject.name}'");
            }
        }
    }
    
    /// <summary>
    /// Called when enemy stops colliding with a soldier
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Soldier"))
        {
            SoldierBehaviour soldier = collision.gameObject.GetComponent<SoldierBehaviour>();
            if (soldier != null && soldier == targetSoldier)
            {
                targetSoldier = null;
                attackTimer = 0f;
                Debug.Log($"[EnemyBehaviour] '{gameObject.name}' stopped attacking '{soldier.gameObject.name}'");
            }
        }
    }
}
