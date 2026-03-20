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
    [ReadOnly] public bool isStunned = false;

    [Header("Combat")]
    [Tooltip("Current scaled time between attacks in seconds")]
    [ReadOnly] public float attackCooldown = 1f;

    private float attackTimer = 0f;
    private SoldierBehaviour targetSoldier;

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

        spriteRenderer.sprite = enemyType.characterSprite;
        speed = enemyType.stats.speed;

        float baseHealth = enemyType.stats.health;
        float baseDamage = enemyType.stats.dmg;
        float baseAttackCooldown = enemyType.stats.attackCooldown;

        if (Application.isPlaying && DifficultyManager.instance != null)
        {
            maxHealth = DifficultyManager.instance.GetScaledHealth(baseHealth);
            dmg = DifficultyManager.instance.GetScaledDamage(baseDamage);
            attackCooldown = DifficultyManager.instance.GetScaledCooldown(baseAttackCooldown);
        }
        else
        {
            maxHealth = baseHealth;
            dmg = baseDamage;
            attackCooldown = baseAttackCooldown;
        }

        currentHealth = maxHealth;
        reward = enemyType.stats.reward;
        isFlying = enemyType.stats.isFlying;
        isRCCar = enemyType.stats.isRCCar;
    }

    [Button("Test: Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }

    private void Initialization()
    {
        SetEnemyType();
        Debug.Log($"[EnemyBehaviour] Initialized speed={speed}, health={currentHealth}/{maxHealth}, dmg={dmg}, reward={reward}, atkCd={attackCooldown} on '{gameObject.name}'");
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
        Debug.Log($"[EnemyBehaviour] '{gameObject.name}' has died! rewardPlayer={rewardPlayer}, countAsKill={countAsKill}");

        if (rewardPlayer && ScoreManager.instance != null)
        {
            ScoreManager.instance.AddPoints((int)reward);
        }

        if (countAsKill && DifficultyManager.instance != null)
        {
            DifficultyManager.instance.OnEnemyKilled();
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

            // No reward and no kill progression if RC car explodes on collision kill.
            Die(false, false);
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