using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ListViewBase), true)]
public class ListViewInspector : Editor
{
    ListViewBase view;
    SerializedProperty direction;
    SerializedProperty childAlignment;
    SerializedProperty padding;
    SerializedProperty spacing;
    SerializedProperty overrideCellSize;
    SerializedProperty fillWidth;
    SerializedProperty fillHeight;
    SerializedProperty cellSize;
    SerializedProperty applyCellSize;

    SerializedProperty clickable;
    SerializedProperty selectable;
    SerializedProperty multiSelection;

    SerializedProperty cacheCapacity;
    SerializedProperty container;
    SerializedProperty prefab;

    private void OnEnable()
    {
        view = target as ListViewBase;

        direction = serializedObject.FindProperty("direction");
        childAlignment = serializedObject.FindProperty("childAlignment");
        padding = serializedObject.FindProperty("padding");
        spacing = serializedObject.FindProperty("spacing");
        overrideCellSize = serializedObject.FindProperty("overrideCellSize");
        fillWidth = serializedObject.FindProperty("fillWidth");
        fillHeight = serializedObject.FindProperty("fillHeight");
        cellSize = serializedObject.FindProperty("cellSize");
        applyCellSize = serializedObject.FindProperty("applyCellSize");
        clickable = serializedObject.FindProperty("clickable");
        selectable = serializedObject.FindProperty("selectable");
        multiSelection = serializedObject.FindProperty("multiSelection");
        cacheCapacity = serializedObject.FindProperty("cacheCapacity");
        container = serializedObject.FindProperty("container");
        prefab = serializedObject.FindProperty("prefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUILayout.PropertyField(direction, new GUIContent("布局方向"));
        EditorGUILayout.PropertyField(childAlignment, new GUIContent("对齐方式"));
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(padding, new GUIContent("四周填充"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.PropertyField(spacing, new GUIContent("元素间距", (view is IGridView) ? string.Empty : "X、Y值分别对应各布局方向的间距"));
        if (view is not IGridView) EditorGUILayout.PropertyField(overrideCellSize, new GUIContent("覆盖元素大小"));
        else overrideCellSize.boolValue = true;
        if (overrideCellSize.boolValue)
        {
            if (view is not IGridView)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(fillWidth, new GUIContent("水平填充"));
                EditorGUILayout.PropertyField(fillHeight, new GUIContent("垂直填充"));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.PropertyField(cellSize, new GUIContent("首选元素大小"));
        }
        if (Application.isPlaying) EditorGUILayout.PropertyField(applyCellSize, new GUIContent("应用元素大小"));
        EditorGUILayout.PropertyField(clickable, new GUIContent("元素可点击"));
        if (clickable.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(selectable, new GUIContent("元素可选中"));
            if (selectable.boolValue)
            {
                bool bef = GUI.enabled;
                GUI.enabled = true;
                EditorGUILayout.PropertyField(multiSelection, new GUIContent("多选"));
                GUI.enabled = bef;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.PropertyField(cacheCapacity, new GUIContent("缓存大小"));
        EditorGUILayout.PropertyField(container, new GUIContent("容器"));
        EditorGUILayout.PropertyField(prefab, new GUIContent("预制件"));
        EditorGUI.EndDisabledGroup();
        SerializedProperty temp = prefab.GetEndProperty();
        while (!string.IsNullOrEmpty(temp.propertyPath))
        {
            EditorGUILayout.PropertyField(temp);
            temp = temp.GetEndProperty();
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}