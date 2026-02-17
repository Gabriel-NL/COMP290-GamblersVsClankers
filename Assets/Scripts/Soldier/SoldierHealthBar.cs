using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class SoldierHealthBar : MonoBehaviour
{
    [Header("References")]
    [MustBeAssigned] public SpriteRenderer healthCircle; // The circle sprite renderer
    [MustBeAssigned] public TMP_Text tierText;
    
    [Header("Color Gradient")]
    [Tooltip("Health percentage thresholds and their colors")]
    public Color greenColor = new Color(0f, 1f, 0f, 1f);      // 100-80%
    public Color yellowColor = new Color(1f, 1f, 0f, 1f);     // 80-60%
    public Color orangeColor = new Color(1f, 0.5f, 0f, 1f);   // 60-40%
    public Color redColor = new Color(1f, 0f, 0f, 1f);        // 40-20%
    public Color blackColor = new Color(0f, 0f, 0f, 1f);      // 20-0%

    private float maxHealth;
    private float currentHealth;

    public void Initialize(float health)
    {
        maxHealth = health;
        currentHealth = health;
        UpdateHealthBar();
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthBar();
    }

    public void SetTier(SoldierTierList.TierEnum tier)
    {
        if (tierText == null) return;

        int tierNumber = (int)tier + 1;

        tierText.text = $"T{tierNumber}";

        tierText.color = SoldierTierList.tierDictionary[tier].tierColor;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }

    private void UpdateHealthBar()
    {
        if (healthCircle == null) return;

        float healthPercent = GetHealthPercentage();
        
        // Update color based on health percentage
        healthCircle.color = GetColorForHealthPercentage(healthPercent);
        
        // Optional: Scale the circle based on health (visual feedback)
        // transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, healthPercent);
    }

    private Color GetColorForHealthPercentage(float percentage)
    {
        // Smooth gradient from Green -> Yellow -> Orange -> Red -> Black
        // Based on exact health percentage for smooth transitions
        
        // Clamp percentage to 0-1 range
        percentage = Mathf.Clamp01(percentage);
        
        // Define the gradient points (health percentage thresholds)
        // 100% = Green, 80% = Yellow, 60% = Orange, 40% = Red, 20% = Black, 0% = Black
        
        if (percentage >= 0.8f) // 100% to 80%: Green to Yellow
        {
            float t = (1f - percentage) / 0.2f; // 0 at 100%, 1 at 80%
            return Color.Lerp(greenColor, yellowColor, t);
        }
        else if (percentage >= 0.6f) // 80% to 60%: Yellow to Orange
        {
            float t = (0.8f - percentage) / 0.2f; // 0 at 80%, 1 at 60%
            return Color.Lerp(yellowColor, orangeColor, t);
        }
        else if (percentage >= 0.4f) // 60% to 40%: Orange to Red
        {
            float t = (0.6f - percentage) / 0.2f; // 0 at 60%, 1 at 40%
            return Color.Lerp(orangeColor, redColor, t);
        }
        else if (percentage >= 0.2f) // 40% to 20%: Red to Black
        {
            float t = (0.4f - percentage) / 0.2f; // 0 at 40%, 1 at 20%
            return Color.Lerp(redColor, blackColor, t);
        }
        else // 20% to 0%: Black
        {
            return blackColor;
        }
    }
}
