using Unity.VisualScripting;
using UnityEngine;

public class SoldierINIT : MonoBehaviour
{
    public SoldierType SoldierType;
    public SoldierTier SoldierTier;
    public Transform firePoint;
    public GameObject bulletPrefab;
    //public GameObject healthBar;

    [HideInInspector] public float bulletSpeed;
    [HideInInspector] public float bulletLife;
    [HideInInspector] public float attackSpeed;
    [HideInInspector] public float health;
    [HideInInspector] public float dmg;
    // legacy public timer kept for inspector visibility if needed, but firing uses attackSpeed
    public float timer;
    private float cooldownTimer;
    //[HideInInspector] public string shootAudioName;



    void Start()
    {
        SetSoldierType();
        // Initialize cooldown. Prefer attackSpeed, fallback to legacy 'timer'. If both are <= 0,
        // set cooldownTimer = 0 so first Frame will fire immediately (and we'll use a safe fallback
        // when resetting the cooldown after firing).
        cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 0f);

        Debug.Log($"[SoldierINIT] Initialized attackSpeed={attackSpeed}, legacy timer={timer}, cooldownTimer={cooldownTimer} on '{gameObject.name}'");
    }
    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        if (cooldownTimer <= 0f)
        {
            Fire();
            // Reset cooldownTimer using attackSpeed if valid, else fallback to legacy timer, else default to 1s
            cooldownTimer = (attackSpeed > 0f) ? attackSpeed : ((timer > 0f) ? timer : 1f);
            Debug.Log($"[SoldierINIT] Fired from '{gameObject.name}'. Next shot in {cooldownTimer} seconds.");
        }
    }
    public void Fire()
    {
        if (firePoint == null)
        {
            Debug.LogWarning($"[SoldierINIT] firePoint is null on '{gameObject.name}'. Cannot Fire().");
            return;
        }
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[SoldierINIT] bulletPrefab is null on '{gameObject.name}'. Cannot Fire().");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(firePoint.up * bulletSpeed, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning($"[SoldierINIT] Instantiated bullet has no Rigidbody2D on '{gameObject.name}'.");
        }
        // Log creation details to help debug why bullets might not be visible
        Debug.Log($"[SoldierINIT] Instantiated bullet at {firePoint.position} with life={bulletLife} on '{gameObject.name}'");
        // Draw a short debug ray showing firing direction in the Scene view
        Debug.DrawRay(firePoint.position, firePoint.up * 2f, Color.red, 0.5f);

        Destroy(bullet, bulletLife); // Destroy bullet after bulletLife seconds
    }
    private void SetSoldierType()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gameObject.name = SoldierType.name;

        switch (SoldierType.type)
        {
            case SoldierType.TypeOfSoldier.Pistol:
                // Pistol specific settings
                bulletSpeed = 10f;
                bulletLife = 2f;
                attackSpeed = 2.8f;
                health = 50f;
                dmg = 10f;
                break;
            case SoldierType.TypeOfSoldier.Rifleman:
                // Rifleman specific settings
                bulletSpeed = 20f;
                bulletLife = 2f;
                attackSpeed = 2f;
                health = 75f;
                dmg = 35f;
                break;
            case SoldierType.TypeOfSoldier.ARSoldier:
                // ARSoldier specific settings
                bulletSpeed = 30f;
                bulletLife = 2f;
                attackSpeed = 1f;
                health = 100f;
                dmg = 25f;
                break;
            case SoldierType.TypeOfSoldier.LaserMan:
                // LaserMan specific settings
                bulletSpeed = 35f;
                bulletLife = 2f;
                attackSpeed = 0.6f;
                health = 150f;
                dmg = 15f;
                break;
        }
        spriteRenderer.sprite = SoldierType.characterSprite;
        //firePoint.GetComponent<SpriteRenderer>().sprite = SoldierType.weaponSprite;
        //shootAudioName = SoldierType.shootAudioName;

        Invoke("SetSoldierTier", 2f);
    }

    private void SetSoldierTier()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        gameObject.name += " - " + SoldierTier.name;

        switch (SoldierTier.tier)
        {
            case SoldierTier.TierOfSoldier.common:
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // white
                break;
            case SoldierTier.TierOfSoldier.uncommon:
                bulletSpeed += 5f;
                attackSpeed -= 0.2f;
                health += 5f;
                dmg += 5f;
                spriteRenderer.color = new Color(1f, 1f, 0.5f); // yellow
                break;
            case SoldierTier.TierOfSoldier.Rare:
                bulletSpeed += 7f;
                attackSpeed -= 0.3f;
                health += 10f;
                dmg += 7f;
                spriteRenderer.color = new Color(0.5f, 0.5f, 1f); // Light blue
                break;
            case SoldierTier.TierOfSoldier.SuperRare:
                bulletSpeed += 8f;
                attackSpeed -= 0.4f;
                health += 15f;
                dmg += 10f;
                spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // purple
                break;
            case SoldierTier.TierOfSoldier.Ultra:
                bulletSpeed += 10f;
                attackSpeed -= 0.6f;
                health += 30f;
                dmg += 20f;
                spriteRenderer.color = new Color(1f, 0.84f, 0f); // gold
                break;
        }

        
        // healthBar.GetComponent<Slider>().maxValue = health;
        // healthBar.GetComponent<Slider>().value = health;
    }
   
}