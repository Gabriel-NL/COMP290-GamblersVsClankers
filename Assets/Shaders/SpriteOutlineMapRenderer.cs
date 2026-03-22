using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineMapRenderer : MonoBehaviour
{
    
    [System.Serializable]
    public class SpriteMapData
    {
        public Vector2Int size;
        public Vector2Int[] visiblePixels;
        public Vector2Int[] outlineablePixels;
        public Texture2D maskTexture;
    }

    [Header("Build threshold: alpha >= this is considered visible for map generation")]
    [SerializeField] private byte visibleAlphaThreshold = 1;

    [Header("Render threshold: alpha must be above this in shader to render visible sprite pixels")]
    [Range(0f, 1f)]
    [SerializeField] private float renderAlphaThreshold = 0.001f;

    [Header("Enchant / Outline")]
    [SerializeField] private bool enchantEnabled = true;
    [SerializeField] private Color enchantColor = Color.white;

    private SpriteRenderer spriteRenderer;
    private Sprite lastSprite;
    private Material sourceRuntimeMaterial;
    private Coroutine waitForProviderCoroutine;
    private int outlineThickness = 3;

    private readonly Dictionary<Sprite, SpriteMapData> spriteMapDictionary = new();
    public IReadOnlyDictionary<Sprite, SpriteMapData> SpriteMapDictionary => spriteMapDictionary;

    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int MaskTexId = Shader.PropertyToID("_MaskTex");
    private static readonly int SpriteRectId = Shader.PropertyToID("_SpriteRect");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int ShowEnchantId = Shader.PropertyToID("_ShowEnchant");
    private static readonly int AlphaCutoffId = Shader.PropertyToID("_AlphaCutoff");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SpriteOutlineRenderDispatcher.RegisterRenderer(this);

        if (waitForProviderCoroutine != null)
            StopCoroutine(waitForProviderCoroutine);

        waitForProviderCoroutine = StartCoroutine(CoWaitForProviderAndApply());
    }

    private void OnDisable()
    {
        SpriteOutlineRenderDispatcher.UnregisterRenderer(this);

        if (waitForProviderCoroutine != null)
        {
            StopCoroutine(waitForProviderCoroutine);
            waitForProviderCoroutine = null;
        }
    }

    private void LateUpdate()
    {
        RefreshCurrentSprite(force: false);
    }

    private IEnumerator CoWaitForProviderAndApply()
    {
        while (!HasValidRuntimeMaterial())
        {
            SpriteOutlineRenderDispatcher.TryApplyToRenderer(this);
            yield return null;
        }

        RefreshCurrentSprite(force: true);
        waitForProviderCoroutine = null;
    }

    private bool HasValidRuntimeMaterial()
    {
        Material mat = GetTargetMaterial();
        return mat != null && sourceRuntimeMaterial != null;
    }

    public void ReceiveRuntimeMaterial(Material runtimeMaterial)
    {
        if (runtimeMaterial == null)
            return;

        sourceRuntimeMaterial = runtimeMaterial;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        Material current = spriteRenderer.material;
        bool needsNewInstance =
            current == null ||
            current.shader != runtimeMaterial.shader ||
            current.name != runtimeMaterial.name + " (Runtime Instance)";

        if (needsNewInstance)
        {
            Material instance = new Material(runtimeMaterial);
            instance.name = runtimeMaterial.name + " (Runtime Instance)";
            spriteRenderer.material = instance;
        }

        ClearMaskSoSpriteRemainsVisible();
        ApplyVisualProperties();
        RefreshCurrentSprite(force: true);
    }

    public void ReceiveRenderSettings(float alphaThreshold)
    {
        renderAlphaThreshold = Mathf.Clamp01(alphaThreshold);
        ApplyVisualProperties();
    }

    private Material GetTargetMaterial()
    {
        if (spriteRenderer == null)
            return null;

        return spriteRenderer.material;
    }

    private void ClearMaskSoSpriteRemainsVisible()
    {
        Material mat = GetTargetMaterial();
        if (mat == null)
            return;

        mat.SetTexture(MaskTexId, null);
    }

    private void ApplyVisualProperties()
    {
        Material mat = GetTargetMaterial();
        if (mat == null)
            return;

        mat.SetColor(OutlineColorId, enchantColor);
        mat.SetFloat(ShowEnchantId, enchantEnabled ? 1f : 0f);
        mat.SetFloat(AlphaCutoffId, Mathf.Clamp01(renderAlphaThreshold));
    }

    private void RefreshCurrentSprite(bool force)
    {
        Material mat = GetTargetMaterial();
        if (spriteRenderer == null || mat == null)
            return;

        Sprite currentSprite = spriteRenderer.sprite;
        if (currentSprite == null)
        {
            lastSprite = null;
            ClearMaskSoSpriteRemainsVisible();
            return;
        }

        if (!force && currentSprite == lastSprite)
            return;

        lastSprite = currentSprite;

        if (!spriteMapDictionary.TryGetValue(currentSprite, out SpriteMapData data) || data == null || data.maskTexture == null)
        {
            data = BuildMapForSprite(currentSprite);
            spriteMapDictionary[currentSprite] = data;
        }

        ApplyToMaterial(currentSprite, data);
    }

    private SpriteMapData BuildMapForSprite(Sprite sprite)
    {
        Texture2D sourceTexture = sprite.texture;

        if (!sourceTexture.isReadable)
        {
            Debug.LogError(
                $"Texture '{sourceTexture.name}' is not readable. Enable Read/Write in import settings.",
                this
            );
            return null;
        }

        Rect rect = sprite.textureRect;

        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        int startX = Mathf.RoundToInt(rect.x);
        int startY = Mathf.RoundToInt(rect.y);

        Color32[] pixels = sourceTexture.GetPixels32();
        int texWidth = sourceTexture.width;

        bool[,] visibleMap = new bool[width, height];
        List<Vector2Int> visiblePixels = new List<Vector2Int>();
        List<Vector2Int> outlineablePixels = new List<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            int texY = startY + y;

            for (int x = 0; x < width; x++)
            {
                int texX = startX + x;
                int flatIndex = texY * texWidth + texX;

                byte alpha = pixels[flatIndex].a;
                bool isVisible = alpha >= visibleAlphaThreshold;

                visibleMap[x, y] = isVisible;

                if (isVisible)
                    visiblePixels.Add(new Vector2Int(x, y));
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (visibleMap[x, y])
                    continue;

                if (HasVisibleNeighborInRange(visibleMap, x, y, width, height, outlineThickness))
                    outlineablePixels.Add(new Vector2Int(x, y));
            }
        }

        Texture2D maskTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        maskTexture.filterMode = FilterMode.Point;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        Color32[] maskPixels = new Color32[width * height];

        for (int i = 0; i < maskPixels.Length; i++)
            maskPixels[i] = new Color32(0, 0, 0, 0);

        foreach (Vector2Int p in visiblePixels)
        {
            int index = p.y * width + p.x;
            maskPixels[index].r = 255;
            maskPixels[index].a = 255;
        }

        foreach (Vector2Int p in outlineablePixels)
        {
            int index = p.y * width + p.x;
            maskPixels[index].g = 255;
            maskPixels[index].a = 255;
        }

        maskTexture.SetPixels32(maskPixels);
        maskTexture.Apply(false, false);

        return new SpriteMapData
        {
            size = new Vector2Int(width, height),
            visiblePixels = visiblePixels.ToArray(),
            outlineablePixels = outlineablePixels.ToArray(),
            maskTexture = maskTexture
        };
    }

    private void ApplyToMaterial(Sprite sprite, SpriteMapData data)
    {
        Material mat = GetTargetMaterial();
        if (mat == null)
            return;

        if (data == null || data.maskTexture == null)
        {
            ClearMaskSoSpriteRemainsVisible();
            ApplyVisualProperties();
            return;
        }

        Texture2D atlas = sprite.texture;
        Rect texRect = sprite.textureRect;

        Vector4 normalizedRect = new Vector4(
            texRect.x / atlas.width,
            texRect.y / atlas.height,
            texRect.width / atlas.width,
            texRect.height / atlas.height
        );

        mat.SetTexture(MainTexId, atlas);
        mat.SetTexture(MaskTexId, data.maskTexture);
        mat.SetVector(SpriteRectId, normalizedRect);

        mat.SetColor(OutlineColorId, enchantColor);
        mat.SetFloat(ShowEnchantId, enchantEnabled ? 1f : 0f);
        mat.SetFloat(AlphaCutoffId, Mathf.Clamp01(renderAlphaThreshold));
    }

    private static bool IsVisible(bool[,] map, int x, int y, int width, int height)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        return map[x, y];
    }
    private static bool HasVisibleNeighborInRange(bool[,] map, int centerX, int centerY, int width, int height, int range)
    {
        if (range <= 0)
            return false;

        for (int y = centerY - range; y <= centerY + range; y++)
        {
            for (int x = centerX - range; x <= centerX + range; x++)
            {
                if (x == centerX && y == centerY)
                    continue;

                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;

                if (map[x, y])
                    return true;
            }
        }

        return false;
    }

    public bool TryGetMap(Sprite sprite, out SpriteMapData data)
    {
        return spriteMapDictionary.TryGetValue(sprite, out data);
    }

    public void SetEnchantColor(Color color)
    {
        enchantColor = color;
        ApplyVisualProperties();
    }

    public void SetEnchantEnabled(bool enabled)
    {
        enchantEnabled = enabled;
        ApplyVisualProperties();
    }

    public void SetVisibleAlphaThreshold(byte threshold, bool rebuildCachedMaps = true)
    {
        if (visibleAlphaThreshold == threshold)
            return;

        visibleAlphaThreshold = threshold;

        if (rebuildCachedMaps)
            RebuildAllMaps();
        else
            RefreshCurrentSprite(force: true);
    }

    public void SetRenderAlphaThreshold(float threshold01)
    {
        renderAlphaThreshold = Mathf.Clamp01(threshold01);
        ApplyVisualProperties();
    }

    private void RebuildAllMaps()
    {
        foreach (SpriteMapData data in spriteMapDictionary.Values)
        {
            if (data != null && data.maskTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(data.maskTexture);
                else
                    DestroyImmediate(data.maskTexture);
            }
        }

        spriteMapDictionary.Clear();
        lastSprite = null;

        ClearMaskSoSpriteRemainsVisible();
        RefreshCurrentSprite(force: true);
    }
}