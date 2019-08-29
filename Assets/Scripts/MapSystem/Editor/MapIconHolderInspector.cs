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

    private void OnEnable()
    {
        icon = serializedObject.FindProperty("icon");
        iconSize = serializedObject.FindProperty("iconSize");
        iconType = serializedObject.FindProperty("iconType");
        drawOnWorldMap = serializedObject.FindProperty("drawOnWorldMap");
        keepOnMap = serializedObject.FindProperty("keepOnMap");
        maxValidDistance = serializedObject.FindProperty("maxValidDistance");
        forceHided = serializedObject.FindProperty("forceHided");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(icon, new GUIContent("图标"));
        EditorGUILayout.PropertyField(iconSize, new GUIContent("图标大小"));
        EditorGUILayout.IntPopup(iconType, new GUIContent[] { new GUIContent("普通"), new GUIContent("标记"), new GUIContent("任务") }, new int[] { 0, 2, 3 }, new GUIContent("图标类型"));
        if (Application.isPlaying) GUI.enabled = true;
        EditorGUILayout.PropertyField(keepOnMap, new GUIContent("保持显示"));
        EditorGUILayout.PropertyField(drawOnWorldMap, new GUIContent("在大地图上显示"));
        EditorGUILayout.PropertyField(maxValidDistance, new GUIContent("最大有效显示距离"));
        EditorGUILayout.PropertyField(forceHided, new GUIContent("强制隐藏"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}