using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapIconHolder))]
public class MapIconHolderInspector : Editor
{
    SerializedProperty icon;
    SerializedProperty iconSize;
    SerializedProperty iconType;
    SerializedProperty drawOnWorldMap;
    SerializedProperty keepOnMap;
    SerializedProperty maxValidDistance;
    SerializedProperty forceHided;
    SerializedProperty showRange;
    SerializedProperty rangeColor;
    SerializedProperty rangeSize;

    private void OnEnable()
    {
        icon = serializedObject.FindProperty("icon");
        iconSize = serializedObject.FindProperty("iconSize");
        iconType = serializedObject.FindProperty("iconType");
        drawOnWorldMap = serializedObject.FindProperty("drawOnWorldMap");
        keepOnMap = serializedObject.FindProperty("keepOnMap");
        maxValidDistance = serializedObject.FindProperty("maxValidDistance");
        forceHided = serializedObject.FindProperty("forceHided");
        showRange = serializedObject.FindProperty("showRange");
        rangeColor = serializedObject.FindProperty("rangeColor");
        rangeSize = serializedObject.FindProperty("rangeSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(icon, new GUIContent("图标"));
        EditorGUILayout.PropertyField(iconSize, new GUIContent("图标大小"));
        EditorGUILayout.IntPopup(iconType, new GUIContent[] { new GUIContent("普通"), new GUIContent("任务") }, new int[] { 0, 3 }, new GUIContent("图标类型"));
        EditorGUILayout.PropertyField(keepOnMap, new GUIContent("保持显示"));
        EditorGUILayout.PropertyField(drawOnWorldMap, new GUIContent("在大地图上显示"));
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(maxValidDistance, new GUIContent("最大有效显示距离"));
        if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.PropertyField(forceHided, new GUIContent("强制隐藏"));
        EditorGUILayout.PropertyField(showRange, new GUIContent("显示范围"));
        if (showRange.boolValue)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(rangeColor, new GUIContent("范围颜色"));
            EditorGUILayout.PropertyField(rangeSize, new GUIContent("范围半径"));
            EditorGUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying) (target as MapIconHolder).distanceSqr = maxValidDistance.floatValue * maxValidDistance.floatValue;
        }
    }
}