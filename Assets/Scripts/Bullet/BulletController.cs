using UnityEngine;

public class BulletController : MonoBehaviour
{
    [HideInInspector]public float DamageAmount;
    public string audioName = "Shot";
    public bool isShootThrough = false;

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
        Debug.Log($"[Bullet] '{gameObject.name}' collided with '{collision.gameObject.name}' (Tag: {collision.gameObject.tag})");
        
        // Check if hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBehaviour enemy = collision.gameObject.GetComponent<EnemyBehaviour>();
            if (enemy != null)
            {
                enemy.TakeDamage(DamageAmount);
                Debug.Log($"[Bullet] Applied {DamageAmount} damage to enemy '{collision.gameObject.name}'");
            }
            else
            {
                Debug.LogWarning($"[Bullet] Enemy '{collision.gameObject.name}' has no EnemyBehaviour component!");
            }
        }
        // Check if hit a soldier (friendly fire or enemy bullets)
        else if (collision.gameObject.CompareTag("Soldier"))
        {
            SoldierBehaviour soldier = collision.gameObject.GetComponent<SoldierBehaviour>();
            if (soldier != null)
            {
                soldier.TakeDamage(DamageAmount);
                Debug.Log($"[Bullet] Applied {DamageAmount} damage to soldier '{collision.gameObject.name}'");
            }
        }
        
        if (!isShootThrough)
        {
            Debug.Log($"[Bullet] Bullet is not shoot-through, destroying on collision.");
            Destroy(this.gameObject);
        }
        // Destroy bullet after collision
    }
}
