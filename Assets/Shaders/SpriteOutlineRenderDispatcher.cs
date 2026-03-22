using System.Collections.Generic;
using UnityEngine;

public class SpriteOutlineRenderDispatcher : MonoBehaviour
{
    public static SpriteOutlineRenderDispatcher Instance { get; private set; }

    [Header("Shared runtime material using the custom outline shader")]
    [SerializeField] private Material runtimeMaterial;

    [Header("Shared render threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float renderAlphaThreshold = 0.001f;

    private static readonly HashSet<SpriteOutlineMapRenderer> registeredRenderers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ApplyToAllRegisteredRenderers();
    }

    private void Start()
    {
        ScanSceneAndRegisterAll();
        ApplyToAllRegisteredRenderers();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void RegisterRenderer(SpriteOutlineMapRenderer renderer)
    {
        if (renderer == null)
            return;

        registeredRenderers.Add(renderer);

        if (Instance != null)
            Instance.ApplyToRendererInternal(renderer);
    }

    public static void UnregisterRenderer(SpriteOutlineMapRenderer renderer)
    {
        if (renderer == null)
            return;

        registeredRenderers.Remove(renderer);
    }

    public static bool TryApplyToRenderer(SpriteOutlineMapRenderer renderer)
    {
        if (Instance == null || renderer == null)
            return false;

        Instance.ApplyToRendererInternal(renderer);
        return true;
    }

    [ContextMenu("Apply To All Registered Renderers")]
    public void ApplyToAllRegisteredRenderers()
    {
        registeredRenderers.RemoveWhere(r => r == null);

        foreach (SpriteOutlineMapRenderer renderer in registeredRenderers)
        {
            ApplyToRendererInternal(renderer);
        }
    }

    [ContextMenu("Scan Scene And Register All")]
    public void ScanSceneAndRegisterAll()
    {
        SpriteOutlineMapRenderer[] found = FindObjectsByType<SpriteOutlineMapRenderer>(FindObjectsSortMode.None);

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
                registeredRenderers.Add(found[i]);
        }
    }

    private void ApplyToRendererInternal(SpriteOutlineMapRenderer renderer)
    {
        if (renderer == null || runtimeMaterial == null)
            return;

        renderer.ReceiveRuntimeMaterial(runtimeMaterial);
        renderer.ReceiveRenderSettings(renderAlphaThreshold);
    }

    public void SetSharedRenderAlphaThreshold(float value)
    {
        renderAlphaThreshold = Mathf.Clamp01(value);
        ApplyToAllRegisteredRenderers();
    }

    public void SetSharedRuntimeMaterial(Material material)
    {
        runtimeMaterial = material;
        ApplyToAllRegisteredRenderers();
    }
}