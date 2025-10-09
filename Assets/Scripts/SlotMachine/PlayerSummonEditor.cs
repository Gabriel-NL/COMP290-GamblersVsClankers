#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerSummon))]
public class PlayerSummonEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 4 lines + spacing
        const int lines = 4;
        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nameProp = property.FindPropertyRelative("summonName");
        var spriteProp = property.FindPropertyRelative("summonSprite");
        var costProp = property.FindPropertyRelative("summonCost");
        var prefabProp = property.FindPropertyRelative("summonPrefab");

        // Layout
        float lineH = EditorGUIUtility.singleLineHeight;
        float padY = EditorGUIUtility.standardVerticalSpacing;

        var r0 = new Rect(position.x, position.y, position.width, lineH);
        var r1 = new Rect(position.x, r0.yMax + padY, position.width, lineH);
        var r2 = new Rect(position.x, r1.yMax + padY, position.width, lineH);
        var r3 = new Rect(position.x, r2.yMax + padY, position.width, lineH);

        // Draw fields
        EditorGUI.PropertyField(r0, nameProp, new GUIContent("Summon Name"));
        EditorGUI.PropertyField(r1, spriteProp, new GUIContent("Summon Sprite"));
        EditorGUI.PropertyField(r2, costProp, new GUIContent("Summon Cost"));

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(r3, prefabProp, new GUIContent("Summon Prefab"));
        bool prefabChanged = EditorGUI.EndChangeCheck();

        // Auto-fill when prefab assigned and the scene settings opt-in is true (default true)
        if (prefabChanged)
        {
            var prefab = prefabProp.objectReferenceValue as GameObject;

        }

        EditorGUI.EndProperty();
    }


}
#endif
