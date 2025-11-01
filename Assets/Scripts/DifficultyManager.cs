using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages progressive difficulty scaling based on player progress
/// Increases enemy stats gradually as soldiers are placed and enemies are killed
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager instance;
    
    [Header("Difficulty Scaling Settings")]
    [Tooltip("Health multiplier increase per progression point (very small, e.g., 0.01 = 1% per point)")]
    public float healthScalingPerPoint = 0.015f; // 1.5% health increase per point
    
    [Tooltip("Damage multiplier increase per progression point")]
    public float damageScalingPerPoint = 0.01f; // 1% damage increase per point
    
    [Tooltip("Attack cooldown reduction per progression point (0.01 = 1% faster)")]
    public float cooldownReductionPerPoint = 0.008f; // 0.8% faster attacks per point
    
    [Tooltip("Minimum attack cooldown (prevents enemies from attacking too fast)")]
    public float minAttackCooldown = 0.3f;
    
    [Header("Progression Tracking")]
    [Tooltip("How many progression points per soldier placed")]
    public float pointsPerSoldierPlaced = 1f;
    
    [Tooltip("How many progression points per enemy killed")]
    public float pointsPerEnemyKilled = 0.5f;
    
    [Header("Current Difficulty (Read-Only)")]
    [SerializeField][ReadOnlyInInspector] private float totalProgressionPoints = 0f;
    [SerializeField][ReadOnlyInInspector] private int soldiersPlaced = 0;
    [SerializeField][ReadOnlyInInspector] private int enemiesKilled = 0;
    [SerializeField][ReadOnlyInInspector] private float currentHealthMultiplier = 1f;
    [SerializeField][ReadOnlyInInspector] private float currentDamageMultiplier = 1f;
    [SerializeField][ReadOnlyInInspector] private float currentCooldownMultiplier = 1f;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        UpdateMultipliers();
        Debug.Log("[DifficultyManager] Initialized. Starting difficulty: 1.0x");
    }
    
    /// <summary>
    /// Call this when a soldier is placed on the battlefield
    /// </summary>
    public void OnSoldierPlaced()
    {
        soldiersPlaced++;
        totalProgressionPoints += pointsPerSoldierPlaced;
        UpdateMultipliers();
        
        Debug.Log($"[DifficultyManager] Soldier placed. Total: {soldiersPlaced}, Points: {totalProgressionPoints:F2}, HP: {currentHealthMultiplier:F3}x, DMG: {currentDamageMultiplier:F3}x, CD: {currentCooldownMultiplier:F3}x");
    }
    
    /// <summary>
    /// Call this when an enemy is killed
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        totalProgressionPoints += pointsPerEnemyKilled;
        UpdateMultipliers();
        
        // Only log every 5 kills to avoid spam
        if (enemiesKilled % 5 == 0)
        {
            Debug.Log($"[DifficultyManager] {enemiesKilled} enemies killed. Points: {totalProgressionPoints:F2}, HP: {currentHealthMultiplier:F3}x, DMG: {currentDamageMultiplier:F3}x, CD: {currentCooldownMultiplier:F3}x");
        }
    }
    
    /// <summary>
    /// Recalculate all multipliers based on current progression
    /// </summary>
    private void UpdateMultipliers()
    {
        // Linear scaling with progression points
        currentHealthMultiplier = 1f + (totalProgressionPoints * healthScalingPerPoint);
        currentDamageMultiplier = 1f + (totalProgressionPoints * damageScalingPerPoint);
        
        // Cooldown reduction (faster attacks = lower cooldown)
        currentCooldownMultiplier = 1f - (totalProgressionPoints * cooldownReductionPerPoint);
        currentCooldownMultiplier = Mathf.Max(currentCooldownMultiplier, 0.2f); // Cap at 80% reduction (5x faster)
    }
    
    /// <summary>
    /// Apply difficulty scaling to base health value
    /// </summary>
    public float GetScaledHealth(float baseHealth)
    {
        return baseHealth * currentHealthMultiplier;
    }
    
    /// <summary>
    /// Apply difficulty scaling to base damage value
    /// </summary>
    public float GetScaledDamage(float baseDamage)
    {
        return baseDamage * currentDamageMultiplier;
    }
    
    /// <summary>
    /// Apply difficulty scaling to base attack cooldown
    /// </summary>
    public float GetScaledCooldown(float baseCooldown)
    {
        float scaledCooldown = baseCooldown * currentCooldownMultiplier;
        return Mathf.Max(scaledCooldown, minAttackCooldown);
    }
    
    /// <summary>
    /// Get current progression points
    /// </summary>
    public float GetProgressionPoints()
    {
        return totalProgressionPoints;
    }
    
    /// <summary>
    /// Get current difficulty as a simple multiplier (average of all multipliers)
    /// </summary>
    public float GetDifficultyLevel()
    {
        return (currentHealthMultiplier + currentDamageMultiplier + (2f - currentCooldownMultiplier)) / 3f;
    }
    
    /// <summary>
    /// Reset difficulty to starting values (for game restart)
    /// </summary>
    public void ResetDifficulty()
    {
        totalProgressionPoints = 0f;
        soldiersPlaced = 0;
        enemiesKilled = 0;
        UpdateMultipliers();
        Debug.Log("[DifficultyManager] Difficulty reset to 1.0x");
    }
}
