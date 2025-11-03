using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of enemy prefabs to spawn randomly")]
    public GameObject[] enemyPrefabs;
    
    [Tooltip("Number of lanes (default 5)")]
    public int numberOfLanes = 5;
    
    [Tooltip("Distance between lanes")]
    public float laneSpacing = 1f;
    
    [Tooltip("Offset for the first lane (can be negative to center)")]
    public float firstLaneOffset = -2f;
    
    [Header("Spawn Timing")]
    [Tooltip("Minimum time between spawns on the same lane")]
    public float minSpawnCooldown = 2f;
    
    [Tooltip("Maximum time between spawns on the same lane")]
    public float maxSpawnCooldown = 5f;
    
    [Tooltip("Enable/disable spawning")]
    public bool isSpawning = true;
    
    [Header("Spawn Position")]
    [Tooltip("X position where enemies spawn (typically off-screen right)")]
    public float spawnXPosition = 10f;
    
    [Tooltip("Use local position relative to this transform")]
    public bool useLocalPosition = true;

    private float[] laneCooldowns;
    private Coroutine[] laneCoroutines;

    void Start()
    {
        InitializeSpawner();
    }

    private void InitializeSpawner()
    {
        // Initialize cooldown timers for each lane
        laneCooldowns = new float[numberOfLanes];
        laneCoroutines = new Coroutine[numberOfLanes];
        
        // Start spawning coroutine for each lane
        for (int i = 0; i < numberOfLanes; i++)
        {
            laneCoroutines[i] = StartCoroutine(SpawnOnLane(i));
        }
        
        Debug.Log($"[EnemySpawner] Initialized with {numberOfLanes} lanes. Spawning started.");
    }

    private IEnumerator SpawnOnLane(int laneIndex)
    {
        // Random initial delay so lanes don't all spawn at once
        yield return new WaitForSeconds(Random.Range(0f, maxSpawnCooldown));
        
        while (true)
        {
            if (isSpawning && enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                SpawnEnemy(laneIndex);
                
                // Wait for cooldown before next spawn
                float cooldown = Random.Range(minSpawnCooldown, maxSpawnCooldown);
                yield return new WaitForSeconds(cooldown);
            }
            else
            {
                // If spawning is disabled, check again after a short delay
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void SpawnEnemy(int laneIndex)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemy prefabs assigned!");
            return;
        }
        
        // Get random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawner] Enemy prefab at index is null!");
            return;
        }
        
        // Calculate spawn position for this lane
        Vector3 spawnPosition = GetLanePosition(laneIndex);
        
        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Optional: Set parent for organization
        if (enemy != null)
        {
            enemy.transform.SetParent(transform);
        }
        
        // Debug.Log($"[EnemySpawner] Spawned {enemyPrefab.name} on lane {laneIndex} at position {spawnPosition}");
    }

    private Vector3 GetLanePosition(int laneIndex)
    {
        // Calculate Y position based on lane index
        float yPosition = firstLaneOffset + (laneIndex * laneSpacing);
        
        Vector3 position = new Vector3(spawnXPosition, yPosition, 0f);
        
        // Convert to world position if using local
        if (useLocalPosition)
        {
            position = transform.TransformPoint(position);
        }
        
        return position;
    }

    /// <summary>
    /// Start spawning enemies
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("[EnemySpawner] Spawning enabled");
    }

    /// <summary>
    /// Stop spawning enemies (existing enemies remain)
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("[EnemySpawner] Spawning disabled");
    }

    /// <summary>
    /// Stop all spawning coroutines (use when destroying spawner)
    /// </summary>
    public void StopAllSpawning()
    {
        isSpawning = false;
        
        if (laneCoroutines != null)
        {
            for (int i = 0; i < laneCoroutines.Length; i++)
            {
                if (laneCoroutines[i] != null)
                {
                    StopCoroutine(laneCoroutines[i]);
                }
            }
        }
        
        Debug.Log("[EnemySpawner] All spawning stopped");
    }

    // Visualize lanes in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        for (int i = 0; i < numberOfLanes; i++)
        {
            Vector3 lanePos = GetLanePosition(i);
            
            // Draw a small sphere at spawn point
            Gizmos.DrawWireSphere(lanePos, 0.3f);
            
            // Draw a line showing the lane
            Vector3 lineStart = lanePos - new Vector3(2f, 0f, 0f);
            Vector3 lineEnd = lanePos;
            Gizmos.DrawLine(lineStart, lineEnd);
        }
    }

    void OnDestroy()
    {
        StopAllSpawning();
    }
}
