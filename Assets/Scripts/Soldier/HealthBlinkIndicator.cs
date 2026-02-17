using System.Collections;
using UnityEngine;

public class HealthBlinkIndicator : MonoBehaviour
{
    [Header("Blink Colors")]
    public Color blinkColor = Color.red;

    [Header("Initial Blink Settings")]
    public float initialBlinkDuration = 0.2f;
    public float initialBlinkCooldown = 0.2f;

    [Header("Scaling Settings")]
    public bool enableScalingStartHealth = true;

    [Range(1, 100)]
    public int minimumPercentageOfHealth = 50;

    [Header("Hit Reaction Blink (Fast Blink)")]
    public float hitBlinkDuration = 1f;
    public float hitBlinkSpeed = 10f;

    private float maxHealth;
    private float currentHealth;

    private float scalableStartHealth;

    private float currentBlinkDuration;
    private float currentBlinkCooldown;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Coroutine blinkCoroutine;

    private bool isInitialized = false;
    private bool isDead = false;

    // -----------------------------
    // Initialization
    // -----------------------------
    public void Initialize(float health, SpriteRenderer rendererToBlink)
    {
        if (rendererToBlink == null)
        {
            Debug.LogError($"[{nameof(HealthBlinkIndicator)}] Initialize failed: SpriteRenderer is null.");
            return;
        }

        spriteRenderer = rendererToBlink;

        maxHealth = health;
        currentHealth = health;

        originalColor = spriteRenderer.color;

        if (enableScalingStartHealth)
            scalableStartHealth = maxHealth * (minimumPercentageOfHealth / 100f);
        else
            scalableStartHealth = maxHealth;

        currentBlinkDuration = initialBlinkDuration;
        currentBlinkCooldown = initialBlinkCooldown;

        isInitialized = true;
        isDead = false;
    }

    // -----------------------------
    // External Trigger
    // -----------------------------
    public void SetHealth(float newHealth)
    {
        if (!isInitialized || isDead)
            return;

        currentHealth = newHealth;

        if (currentHealth <= 0f)
        {
            KillBlinking();
            return;
        }

        if (currentHealth >= scalableStartHealth)
        {
            StopBlinkingInstant();
            return;
        }


        UpdateBlinkFrequency();
        RestartBlinking();
    }

    // -----------------------------
    // Frequency Logic
    // -----------------------------
    private void UpdateBlinkFrequency()
    {
        if (scalableStartHealth <= 0f)
        {
            currentBlinkDuration = initialBlinkDuration;
            currentBlinkCooldown = initialBlinkCooldown;
            return;
        }

        float multiplier = Mathf.Clamp(currentHealth / scalableStartHealth, 0.1f, 1f);

        currentBlinkDuration = initialBlinkDuration * multiplier;
        currentBlinkCooldown = initialBlinkCooldown * multiplier;
    }

    // -----------------------------
    // Coroutine Control
    // -----------------------------
    private void RestartBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;

            spriteRenderer.color = originalColor;
        }

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlinkingInstant()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        spriteRenderer.color = originalColor;
    }

    private void KillBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        spriteRenderer.color = originalColor;

        isDead = true;
        enabled = false;
    }

    // -----------------------------
    // Main Routine
    // -----------------------------
    private IEnumerator BlinkRoutine()
    {
        yield return StartCoroutine(HitBlinkRoutine());

        while (currentHealth > 0f && currentHealth < scalableStartHealth)
        {
            yield return StartCoroutine(SmoothBlinkOnce(currentBlinkDuration));
            yield return new WaitForSeconds(currentBlinkCooldown);
        }

        spriteRenderer.color = originalColor;
        blinkCoroutine = null;
    }

    // -----------------------------
    // Fast Blink
    // -----------------------------
    private IEnumerator HitBlinkRoutine()
    {
        float timer = 0f;

        while (timer < hitBlinkDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.PingPong(Time.time * hitBlinkSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, blinkColor, t);

            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    // -----------------------------
    // Smooth Blink Cycle
    // -----------------------------
    private IEnumerator SmoothBlinkOnce(float duration)
    {
        if (duration <= 0.01f)
            duration = 0.01f;

        float half = duration * 0.5f;

        float timer = 0f;

        // Original -> Blink
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;

            spriteRenderer.color = Color.Lerp(originalColor, blinkColor, t);
            yield return null;
        }

        timer = 0f;

        // Blink -> Original
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;

            spriteRenderer.color = Color.Lerp(blinkColor, originalColor, t);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }
}
