using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SoldierEnchantController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SoldierBehaviour soldierBehaviour;

    [Header("Enchant Toggle")]
    [SerializeField] private bool enchantEnabled = true;
    [SerializeField] private bool autoUseRarityColor = true;
    [SerializeField] private bool autoDisableForCommon = true;

    [Header("Enchant Settings")]
    [SerializeField] private Color enchantColor = new Color(0.4f, 0.8f, 1f, 0.35f);
    [SerializeField] private float waveSpeed = 1.0f;
    [SerializeField] private float waveFrequency = 8.0f;
    [SerializeField] private float waveWidth = 0.2f;
    [SerializeField] private float waveStrength = 0.5f;
    [SerializeField] private float diagonalAmount = 1.0f;

    private MaterialPropertyBlock block;

    private static readonly int EnchantEnabledID = Shader.PropertyToID("_EnchantEnabled");
    private static readonly int EnchantColorID = Shader.PropertyToID("_EnchantColor");
    private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
    private static readonly int WaveFrequencyID = Shader.PropertyToID("_WaveFrequency");
    private static readonly int WaveWidthID = Shader.PropertyToID("_WaveWidth");
    private static readonly int WaveStrengthID = Shader.PropertyToID("_WaveStrength");
    private static readonly int DiagonalAmountID = Shader.PropertyToID("_DiagonalAmount");

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        soldierBehaviour = GetComponent<SoldierBehaviour>();
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (soldierBehaviour == null)
        {
            soldierBehaviour = GetComponent<SoldierBehaviour>();
        }

        block = new MaterialPropertyBlock();
        RefreshFromRarity();
        ApplyProperties();
    }

    private void Start()
    {
        RefreshFromRarity();
        ApplyProperties();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (soldierBehaviour == null)
        {
            soldierBehaviour = GetComponent<SoldierBehaviour>();
        }

        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        RefreshFromRarity();
        ApplyProperties();
    }

    public void RefreshFromRarity()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (autoUseRarityColor)
        {
            Color rarityColor = spriteRenderer.color;
            rarityColor.a = Mathf.Clamp01(enchantColor.a);
            enchantColor = rarityColor;
        }

        if (autoDisableForCommon && soldierBehaviour != null)
        {
            enchantEnabled = soldierBehaviour.tier != SoldierTierList.TierEnum.Common;
        }
    }

    public void ApplyProperties()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.GetPropertyBlock(block);

        block.SetFloat(EnchantEnabledID, enchantEnabled ? 1f : 0f);
        block.SetColor(EnchantColorID, enchantColor);
        block.SetFloat(WaveSpeedID, waveSpeed);
        block.SetFloat(WaveFrequencyID, waveFrequency);
        block.SetFloat(WaveWidthID, waveWidth);
        block.SetFloat(WaveStrengthID, waveStrength);
        block.SetFloat(DiagonalAmountID, diagonalAmount);

        spriteRenderer.SetPropertyBlock(block);
    }

    public void SetEnchantEnabled(bool enabled)
    {
        enchantEnabled = enabled;
        ApplyProperties();
    }

    public void ToggleEnchant()
    {
        enchantEnabled = !enchantEnabled;
        ApplyProperties();
    }

    public void SetEnchantColor(Color newColor)
    {
        enchantColor = newColor;
        ApplyProperties();
    }

    public void SetWaveSpeed(float newSpeed)
    {
        waveSpeed = newSpeed;
        ApplyProperties();
    }

    public void SetWaveFrequency(float newFrequency)
    {
        waveFrequency = newFrequency;
        ApplyProperties();
    }

    public void SetWaveWidth(float newWidth)
    {
        waveWidth = newWidth;
        ApplyProperties();
    }

    public void SetWaveStrength(float newStrength)
    {
        waveStrength = newStrength;
        ApplyProperties();
    }

    public void SetDiagonalAmount(float newDiagonalAmount)
    {
        diagonalAmount = newDiagonalAmount;
        ApplyProperties();
    }

    public void ApplyRarityVisualsNow()
    {
        RefreshFromRarity();
        ApplyProperties();
    }
}