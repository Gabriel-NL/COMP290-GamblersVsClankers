using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public int lives = 3; //number of enemies that can enter the casino before player loses
    [Header("Audio")]
    [Tooltip("Sound to play when an enemy crosses the line")]
    public string crossingClipname;

    // Update is called once per frame
    void Update()
    {
        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        lives = 0;
        Debug.Log("Game Over");
        //Insert logic to display game over screen or restart level
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[DeathZone] OnCollisionEnter2D triggered by '{collision.gameObject.name}' with tag '{collision.gameObject.tag}'");
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[DeathZone] Enemy '{collision.gameObject.name}' entered death zone. Lives before: {lives}");

            // Play the crossing sound once per enemy. Some enemies may have multiple colliders
            // and trigger this method multiple times before being destroyed. To avoid duplicate
            // playback we attach a lightweight marker component to the enemy and only play the
            // sound the first time we see that marker.
            if (collision.gameObject.GetComponent<DeathZoneCrossedMarker>() == null)
            {
                // Add marker so subsequent trigger calls for the same object won't replay
                collision.gameObject.AddComponent<DeathZoneCrossedMarker>();

                if (!string.IsNullOrEmpty(crossingClipname))
                {
                    AudioManager.Play(crossingClipname);
                }
            }

            lives--;
            Debug.Log($"[DeathZone] Lives after: {lives}");
            Destroy(collision.gameObject);
        }
    }

    // Lightweight marker used to indicate an enemy already triggered the death zone
    private class DeathZoneCrossedMarker : MonoBehaviour { }
}
