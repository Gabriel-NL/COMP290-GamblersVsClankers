using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MBH_GridSpawner))]
public class Editor_GridSpawner : Editor
{
    // Serialized props (so we can selectively draw fields)
    SerializedProperty sizeOption;
    SerializedProperty tileOption;

    SerializedProperty gridWidth;
    SerializedProperty gridHeight;

    SerializedProperty targetUnitsWidth;
    SerializedProperty targetUnitsHeight;

    SerializedProperty cellSize;
    SerializedProperty gap_size;

    SerializedProperty board_parent;
    SerializedProperty prefab;
    SerializedProperty defaultTileColor;
    SerializedProperty show_logs;
    SerializedProperty colorMode;
    SerializedProperty colorA;
    SerializedProperty colorB;


    MBH_GridSpawner spawner;

    readonly Dictionary<int, Color> colorDict = new Dictionary<int, Color>();

    void OnEnable()
    {
        spawner = (MBH_GridSpawner)target;

        sizeOption = serializedObject.FindProperty(nameof(MBH_GridSpawner.sizeOption));
        tileOption = serializedObject.FindProperty(nameof(MBH_GridSpawner.tileOption));

        gridWidth = serializedObject.FindProperty(nameof(MBH_GridSpawner.gridWidth));
        gridHeight = serializedObject.FindProperty(nameof(MBH_GridSpawner.gridHeight));

        targetUnitsWidth = serializedObject.FindProperty(nameof(MBH_GridSpawner.targetUnitsWidth));
        targetUnitsHeight = serializedObject.FindProperty(nameof(MBH_GridSpawner.targetUnitsHeight));

        cellSize = serializedObject.FindProperty(nameof(MBH_GridSpawner.cellSize));
        gap_size = serializedObject.FindProperty(nameof(MBH_GridSpawner.gap_size));

        board_parent = serializedObject.FindProperty(nameof(MBH_GridSpawner.board_parent));
        prefab = serializedObject.FindProperty(nameof(MBH_GridSpawner.prefab));
        defaultTileColor = serializedObject.FindProperty(nameof(MBH_GridSpawner.defaultTileColor));
        show_logs = serializedObject.FindProperty(nameof(MBH_GridSpawner.show_logs));

        colorMode = serializedObject.FindProperty(nameof(MBH_GridSpawner.colorMode));
        colorA = serializedObject.FindProperty(nameof(MBH_GridSpawner.colorA));
        colorB = serializedObject.FindProperty(nameof(MBH_GridSpawner.colorB));

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Always visible core controls (enums first) ──────────────────────────
        EditorGUILayout.PropertyField(sizeOption);
        EditorGUILayout.PropertyField(tileOption);

        // ── Conditional visibility based on enums ───────────────────────────────
        var sizeMode = (MBH_GridSpawner.OptionsForSize)sizeOption.enumValueIndex;
        var tileMode = (MBH_GridSpawner.OptionsForTile)tileOption.enumValueIndex;

        // Common to both modes

        if (sizeMode == MBH_GridSpawner.OptionsForSize.BasedOnGridWidthAndHeight)
        {
            EditorGUILayout.PropertyField(gridWidth);
            EditorGUILayout.PropertyField(gridHeight);
            EditorGUILayout.PropertyField(cellSize, new GUIContent("Cell Size (world units)"));
        }
        else // BasedOnTargetWidthAndHeight
        {
            EditorGUILayout.PropertyField(targetUnitsWidth, new GUIContent("Target Width (units)"));
            EditorGUILayout.PropertyField(targetUnitsHeight, new GUIContent("Target Height (units)"));
            EditorGUILayout.PropertyField(cellSize, new GUIContent("Additive Cell Size (+units)"));
        }

        EditorGUILayout.PropertyField(gap_size);

        if (tileMode == MBH_GridSpawner.OptionsForTile.UsePrefab)
        {
            EditorGUILayout.PropertyField(prefab);
        }
        else
        {
            EditorGUILayout.PropertyField(defaultTileColor);
        }

        EditorGUILayout.PropertyField(board_parent);
        EditorGUILayout.PropertyField(show_logs);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(tileMode == MBH_GridSpawner.OptionsForTile.UsePrefab && prefab.objectReferenceValue == null))
        {
            if (GUILayout.Button("Create Grid"))
            {
                CreateGrid(spawner);
            }
        }

        EditorGUILayout.PropertyField(colorMode);

        ColorModeColorsVisibility();

        EditorGUILayout.Space();

        if (GUILayout.Button("Recolor Tiles"))
        {
            spawner.RecolorExistingGrid();
            EditorUtility.SetDirty(spawner);
        }


