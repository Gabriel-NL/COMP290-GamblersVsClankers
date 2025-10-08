using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Player Summon List", menuName = "ScriptableObjects/Player Summon List")]
public class PlayerSummonsList : ScriptableObject
{
    [SerializeField] public PlayerSummon[] playerSummons;

    // Quick count
    public int Count => playerSummons != null ? playerSummons.Length : 0;

    // Index-safe access
    public PlayerSummon GetAt(int index)
    {
        if (playerSummons == null || index < 0 || index >= playerSummons.Length) return null;
        return playerSummons[index];
    }

    // Find by name (case-insensitive)
    public PlayerSummon GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || playerSummons == null) return null;
        return playerSummons.FirstOrDefault(s =>
            !string.IsNullOrEmpty(s.summonName) &&
            s.summonName.Trim().ToLowerInvariant() == name.Trim().ToLowerInvariant());
    }

    public bool TryGetByName(string name, out PlayerSummon summon)
    {
        summon = GetByName(name);
        return summon != null;
    }

    // Filter by max cost
    public List<PlayerSummon> GetAffordable(int currentPoints)
    {
        if (playerSummons == null) return new List<PlayerSummon>();
        return playerSummons.Where(s => s != null && s.summonCost >= 0 && s.summonCost <= currentPoints).ToList();
    }

    // Return all entries that look valid (have prefab & sprite)
    public List<PlayerSummon> GetValid()
    {
        if (playerSummons == null) return new List<PlayerSummon>();
        return playerSummons.Where(IsValid).ToList();
    }

    // Basic validity check
    public static bool IsValid(PlayerSummon s)
    {
        if (s == null) return false;
        if (string.IsNullOrWhiteSpace(s.summonName)) return false;
        if (s.summonCost < 0) return false;
        if (s.summonPrefab == null) return false;
        return true;
    }

#if UNITY_EDITOR
    // Editor-side validation: clamp costs, warn duplicates
    private void OnValidate()
    {
        if (playerSummons == null) return;

        var seen = new HashSet<string>();
        for (int i = 0; i < playerSummons.Length; i++)
        {
            var s = playerSummons[i];
            if (s == null) continue;

            if (s.summonCost < 0) s.summonCost = 0;

            // Normalize name for duplicate detection
            var key = string.IsNullOrWhiteSpace(s.summonName) ? "" : s.summonName.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(key))
            {
                if (!seen.Add(key))
                {
                    Debug.LogWarning($"[PlayerSummonsList] Duplicate summon name detected: \"{s.summonName}\" at index {i}. Names should be unique.");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerSummonsList] Empty summon name at index {i}.");
            }
        }
    }
#endif
}

[System.Serializable] // <-- Make the nested data class serializable for the inspector
public class PlayerSummon
{
    public string summonName;
    public Sprite summonSprite;
    [Min(0)] public int summonCost;
    public GameObject summonPrefab;
}
