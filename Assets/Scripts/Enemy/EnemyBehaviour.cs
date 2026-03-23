using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("References")]
    public EnemyType enemyType;
    [MustBeAssigned] public SpriteRenderer spriteRenderer;

    [Header("Enemy Stats (Read-Only)")]
    [ReadOnly] public float speed;
    [ReadOnly] public float maxHealth;
    [ReadOnly] public float currentHealth;
    [ReadOnly] public float dmg;
    [ReadOnly] public float reward;
    [ReadOnly] public bool isFlying;
    [ReadOnly] public bool isRCCar;
    [ReadOnly] public bool isCybertruck;
    [ReadOnly] public bool isStunned = false;

    [Header("Combat")]
    [Tooltip("Current time between attacks in seconds")]
    [ReadOnly] public float attackCooldown = 1f;

    private float attackTimer = 0f;
    private SoldierBehaviour targetSoldier;

    [Header("Cybertruck Explosion")]
    [Tooltip("Prefab to spawn for explosion effect (should have EMPController)")]
    public GameObject bulletPrefab;
    [Tooltip("Radius of the explosion area of effect")]
    public float aoeRadius = 5f;
    [Tooltip("Layer mask for enemies to damage")]
    public LayerMask enemyLayer;
    [Tooltip("Duration of the explosion animation in seconds")]
    public float explosionAnimationDuration = 0.5f;
    [Tooltip("Max scale reached during explosion animation")]
    public float explosionMaxScale = 1.5f;

    [Header("Attack Animation")]
    [Tooltip("How far forward the sprite lunges during attack")]
    public float attackLungeDistance = 0.3f;
    [Tooltip("How fast the sprite lunges forward")]
    public float attackLungeSpeed = 10f;

    private Vector3 spriteRestPosition;
    private bool isLunging = false;
    private bool isReturning = false;

    [Header("Jitter Animation")]
    [Tooltip("How far (units) the sprite can jitter from its resting position")]
    public float jitterAmplitude = 0.05f;
    [Tooltip("Speed multiplier for jitter movement")]
    public float jitterFrequency = 8f;
    [Tooltip("Subtle rotation applied along with position jitter")]
    public float jitterRotationAmplitude = 2f;

    private Vector3 spriteOriginalLocalPos;
    private Quaternion spriteOriginalLocalRot;
    private float jitterSeed;

    private readonly List<SoldierBehaviour> overlappingSoldiers = new List<SoldierBehaviour>();

    private Coroutine stunCoroutine;
    private float stunEndTime = -1f;
    private Color preStunColor = Color.white;

    private HordeManager hordeManager;
    private bool hasReportedDeathToHordeManager;

    void Start()
    {
        Initialization();

        if (spriteRenderer != null)
        {
            spriteOriginalLocalPos = spriteRenderer.transform.localPosition;
            spriteOriginalLocalRot = spriteRenderer.transform.localRotation;
            spriteRestPosition = spriteRenderer.transform.localPosition;
            preStunColor = spriteRenderer.color;
        }

        jitterSeed = Random.Range(0f, 1000f);
    }

    public void SetHordeManager(HordeManager manager)
    {
        hordeManager = manager;
    }

    void Update()
    {
        UpdateAttackAnimation();
        CleanupOverlappingSoldiers();
        RefreshTargetSoldier();

        if (targetSoldier != null)
        {
            if (isStunned)
                return;

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                AttackSoldier(targetSoldier);
                attackTimer = attackCooldown;
            }
        }
    }

    private void DetonateBomb()
    {
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' is a Cybertruck, detonating");

        // Start the explosion sequence (animation + damage)
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        // Disable collision and movement during explosion
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        EnemyWalking walking = GetComponent<EnemyWalking>();
        if (walking != null)
        {
            walking.Stop();
        }

        Vector3 originalScale = spriteRenderer != null ? spriteRenderer.transform.localScale : Vector3.one;
        Vector3 targetScale = originalScale * explosionMaxScale;
        float elapsedTime = 0f;

        // Play explosion animation (scale up then down)
        while (elapsedTime < explosionAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / explosionAnimationDuration;
            
            // Scale up then down smoothly
            float scaleProgress = Mathf.Sin(progress * Mathf.PI);
            Vector3 currentScale = Vector3.Lerp(originalScale, targetScale, scaleProgress);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = currentScale;
            }

            yield return null;
        }

        // Reset scale
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = originalScale;
        }

        // Spawn the explosion effect (visual feedback)
        if (bulletPrefab != null)
        {
            GameObject cybertruckBombEffect = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            EMPController areaEffectController = cybertruckBombEffect.GetComponent<EMPController>();
            
            if (areaEffectController != null)
            {
                // Configure the effect as pure damage (no stun)
                areaEffectController.stunDuration = 0f;
                areaEffectController.damageAmount = dmg;
                areaEffectController.aoeRadius = aoeRadius;
                areaEffectController.enemyLayer = enemyLayer;
                Debug.Log($"[EnemyBehaviour] Cybertruck explosion spawned: damage={dmg}, aoeRadius={aoeRadius}");
            }
            else
            {
                Debug.LogWarning($"[EnemyBehaviour] Cybertruck bomb effect has no EMPController on '{gameObject.name}'");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyBehaviour] Cybertruck '{gameObject.name}' has no bulletPrefab assigned. No visual effect spawned.");
        }

        // Apply direct damage to all enemies in AOE radius using Physics overlap
        ApplyExplosionDamage();

        // Destroy the cybertruck after the animation
        Destroy(gameObject);
    }

    private void ApplyExplosionDamage()
    {
        // Find all soldiers in the scene and check distance
        SoldierBehaviour[] allSoldiers = FindObjectsByType<SoldierBehaviour>(FindObjectsSortMode.None);
        int damagedCount = 0;

        foreach (SoldierBehaviour soldier in allSoldiers)
        {
            // Calculate distance to this soldier
            float distance = Vector3.Distance(transform.position, soldier.transform.position);
            
            if (distance <= aoeRadius)
            {
                soldier.TakeDamage(dmg);
                damagedCount++;
                Debug.Log($"[EnemyBehaviour] Cybertruck explosion dealt {dmg} damage to soldier '{soldier.gameObject.name}' (distance: {distance:F2})");
            }
        }

        Debug.Log($"[EnemyBehaviour] Cybertruck explosion affected {damagedCount} soldier(s) within radius {aoeRadius}");
    }

    private void UpdateAttackAnimation()
    {
        if (spriteRenderer == null)
            return;

        Vector3 targetPosition = spriteRestPosition;

        if (isLunging)
        {
            targetPosition = spriteRestPosition + new Vector3(-attackLungeDistance, 0f, 0f);
            spriteRenderer.transform.localPosition = Vector3.Lerp(
                spriteRenderer.transform.localPosition,
                targetPosition,
                attackLungeSpeed * Time.deltaTime
            );

            if (Vector3.Distance(spriteRenderer.transform.localPosition, targetPosition) < 0.01f)
            {
                isLunging = false;
                isReturning = true;
            }
        }
        else if (isReturning)
        {
            spriteRenderer.transform.localPosition = Vector3.Lerp(
                spriteRenderer.transform.localPosition,
                spriteRestPosition,
                attackLungeSpeed * Time.deltaTime
            );

            if (Vector3.Distance(spriteRenderer.transform.localPosition, spriteRestPosition) < 0.01f)
            {
                spriteRenderer.transform.localPosition = spriteRestPosition;
                isReturning = false;
            }
        }
    }

    [Button("Import data from EnemyType SO")]
    private void SetEnemyType()
    {
        if (enemyType == null)
        {
            Debug.LogWarning($"[EnemyBehaviour] '{gameObject.name}' has no EnemyType assigned.");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[EnemyBehaviour] '{gameObject.name}' has no SpriteRenderer assigned.");
            return;
        }

        spriteRenderer.sprite = enemyType.characterSprite;

        speed = enemyType.stats.speed;
        maxHealth = enemyType.stats.health;
        currentHealth = maxHealth;
        dmg = enemyType.stats.dmg;
        attackCooldown = enemyType.stats.attackCooldown;
        reward = enemyType.stats.reward;
        isFlying = enemyType.stats.isFlying;
        isRCCar = enemyType.stats.isRCCar;
        isCybertruck = enemyType.stats.isCybertruck;
    }

    // [Button("Test: Take 10 Damage")]
    // private void TestTakeDamage()
    // {
    //     TakeDamage(10f);
    // }

    private void Initialization()
    {
        SetEnemyType();

        Debug.Log(
            $"[EnemyBehaviour] Initialized speed={speed}, health={currentHealth}/{maxHealth}, dmg={dmg}, reward={reward}, atkCd={attackCooldown} on '{gameObject.name}'"
        );
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die(true, true);
        }
    }

    private void Die(bool rewardPlayer, bool countAsKill)
    {
        if (hasReportedDeathToHordeManager)
        {
            Debug.LogWarning($"[EnemyBehaviour] '{gameObject.name}' death was already processed.");
            return;
        }

        hasReportedDeathToHordeManager = true;

        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' has died! rewardPlayer={rewardPlayer}, countAsKill={countAsKill}");

        if (rewardPlayer && ScoreManager.instance != null)
        {
            ScoreManager.instance.AddPoints((int)reward);
        }

        // countAsKill foi mantido para compatibilidade com sua assinatura atual,
        // mas a progressão de horda agora depende do HordeManager, não do DifficultyManager.
        if (countAsKill)
        {
            // Intencionalmente vazio.
        }

        if (hordeManager != null)
        {
            hordeManager.RemoveEnemy(this);
        }
        else
        {
            Debug.LogWarning($"[EnemyBehaviour] '{gameObject.name}' died without HordeManager reference.");
        }

        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    public void Stun(float duration)
    {
        if (!IsAlive())
            return;

        stunEndTime = Time.time + duration;

        if (stunCoroutine == null)
        {
            stunCoroutine = StartCoroutine(StunCoroutine());
        }
    }

    private IEnumerator StunCoroutine()
    {
        isStunned = true;
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' is stunned until t={stunEndTime:F2}");

        EnemyWalking walking = GetComponent<EnemyWalking>();
        if (walking != null)
        {
            walking.Stop();
        }

        if (spriteRenderer != null)
        {
            preStunColor = spriteRenderer.color;
            spriteRenderer.color = Color.cyan;
        }

        while (Time.time < stunEndTime)
        {
            yield return null;
        }

        isStunned = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = preStunColor;
        }

        if (walking != null)
        {
            walking.Resume();
        }

        stunCoroutine = null;
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' stun ended");
    }

    private void AttackSoldier(SoldierBehaviour soldier)
    {
        if (soldier == null || !soldier.IsAlive())
            return;

        if (!isLunging && !isReturning)
        {
            isLunging = true;
        }

        soldier.TakeDamage(dmg);
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' attacked '{soldier.gameObject.name}' for {dmg} damage");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Soldier"))
            return;

        SoldierBehaviour soldier = collision.gameObject.GetComponent<SoldierBehaviour>();
        if (soldier == null || !soldier.IsAlive())
            return;

        if (isRCCar)
        {
            Debug.Log($"[EnemyBehaviour] RC Car '{gameObject.name}' collided with '{collision.gameObject.name}'. Detonating.");

            soldier.TakeDamage(99999f);

            // Continua sem reward e sem kill credit normal,
            // mas agora ainda remove da horda porque o próprio Die()
            // sempre notifica o HordeManager.
            Die(false, false);
            return;
        }

        if(isCybertruck)
        {
            Debug.Log($"[EnemyBehaviour] Cybertruck '{gameObject.name}' collided with '{collision.gameObject.name}'. Detonating.");

            DetonateBomb();
            return;
        }

        AddOverlappingSoldier(soldier);
        RefreshTargetSoldier();
        attackTimer = 0f;

        if (targetSoldier != null)
        {
            Debug.Log($"[EnemyBehaviour] '{gameObject.name}' started attacking '{targetSoldier.gameObject.name}'");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Soldier"))
            return;

        SoldierBehaviour soldier = collision.gameObject.GetComponent<SoldierBehaviour>();
        if (soldier == null)
            return;

        RemoveOverlappingSoldier(soldier);
        RefreshTargetSoldier();

        if (targetSoldier == null)
        {
            attackTimer = 0f;
            Debug.Log($"[EnemyBehaviour] '{gameObject.name}' has no current soldier target");
        }
        else
        {
            Debug.Log($"[EnemyBehaviour] '{gameObject.name}' switched target to '{targetSoldier.gameObject.name}'");
        }
    }

    private void AddOverlappingSoldier(SoldierBehaviour soldier)
    {
        if (soldier == null)
            return;

        if (!overlappingSoldiers.Contains(soldier))
        {
            overlappingSoldiers.Add(soldier);
        }
    }

    private void RemoveOverlappingSoldier(SoldierBehaviour soldier)
    {
        if (soldier == null)
            return;

        overlappingSoldiers.Remove(soldier);
    }

    private void CleanupOverlappingSoldiers()
    {
        for (int i = overlappingSoldiers.Count - 1; i >= 0; i--)
        {
            SoldierBehaviour soldier = overlappingSoldiers[i];
            if (soldier == null || !soldier.IsAlive())
            {
                overlappingSoldiers.RemoveAt(i);
            }
        }
    }

    private void RefreshTargetSoldier()
    {
        SoldierBehaviour bestTarget = null;
        float bestX = float.NegativeInfinity;

        for (int i = 0; i < overlappingSoldiers.Count; i++)
        {
            SoldierBehaviour soldier = overlappingSoldiers[i];
            if (soldier == null || !soldier.IsAlive())
                continue;

            float x = soldier.transform.position.x;
            if (bestTarget == null || x > bestX)
            {
                bestTarget = soldier;
                bestX = x;
            }
        }

        targetSoldier = bestTarget;
    }

    private void OnDestroy()
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }
    }
}