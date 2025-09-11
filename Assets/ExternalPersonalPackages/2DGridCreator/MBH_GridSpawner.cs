using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MBH_GridSpawner : MonoBehaviour
{
    // ── Enums ───────────────────────────────────────────────────────────────────
    public enum OptionsForSize
    {
        BasedOnGridWidthAndHeight,
        // Uses gridWidth/gridHeight counts. If using a prefab, it auto-scales to fit your cellSize.
        // Gap is applied between cells.

        BasedOnTargetWidthAndHeight,
        // Tries to fit the MAX number of cells within targetUnitsWidth/targetUnitsHeight,
        // using the prefab’s *native world size* and then applying cellSize ADDITIVELY to that size.
        // (baseSize + cellSize). Gap is applied between cells.
    }
    public OptionsForSize sizeOption = OptionsForSize.BasedOnGridWidthAndHeight;

    public enum OptionsForTile
    {
        UseDefaultTile, // A 1x1 unit white square (SpriteRenderer) tinted with defaultTileColor
        UsePrefab       // Uses your prefab (must contain a SpriteRenderer)
    }
    public OptionsForTile tileOption = OptionsForTile.UseDefaultTile;

    // ── Common params ───────────────────────────────────────────────────────────
    [Min(1)] public int gridWidth = 5;
    [Min(1)] public int gridHeight = 5;

    [Tooltip("Only used in BasedOnTargetWidthAndHeight mode (world units).")]
    [Min(0.0001f)] public float targetUnitsWidth = 10f;

    [Tooltip("Only used in BasedOnTargetWidthAndHeight mode (world units).")]
    [Min(0.0001f)] public float targetUnitsHeight = 10f;

    [Tooltip("Cell size meaning depends on mode: \n- GridWidthAndHeight: explicit desired cell edge (units).\n- TargetWidthAndHeight: ADDITIVE delta to prefab’s base size.")]
    public float cellSize = 1f;

    [Tooltip("Space between cells (world units).")]
    public float gap_size = 0f;

    [Tooltip("Parent under which cells are created. If null, one is created.")]
    public Transform board_parent;

    [Tooltip("Required if tileOption = UsePrefab. Must contain a SpriteRenderer.")]
    public GameObject prefab;

    [Tooltip("Tint for the default generated tile.")]
    public Color defaultTileColor = Color.white;

    public bool show_logs = false;

    // ── Helpers (used by editor) ────────────────────────────────────────────────
    public GameObject CreateDefaultTileGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent, false);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = MakeUnitSprite();
        sr.color = defaultTileColor;
        return go;
    }

    public static Sprite MakeUnitSprite()
    {
        // 1x1 world-unit white sprite from a 2x2 texture
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        var pivot = new Vector2(0.5f, 0.5f);
        // Pixels Per Unit = 2  => sprite world size ≈ (1,1) at scale (1,1,1)
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), pivot, 2f);
    }

    public Transform GetOrCreateBoardParent()
    {
        if (board_parent != null) return board_parent;
        var holder = transform.Find("Board");
        if (holder == null)
        {
            var go = new GameObject("Board");
            go.transform.SetParent(transform, false);
            holder = go.transform;
        }
        board_parent = holder;
        return holder;
    }

    public static float GetSpriteWorldEdge(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null)
        {
            // default edge size if nothing valid is provided
            return 1f;
        }

        Vector2 size = sr.bounds.size;
        return Mathf.Max(size.x, size.y);
    }

    // ── Color application options ─────────────────────────────────────────────────
    public enum ApplyColorToTiles
    {
        None,
        ChessPattern,              // requires colorA, colorB
        UniqueColorBetween2Colors  // generates unique colors along the A→B gradient
    }
    public ApplyColorToTiles colorMode = ApplyColorToTiles.None;

    // Used when colorMode != None
    public Color colorA = Color.white;
    public Color colorB = Color.black;

    public void ApplyColorToCell(int x, int y, SpriteRenderer sr, int width, int height, int flatIndex)
    {
        switch (colorMode)
        {
            case ApplyColorToTiles.None:
                return;

            case ApplyColorToTiles.ChessPattern:
                sr.color = ((x + y) % 2 == 0) ? colorA : colorB;
                return;

            case ApplyColorToTiles.UniqueColorBetween2Colors:
                {
                    // Deterministic unique color along the gradient A→B.
                    // flatIndex ∈ [0, width*height-1]
                    int total = Mathf.Max(1, width * height - 1);
                    float t = (total == 0) ? 0f : (float)flatIndex / total;
                    sr.color = Color.Lerp(colorA, colorB, t);
                    return;
                }
        }
    }

    public void RecolorExistingGrid()
    {
        var parent = GetOrCreateBoardParent();
        int childCount = parent.childCount;

        if (childCount == 0) return;

        // Try to infer width/height from current layout.
        // Fallback to stored gridWidth/Height.
        int width = gridWidth;
        int height = gridHeight;

        // If we created by target-fit mode, try to re-derive from children count
        if (sizeOption == OptionsForSize.BasedOnTargetWidthAndHeight)
        {
            // pick a reasonably compact factorization close to square
            FactorToGrid(childCount, out width, out height);
        }

        for (int i = 0; i < childCount; i++)
        {
            var t = parent.GetChild(i);
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            int x = i % width;
            int y = i / width;
            ApplyColorToCell(x, y, sr, width, height, i);
        }
    }

    // Quick helper to factor N into width/height (closest to square)
    static void FactorToGrid(int n, out int width, out int height)
    {
        width = Mathf.CeilToInt(Mathf.Sqrt(n));
        height = Mathf.CeilToInt((float)n / width);
    }





}
