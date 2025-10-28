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
    [ReadOnly] public float health;
    [ReadOnly] public float dmg;
    [ReadOnly] public float reward;

    [Header("Timer")]
    public float timer;
    private float cooldownTimer;

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
    [NaughtyAttributes.Button("Apply Tier Changes")]

    private void Initialization()
    {
        SetEnemyType();
        cooldownTimer = (speed > 0f) ? speed : ((timer > 0f) ? timer : 0f);
        Debug.Log($"[SoldierINIT] Initialized attackSpeed={speed}, legacy timer={timer}, cooldownTimer={cooldownTimer} on '{gameObject.name}'");
    }
}
