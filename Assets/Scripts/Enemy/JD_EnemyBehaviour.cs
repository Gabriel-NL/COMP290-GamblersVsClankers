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
        }
        jitterSeed = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        // Enemy movement logic would go here
        ApplyJitter();
    }

    private void ApplyJitter()
    {
        if (spriteRenderer == null) return;

        // Use Perlin noise for smooth, natural movement
        float t = Time.time * jitterFrequency + jitterSeed;
        float nx = Mathf.PerlinNoise(t, 0f) - 0.5f;
        float ny = Mathf.PerlinNoise(0f, t) - 0.5f;

        Vector3 offset = new Vector3(nx * 2f * jitterAmplitude, ny * 2f * jitterAmplitude, 0f);
        spriteRenderer.transform.localPosition = spriteOriginalLocalPos + offset;

        // subtle rotation jitter
        float nr = (Mathf.PerlinNoise(t + 37.1f, t + 12.3f) - 0.5f) * 2f;
        float angle = nr * jitterRotationAmplitude;
        spriteRenderer.transform.localRotation = spriteOriginalLocalRot * Quaternion.Euler(0f, 0f, angle);
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
        Debug.Log($"[SoldierINIT] Initialized attackSpeed={speed}, legacy timer={timer}, cooldownTimer={cooldownTimer} on '{gameObject.name}'");
    }
}
