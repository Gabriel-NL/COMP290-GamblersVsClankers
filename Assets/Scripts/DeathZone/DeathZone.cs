using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public int lives = 3; //number of enemies that can enter the casino before player loses
    [SerializeField] GameObject livesDisplay;
    [SerializeField] GameObject gameOverScreen;
    [Header("Audio")]
    [Tooltip("Sound to play when an enemy crosses the line")]
    public string crossingClipname;


    public void GameOver()
    {
        AudioManager.StopAllSounds();
        lives = 0;
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over");

        
        AudioManager.Play("Death");
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
            //Update the draw mode width of the to display only the remaining lives
            livesDisplay.GetComponent<SpriteRenderer>().size -= new Vector2(15.8f, 0);
            
            Debug.Log($"[DeathZone] Lives after: {lives}");
            Destroy(collision.gameObject);
            if (lives <= 0)
            {
                GameOver();
            }


        }
    }

    // Lightweight marker used to indicate an enemy already triggered the death zone
    private class DeathZoneCrossedMarker : MonoBehaviour { }
}
