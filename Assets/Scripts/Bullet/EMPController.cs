using System.Collections;
using UnityEngine;

public class EMPController: MonoBehaviour
{
    [HideInInspector] public float stunDuration = 3f;
    [HideInInspector] public float aoeRadius = 3f;
    [HideInInspector] public LayerMask enemyLayer;

    private void Start()
    {
        // Instantly detonate on spawn
        Detonate();
    }

    private void Detonate()
    {
        Debug.Log($"[EMPController] EMP detonating at position {transform.position} with radius {aoeRadius}, stunDuration={stunDuration}, enemyLayer={enemyLayer.value}");

        // Find all enemies in radius - try without layer mask first to debug
        Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        Debug.Log($"[EMPController] Found {allColliders.Length} total colliders in radius");

        // Find all enemies in radius with layer mask
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyLayer);

        Debug.Log($"[EMPController] Found {hitColliders.Length} enemies in EMP radius with layer mask");

        foreach (Collider2D hitCollider in hitColliders)
        {
            Debug.Log($"[EMPController] Checking collider: '{hitCollider.gameObject.name}' on layer {LayerMask.LayerToName(hitCollider.gameObject.layer)}");

            EnemyBehaviour enemy = hitCollider.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                enemy.Stun(stunDuration);
                Debug.Log($"[EMPController] Successfully stunned enemy '{hitCollider.gameObject.name}' for {stunDuration} seconds");
            }
            else
            {
                Debug.LogWarning($"[EMPController] Collider '{hitCollider.gameObject.name}' has no EnemyBehaviour component!");
            }
        }

        // Destroy the EMP object immediately
        Destroy(gameObject);
    }

    // Draw the EMP radius in the editor for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
