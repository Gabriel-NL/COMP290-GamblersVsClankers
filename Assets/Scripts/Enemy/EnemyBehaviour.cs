using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class JD_EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    public EnemyType enemyType;
    [MustBeAssigned] public SpriteRenderer spriteRenderer;

    //[HideInInspector] public string shootAudioName;

    //public GameObject healthBar;
    [Header("Enemy Stats (Read-Only)")]
    [ReadOnly] public float speed;
    [ReadOnly] public float health;
    [ReadOnly] public float dmg;
    [ReadOnly] public float reward;

    [Header("Timer")]
    public float timer;
    private float cooldownTimer;

    private EnemyWalking enemyWalking;

    void Start()
    {
        Initialization();
    }

    private void Update()
    {
        // Enemy movement logic would go here
    }

    [NaughtyAttributes.Button("Import data from EnemyType SO")]
    private void SetEnemyType()
    {
        gameObject.name += " - " + enemyType.name;
        gameObject.name = enemyType.name;
        spriteRenderer.sprite = enemyType.characterSprite;
        speed = enemyType.stats.speed;
        health = enemyType.stats.health;
        dmg = enemyType.stats.dmg;
    }
    
    private void Initialization()
    {
        SetEnemyType();
        cooldownTimer = (speed > 0f) ? speed : ((timer > 0f) ? timer : 0f);
        Debug.Log($"[EnemyBehaviour] Initialized attackSpeed={speed}, legacy timer={timer}, cooldownTimer={cooldownTimer} on '{gameObject.name}'");
        
        // Get or add EnemyWalking component
        enemyWalking = GetComponent<EnemyWalking>();
        if (enemyWalking == null)
        {
            enemyWalking = gameObject.AddComponent<EnemyWalking>();
            Debug.Log($"[EnemyBehaviour] Added EnemyWalking component to '{gameObject.name}'");
        }
        
        // Set the movement speed from EnemyType stats
        enemyWalking.moveSpeed = speed;
        Debug.Log($"[EnemyBehaviour] Set EnemyWalking moveSpeed to {speed} on '{gameObject.name}'");
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        // Add reward logic here if needed
        Destroy(gameObject);
    }
}
