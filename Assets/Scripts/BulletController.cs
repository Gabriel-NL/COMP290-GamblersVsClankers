using UnityEngine;

public class BulletController : MonoBehaviour
{
    [HideInInspector]public float DamageAmount;
    public string audioName = "Shot";
    //public float bulletScale = 1f;

    private void Start()
    {
        AudioManager.Play(audioName);
        
        // Apply bullet scale
        // if (bulletScale != 1f)
        // {
        //     transform.localScale = Vector3.one * bulletScale;
        // }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     collision.gameObject.GetComponent<EnemyNavigation>().TakeDamage(DamageAmount);
        // }
        // else if (collision.gameObject.CompareTag("Player"))
        // {
        //     collision.gameObject.GetComponent<PlayerController>().TakeDamage(DamageAmount);
        // }
        Debug.Log($"[Bullet] '{gameObject.name}' collided with '{collision.gameObject.name}' and will be destroyed.");
        Destroy(this.gameObject);
    }
}
