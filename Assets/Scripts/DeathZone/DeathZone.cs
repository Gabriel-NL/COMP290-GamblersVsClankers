using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public int lives = 3; //number of enemies that can enter the casino before player loses
    [Header("Audio")]
    [Tooltip("Sound to play when an enemy crosses the line")]
    public AudioClip crossingClip;

    // audio source used to play the crossing sound
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        // Ensure we have an AudioSource to play the clip
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (lives <= 0)
        {
            Debug.Log("Game Over");
            // Implement game over logic here
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"[DeathZone] Object '{collision.gameObject.name}' entered death zone.");

            // Play the crossing sound once per enemy. Some enemies may have multiple colliders
            // and trigger this method multiple times before being destroyed. To avoid duplicate
            // playback we attach a lightweight marker component to the enemy and only play the
            // sound the first time we see that marker.
            if (collision.gameObject.GetComponent<DeathZoneCrossedMarker>() == null)
            {
                // Add marker so subsequent trigger calls for the same object won't replay
                collision.gameObject.AddComponent<DeathZoneCrossedMarker>();

                if (crossingClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(crossingClip);
                }
            }

            lives--;
            Destroy(collision.gameObject);
        }
    }

        // Lightweight marker used to indicate an enemy already triggered the death zone
        private class DeathZoneCrossedMarker : MonoBehaviour { }
}
