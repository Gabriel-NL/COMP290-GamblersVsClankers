using UnityEngine;

public partial class DeathZone : MonoBehaviour
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
        AudioManager.Play("Death");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log($"[DeathZone] OnCollisionEnter2D triggered by '{collision.gameObject.name}' with tag '{collision.gameObject.tag}'");
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Ignore duplicate collision callbacks for the same enemy.
            if (collision.gameObject.GetComponent<DeathZoneCrossedMarker>() != null)
            {
                return;
            }

            // Mark the enemy so multi-collider enemies only process once.
            collision.gameObject.AddComponent<DeathZoneCrossedMarker>();

            // Report the enemy to the horde system before destroying it.
            EnemyBehaviour enemyBehaviour = collision.gameObject.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null)
            {
                enemyBehaviour.ReportDeathToHordeManager();
            }
            else
            {
                Debug.LogWarning($"[DeathZone] Enemy '{collision.gameObject.name}' has no EnemyBehaviour component.");
            }

            if (!string.IsNullOrEmpty(crossingClipname))
            {
                AudioManager.Play(crossingClipname);
            }

            lives--;

            livesDisplay.GetComponent<SpriteRenderer>().size -= new Vector2(15.8f, 0);
            
            //Debug.Log($"[DeathZone] Lives after: {lives}");
            Destroy(collision.gameObject);
            if (lives <= 0)
            {
                GameOver();
            }
        }
    }
}
