using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed partial class HordeManager : MonoBehaviour
{

    [Header("Database")]
    [SerializeField] private HordePrefabDatabase prefabDatabase;
    [SerializeField] private TextMeshProUGUI hordeNumberText;

    [Header("Lane Position Logic")]
    private int numberOfLanes = 5;
    private float laneSpacing = -1.1225f;
    private float firstLaneOffset = 1.11f;
    private float spawnXPosition = 6.55f;
    private bool useLocalPosition = false;

    [Header("Render Layering")]
    [SerializeField] private int minimumEnemySortingOrder = 1;

    [Header("Debug / Read Only")]
    [SerializeField] private HordeState currentState = HordeState.Disabled;
    [SerializeField] private int currentHordeNumber = 0;

    private IReadOnlyList<PreMadeHorde> orderedHordes;
    private Dictionary<int, PreMadeHorde> dictionaryOfPreMadeHordes;

    private readonly HashSet<EnemyBehaviour> aliveEnemies = new HashSet<EnemyBehaviour>();

    private Coroutine hordeLoopCoroutine;
    private bool isValidated;

    public HordeState CurrentState => currentState;
    public int CurrentHordeNumber => currentHordeNumber;
    public int AliveEnemyCount => aliveEnemies.Count;

    private void Awake()
    {
        try
        {
            orderedHordes = HordeDefinitions.OrderedHordes;
            dictionaryOfPreMadeHordes = HordeDefinitions.BuildDictionary();

            ValidateConfigurationOrThrow();

            isValidated = true;
            currentState = HordeState.Disabled;
        }
        catch (Exception ex)
        {
            currentState = HordeState.Disabled;
            enabled = false;

            Debug.LogError($"[HordeManager] Validation failed. System disabled.\n{ex}", this);
            throw;
        }
    }

    private void Start()
    {
        if (!enabled || !isValidated)
        {
            return;
        }

        hordeLoopCoroutine = StartCoroutine(HordeLoopRoutine());
    }

    private void OnDisable()
    {
        if (hordeLoopCoroutine != null)
        {
            StopCoroutine(hordeLoopCoroutine);
            hordeLoopCoroutine = null;
        }
    }

    private IEnumerator HordeLoopRoutine()
    {
        int hordeIndex = 0;

        while (enabled)
        {
            currentHordeNumber = hordeIndex + 1;
            hordeNumberText.text = currentHordeNumber.ToString();

            yield return SpawnHordeRoutine(orderedHordes[hordeIndex]);

            currentState = HordeState.WaitingForAllEnemiesDead;
            Debug.Log($"[HordeManager] Horde {currentHordeNumber} spawned. Waiting for {aliveEnemies.Count} enemies to die...");
            
            yield return new WaitUntil(() => {
                if (aliveEnemies.Count == 0)
                    return true;
                return false;
            });
            
            Debug.Log($"[HordeManager] Horde {currentHordeNumber} complete! All enemies defeated.");

            currentState = HordeState.WaitingForNextHorde;

            bool isLastHorde = hordeIndex >= orderedHordes.Count - 1;
            float cooldown = isLastHorde
                ? HordeDefinitions.LastHordeCooldownSeconds
                : HordeDefinitions.InterHordeCooldownSeconds;

            if (cooldown > 0f)
            {
                yield return new WaitForSeconds(cooldown);
            }

            hordeIndex++;
            if (hordeIndex >= orderedHordes.Count)
            {
                hordeIndex = 0;
            }
        }
    }

    private IEnumerator SpawnHordeRoutine(PreMadeHorde horde)
    {
        currentState = HordeState.SpawningHorde;

        List<PreMadeSummon> summons = horde.summons;
        for (int i = 0; i < summons.Count; i++)
        {
            PreMadeSummon summon = summons[i];

            if (summon.relativeDelay > 0f)
            {
                yield return new WaitForSeconds(summon.relativeDelay);
            }

            TrySpawnSummonRuntimeSafe(summon);
        }
    }

    public void RemoveEnemy(EnemyBehaviour enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("[HordeManager] RemoveEnemy received null.", this);
            return;
        }

        bool removed = aliveEnemies.Remove(enemy);

        if (!removed)
        {
            Debug.LogWarning(
                $"[HordeManager] Enemy '{enemy.gameObject.name}' was not in alive enemies list! Already removed? Remaining alive: {aliveEnemies.Count}",
                this);
            return;
        }

        Debug.Log($"[HordeManager] Enemy '{enemy.gameObject.name}' removed. Alive enemies remaining: {aliveEnemies.Count}");

        if (currentState == HordeState.WaitingForAllEnemiesDead && aliveEnemies.Count == 0)
        {
            Debug.Log("[HordeManager] All enemies defeated! Horde complete.");
            // No direct action needed here because WaitUntil in coroutine will continue automatically.
        }
    }

    private void TrySpawnSummonRuntimeSafe(PreMadeSummon summon)
    {
        if (!IsLaneIndexValid(summon.laneIndex))
        {
            Debug.LogError(
                $"[HordeManager] Runtime invalid lane index {summon.laneIndex} in horde {currentHordeNumber}. Summon skipped.",
                this);
            return;
        }

        if (!TryResolvePrefab(summon.enemyPrefabIndex, out GameObject prefab))
        {
            Debug.LogError(
                $"[HordeManager] Runtime null/invalid prefab for prefab index {summon.enemyPrefabIndex} in horde {currentHordeNumber}. Summon skipped.",
                this);
            return;
        }

        SpawnEnemy(prefab, summon.laneIndex);
    }

    private void SpawnEnemy(GameObject prefab, int laneIndex)
    {
        Vector3 worldSpawnPosition = GetSpawnPosition(laneIndex);

        GameObject instance = Instantiate(prefab, worldSpawnPosition, prefab.transform.rotation);
        instance.SetActive(false);

        EnemyBehaviour enemyBehaviour = instance.GetComponent<EnemyBehaviour>();
        if (enemyBehaviour == null)
        {
            Debug.LogError(
                $"[HordeManager] Spawned object '{instance.name}' does not contain EnemyBehaviour on root. Destroying spawned object.",
                instance);

            Destroy(instance);
            return;
        }

        instance.transform.SetParent(transform, worldPositionStays: true);
        ApplyEnemySorting(instance);

        enemyBehaviour.SetHordeManager(this);

        bool added = aliveEnemies.Add(enemyBehaviour);

        // Debug.Log(!added
        //     ? $"[HordeManager] Duplicate EnemyBehaviour instance '{instance.name}' detected. This should not happen. Check if the prefab has multiple EnemyBehaviour components or if the same instance is being spawned multiple times."
        //     : $"[HordeManager] Enemy '{instance.name}' spawned successfully and registered. Total alive: {aliveEnemies.Count}", instance);

        instance.SetActive(true);
    }

    private Vector3 GetSpawnPosition(int laneIndex)
    {
        float spawnY = firstLaneOffset + (laneIndex * laneSpacing);
        
        // Fine-tune bottom lane
        if (laneIndex == 4)
        {
            spawnY -= 0.1f;
        }
        
        if (useLocalPosition)
        {
            return transform.TransformPoint(new Vector3(spawnXPosition, spawnY, 0f));
        }
        else
        {
            return new Vector3(spawnXPosition, spawnY, 0f);
        }
    }

    private void ApplyEnemySorting(GameObject enemyInstance)
    {
        if (enemyInstance == null)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers = enemyInstance.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer == null)
            {
                continue;
            }

            if (spriteRenderer.sortingOrder < minimumEnemySortingOrder)
            {
                spriteRenderer.sortingOrder = minimumEnemySortingOrder;
            }
        }
    }

    private void ValidateConfigurationOrThrow()
    {
        if (prefabDatabase == null)
        {
            throw new InvalidOperationException("[HordeManager] Prefab database is not assigned.");
        }

        if (prefabDatabase.enemyPrefabs == null)
        {
            throw new InvalidOperationException("[HordeManager] Prefab database enemyPrefabs array is null.");
        }

        if (numberOfLanes <= 0)
        {
            throw new InvalidOperationException("[HordeManager] numberOfLanes must be greater than zero.");
        }

        if (orderedHordes == null || orderedHordes.Count == 0)
        {
            throw new InvalidOperationException("[HordeManager] No hordes exist. At least one horde is required.");
        }

        for (int hordeIndex = 0; hordeIndex < orderedHordes.Count; hordeIndex++)
        {
            ValidateSingleHordeOrThrow(orderedHordes[hordeIndex], hordeIndex + 1);
        }
    }

    private void ValidateSingleHordeOrThrow(PreMadeHorde horde, int hordeNumber)
    {
        if (horde == null)
        {
            throw new InvalidOperationException($"[HordeManager] Horde {hordeNumber} is null.");
        }

        if (horde.summons == null || horde.summons.Count == 0)
        {
            throw new InvalidOperationException($"[HordeManager] Horde {hordeNumber} is empty.");
        }

        int validSummons = 0;

        for (int summonIndex = 0; summonIndex < horde.summons.Count; summonIndex++)
        {
            PreMadeSummon summon = horde.summons[horde.summons.Count > summonIndex ? summonIndex : 0];
        }

        for (int summonIndex = 0; summonIndex < horde.summons.Count; summonIndex++)
        {
            PreMadeSummon summon = horde.summons[summonIndex];

            if (summon == null)
            {
                continue;
            }

            if (summon.relativeDelay < 0f)
            {
                throw new InvalidOperationException(
                    $"[HordeManager] Horde {hordeNumber}, summon {summonIndex}: negative relativeDelay is forbidden.");
            }

            if (!IsLaneIndexValid(summon.laneIndex))
            {
                throw new InvalidOperationException(
                    $"[HordeManager] Horde {hordeNumber}, summon {summonIndex}: invalid laneIndex {summon.laneIndex}. Valid range: 0 to {numberOfLanes - 1}.");
            }

            if (!IsPrefabIndexWithinArrayBounds(summon.enemyPrefabIndex))
            {
                continue;
            }

            GameObject prefab = prefabDatabase.enemyPrefabs[summon.enemyPrefabIndex];
            if (prefab == null)
            {
                continue;
            }

            EnemyBehaviour enemyBehaviour = prefab.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour == null)
            {
                continue;
            }

            validSummons++;
        }

        if (validSummons <= 0)
        {
            throw new InvalidOperationException(
                $"[HordeManager] Horde {hordeNumber} has no valid summons after validation.");
        }
    }

    private bool IsLaneIndexValid(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < numberOfLanes;
    }

    private bool IsPrefabIndexWithinArrayBounds(int prefabIndex)
    {
        return prefabIndex >= 0 && prefabIndex < prefabDatabase.enemyPrefabs.Length;
    }

    private bool TryResolvePrefab(int prefabIndex, out GameObject prefab)
    {
        prefab = null;

        if (!IsPrefabIndexWithinArrayBounds(prefabIndex))
        {
            return false;
        }

        prefab = prefabDatabase.enemyPrefabs[prefabIndex];
        if (prefab == null)
        {
            return false;
        }

        if (prefab.GetComponent<EnemyBehaviour>() == null)
        {
            return false;
        }

        return true;
    }
}