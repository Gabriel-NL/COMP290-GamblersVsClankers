using System;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif

public class MBH_GridSpawner : MonoBehaviour
{
    // ── Enums ───────────────────────────────────────────────────────────────────
    public enum OptionsForSize
    {
        BasedOnGridWidthAndHeight,
        BasedOnTargetWidthAndHeight,
    }
    public OptionsForSize sizeOption = OptionsForSize.BasedOnGridWidthAndHeight;

    public enum OptionsForTile
    {
        UseDefaultTile,
        UsePrefab
    }
    public OptionsForTile tileOption = OptionsForTile.UseDefaultTile;

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
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        var pivot = new Vector2(0.5f, 0.5f);

        return Sprite.Create(tex, new Rect(0, 0, 2, 2), pivot, 2f);
    }

    public static float GetSpriteWorldEdge(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null)
        {
            return 1f;
        }

        Vector2 size = sr.bounds.size;
        return Mathf.Max(size.x, size.y);
    }

    public enum ApplyColorToTiles
    {
        None,
        SingleColor,
        ChessPattern,
        UniqueColorBetween2Colors
    }
    public ApplyColorToTiles colorMode = ApplyColorToTiles.None;

    public Color colorA = Color.white;
    public Color colorB = Color.black;

    public void RecolorExistingGrid()
    {
        if (board_parent == null) { return; }
        int childCount = board_parent.childCount;

        if (childCount == 0) return;

        SpriteRenderer[] spriteRendererArray = board_parent.GetComponentsInChildren<SpriteRenderer>();

        switch (colorMode)
        {
            case ApplyColorToTiles.None:
                return;
            case ApplyColorToTiles.SingleColor:
                for (int i = 0; i < spriteRendererArray.Length; i++)
                {
                    if (spriteRendererArray[i] != null)
                        spriteRendererArray[i].color = colorA;
                }
                return;

            case ApplyColorToTiles.ChessPattern:
                ApplyChessPattern(spriteRendererArray, colorA, colorB);
                return;

            case ApplyColorToTiles.UniqueColorBetween2Colors:
                {
                    UniqueColorBetween2Colors(spriteRendererArray, colorA, colorB);
                    return;
                }
        }
    }

    public void ApplyChessPattern(SpriteRenderer[] tiles, Color colorA, Color colorB)
    {


        List<float> uniqueXs = new List<float>();

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null) continue;

            Vector3 positionVector = tiles[i].transform.localPosition;
            if (uniqueXs.Contains(positionVector.x) == false)
            {
                uniqueXs.Add(positionVector.x);
            }
        }
        int width = uniqueXs.Count;

        bool isEven = (width % 2) == 0;
        bool isLastInRow;

        bool alternate = true;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (alternate)
            {
                tiles[i].color = colorA;
            }
            else
            {
                tiles[i].color = colorB;
            }
            bool stopAlternation = isEven && (i + 1) % width == 0;

            if (stopAlternation)
            {
                continue;
            }
            alternate = !alternate;
        }
    }

    public void UniqueColorBetween2Colors(SpriteRenderer[] tiles, Color colorA, Color colorB)
    {
        if (tiles == null || tiles.Length == 0) return;

        int count = tiles.Length;

        Color[] gradientColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0f : (float)i / (count - 1);
            gradientColors[i] = Color.Lerp(colorA, colorB, t);
        }

        System.Random rng = new System.Random();
        for (int i = count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (gradientColors[i], gradientColors[j]) = (gradientColors[j], gradientColors[i]);
        }

        for (int i = 0; i < count; i++)
        {
            if (tiles[i] != null)
                tiles[i].color = gradientColors[i];
        }
    }

}
