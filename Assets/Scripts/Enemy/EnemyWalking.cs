using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalking : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    private bool isStopped = false;
    
    [Header("Auto Destroy")]
    [Tooltip("X position where enemy gets destroyed (typically off-screen left)")]
    public float destroyAtX = -12f;
    
    [Tooltip("Enable auto-destroy when off screen")]
    public bool autoDestroy = true;

    void Update()
    {
        if (transform == null) return; // Safety check
        
        if (!isStopped)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
        
        // Auto-destroy if off screen
        if (autoDestroy && transform.position.x < destroyAtX)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Soldier"))
        {
            isStopped = true;
            Debug.Log($"{gameObject.name} stopped by {collision.gameObject.name}");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Soldier"))
        {
            isStopped = false;
            Debug.Log($"{gameObject.name} resumed walking");
        }
    }

    public void Stop()
    {
        isStopped = true;
    }

    public void Resume()
    {
        isStopped = false;
    }
    
    void OnDestroy()
    {
        // Clean up - prevents inspector errors when destroyed
        StopAllCoroutines();
    }
}
