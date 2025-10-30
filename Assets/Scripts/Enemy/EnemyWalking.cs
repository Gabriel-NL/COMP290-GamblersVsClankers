using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalking : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isStopped = false;

    void Update()
    {
        if (!isStopped)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
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
}
