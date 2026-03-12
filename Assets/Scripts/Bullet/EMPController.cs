using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPController: MonoBehaviour
{
    [HideInInspector] public float stunDuration = 3f;
    [HideInInspector] public float damageAmount;
    [HideInInspector] public float aoeRadius = 3f;
    [HideInInspector] public LayerMask enemyLayer;
    [SerializeField] private string audioName = "EMP";
    [SerializeField] private GameObject empEffectPrefab;
    [SerializeField] private float effectLifetime = 1f;

    private void Start()
    {
        // Instantly detonate on spawn
        Detonate();
    }

    private void Detonate()
    {
        Debug.Log($"[EMPController] Area effect detonating at position {transform.position} with radius {aoeRadius}, stunDuration={stunDuration}, damageAmount={damageAmount}, enemyLayer={enemyLayer.value}");

        SpawnEffect();

        // Find all enemies in radius - try without layer mask first to debug
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        Debug.Log($"[EMPController] Found {allColliders.Length} total colliders in radius");

        // Find all enemies in radius with layer mask
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyLayer);

        Debug.Log($"[EMPController] Found {hitColliders.Length} enemies in EMP radius with layer mask");

        HashSet<EnemyBehaviour> affectedEnemies = new HashSet<EnemyBehaviour>();

        foreach (Collider2D hitCollider in hitColliders)
        {
            Debug.Log($"[EMPController] Checking collider: '{hitCollider.gameObject.name}' on layer {LayerMask.LayerToName(hitCollider.gameObject.layer)}");

            EnemyBehaviour enemy = ResolveEnemyBehaviour(hitCollider);
            if (enemy == null)
            {
                Debug.LogWarning($"[EMPController] Collider '{hitCollider.gameObject.name}' has no EnemyBehaviour component!");
                continue;
            }

            if (!affectedEnemies.Add(enemy))
                continue; // already hit this enemy via another collider

            if (damageAmount > 0f)
            {
                enemy.TakeDamage(damageAmount);
                Debug.Log($"[EMPController] Applied {damageAmount} area damage to enemy '{enemy.gameObject.name}'");
            }

            if (stunDuration > 0f)
            {
                enemy.Stun(stunDuration);
                Debug.Log($"[EMPController] Successfully stunned enemy '{enemy.gameObject.name}' for {stunDuration} seconds");
            }
        }

        // Destroy the EMP object immediately
        Destroy(gameObject);
    }

    private void SpawnEffect()
    {
        // if (!string.IsNullOrEmpty(audioName))
        // {
        //     AudioManager.Play(audioName);
        // }

        if (empEffectPrefab == null)
        {
            Debug.LogWarning($"[EMPController] No EMP effect prefab assigned on '{gameObject.name}'.");
            return;
        }

        GameObject effectInstance = Instantiate(empEffectPrefab, transform.position, Quaternion.identity);
        if (effectLifetime > 0f)
        {
            Destroy(effectInstance, effectLifetime);
        }
    }

    /// <summary>
    /// Walks up the hierarchy to find EnemyBehaviour in case the hit collider belongs to a child object.
    /// </summary>
    private EnemyBehaviour ResolveEnemyBehaviour(Collider2D col)
    {
        EnemyBehaviour eb = col.GetComponent<EnemyBehaviour>();
        if (eb != null) return eb;

        eb = col.GetComponentInParent<EnemyBehaviour>();
        if (eb != null) return eb;

        if (col.attachedRigidbody != null)
            eb = col.attachedRigidbody.GetComponent<EnemyBehaviour>();

        return eb;
    }

    // Draw the EMP radius in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