        serializedObject.ApplyModifiedProperties();
    }

    void ColorModeColorsVisibility()
    {
        var cm = (MBH_GridSpawner.ApplyColorToTiles)colorMode.enumValueIndex;
        if (cm == MBH_GridSpawner.ApplyColorToTiles.SingleColor)
        {
            EditorGUILayout.PropertyField(colorA, new GUIContent("Color A"));
            return;
        }
        if (cm == MBH_GridSpawner.ApplyColorToTiles.ChessPattern ||
            cm == MBH_GridSpawner.ApplyColorToTiles.UniqueColorBetween2Colors || cm == MBH_GridSpawner.ApplyColorToTiles.MultipleRowsGradient)
        {
            EditorGUILayout.PropertyField(colorA, new GUIContent("Color A"));
            EditorGUILayout.PropertyField(colorB, new GUIContent("Color B"));
        }
    }
    // ── Grid creation ───────────────────────────────────────────────────────────
    void CreateGrid(MBH_GridSpawner sp)
    {
        var parent = (sp).board_parent;

        // Clear previous
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);

        var sizeMode = sp.sizeOption;
        var tileMode = sp.tileOption;

        int usedWidth, usedHeight;
        float cellEdge;

        if (sizeMode == MBH_GridSpawner.OptionsForSize.BasedOnGridWidthAndHeight)
        {
            usedWidth = Mathf.Max(1, sp.gridWidth);
            usedHeight = Mathf.Max(1, sp.gridHeight);
            cellEdge = Mathf.Max(0.0001f, sp.cellSize);
        }
        else
        {
            // Fit as many as possible inside target area:
            // Base size from prefab/default tile, then ADDITIVE cellSize
            float baseEdge = 1f;
            if (tileMode == MBH_GridSpawner.OptionsForTile.UsePrefab && sp.prefab != null)
            {
                var sr = sp.prefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) baseEdge = Mathf.Max(sr.bounds.size.x, sr.bounds.size.y);
            }
            cellEdge = Mathf.Max(0.0001f, baseEdge + sp.cellSize);

            float stride = cellEdge + sp.gap_size;

            usedWidth = Mathf.Max(1, Mathf.FloorToInt((sp.targetUnitsWidth + sp.gap_size) / stride));
            usedHeight = Mathf.Max(1, Mathf.FloorToInt((sp.targetUnitsHeight + sp.gap_size) / stride));

            if (sp.show_logs)
                Debug.Log($"[GridSpawner] Target fit → cellEdge={cellEdge:F3}, stride={stride:F3}, count=({usedWidth},{usedHeight})");
        }

        float totalW = usedWidth * (cellEdge + sp.gap_size) - sp.gap_size;
        float totalH = usedHeight * (cellEdge + sp.gap_size) - sp.gap_size;

        // Start in bottom-left so the grid is centered on board_parent
        Vector3 origin = new Vector3(-totalW * 0.5f + cellEdge * 0.5f, -totalH * 0.5f + cellEdge * 0.5f, 0f);

        int colorIndex = 0;

        for (int y = 0; y < usedHeight; y++)
        {
            for (int x = 0; x < usedWidth; x++)
            {
                GameObject cellGO;

                if (tileMode == MBH_GridSpawner.OptionsForTile.UsePrefab)
                {
                    if (sp.prefab == null)
                    {
                        Debug.LogError("[GridSpawner] Prefab is null but tile option is UsePrefab.");
                        return;
                    }
                    cellGO = (GameObject)PrefabUtility.InstantiatePrefab(sp.prefab, sp.board_parent);
                    if (cellGO.transform.parent != sp.board_parent) cellGO.transform.SetParent(sp.board_parent, true);
                }
                else
                {
                    cellGO = sp.CreateDefaultTileGO("DefaultCell", sp.board_parent);
                }

                cellGO.name = $"Cell_{x}_{y}";

                // Position
                var pos = origin + new Vector3(x * (cellEdge + sp.gap_size), y * (cellEdge + sp.gap_size), 0f);
                cellGO.transform.localPosition = pos;

                // Scale to desired cellEdge (uniform)
                var sr = cellGO.GetComponentInChildren<SpriteRenderer>();
                if (sr == null)
                {
                    Debug.LogError("[GridSpawner] Spawned object has no SpriteRenderer.");
                    DestroyImmediate(cellGO);
                    continue;
                }

                float currentEdge = Mathf.Max(0.0001f, Mathf.Max(sr.bounds.size.x, sr.bounds.size.y));
                float scaleFactor = cellEdge / currentEdge;

                // Apply uniform local scale (respecting existing localScale)
                var t = cellGO.transform;
                var ls = t.localScale;
                t.localScale = new Vector3(ls.x * scaleFactor, ls.y * scaleFactor, ls.z);

                // Optional: stash colors seen (you had a color_dict before)
                if (!colorDict.ContainsValue(sr.color))
                    colorDict.Add(colorIndex++, sr.color);
            }
        }

        if (sp.show_logs)
            Debug.Log($"[GridSpawner] Created {usedWidth} x {usedHeight} cells. Cell={cellEdge:F3}, Gap={sp.gap_size:F3}");
    }
}
