using System.Collections.Generic;
using UnityEngine;

public class EnemyWalking : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Auto Destroy")]
    [Tooltip("X position where enemy gets destroyed (typically off-screen left)")]
    public float destroyAtX = -12f;

    [Tooltip("Enable auto-destroy when off screen")]
    public bool autoDestroy = true;

    private readonly HashSet<GameObject> blockingSoldiers = new HashSet<GameObject>();
    private int externalStopRequests = 0;

    private bool IsMovementBlocked => externalStopRequests > 0 || blockingSoldiers.Count > 0;

    void Start()
    {
        EnemyBehaviour enemyBehaviour = GetComponent<EnemyBehaviour>();
        if (enemyBehaviour != null)
        {
            moveSpeed = enemyBehaviour.speed;
        }
    }

    void Update()
    {
        if (transform == null)
            return;

        if (!IsMovementBlocked)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }

        if (autoDestroy && transform.position.x < destroyAtX)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Soldier"))
            return;

        if (blockingSoldiers.Add(collision.gameObject))
        {
            Debug.Log($"{gameObject.name} stopped by {collision.gameObject.name}. Blockers={blockingSoldiers.Count}");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Soldier"))
            return;

        if (blockingSoldiers.Remove(collision.gameObject))
        {
            if (IsMovementBlocked)
            {
                Debug.Log($"{gameObject.name} still blocked. Remaining blockers={blockingSoldiers.Count}, externalStops={externalStopRequests}");
            }
            else
            {
                Debug.Log($"{gameObject.name} resumed walking");
            }
        }
    }

    public void Stop()
    {
        externalStopRequests++;
        Debug.Log($"{gameObject.name} Stop() called. externalStops={externalStopRequests}");
    }

    public void Resume()
    {
        externalStopRequests = Mathf.Max(0, externalStopRequests - 1);

        if (IsMovementBlocked)
        {
            Debug.Log($"{gameObject.name} Resume() called but still blocked. blockers={blockingSoldiers.Count}, externalStops={externalStopRequests}");
        }
        else
        {
            Debug.Log($"{gameObject.name} resumed walking");
        }
    }

    private void OnDestroy()
    {
        blockingSoldiers.Clear();
        StopAllCoroutines();
    }
}