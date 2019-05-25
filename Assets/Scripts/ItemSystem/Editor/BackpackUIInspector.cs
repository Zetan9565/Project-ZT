using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BackpackUI))]
public class BackpackUIInspector : Editor
{
    BackpackUI UI;

    SerializedProperty backpackWindow;
    SerializedProperty tabs;
    SerializedProperty itemCellPrefab;
    SerializedProperty itemCellsParent;
    SerializedProperty money;
    SerializedProperty weight;
    SerializedProperty size;

    SerializedProperty closeButton;
    SerializedProperty sortButton;
    SerializedProperty handworkButton;

    SerializedProperty discardArea;
    SerializedProperty gridRect;
    SerializedProperty gridMask;

    private void OnEnable()
    {
        UI = target as BackpackUI;

        backpackWindow = serializedObject.FindProperty("backpackWindow");
        tabs = serializedObject.FindProperty("tabs");
        itemCellPrefab = serializedObject.FindProperty("itemCellPrefab");
        itemCellsParent = serializedObject.FindProperty("itemCellsParent");
        money = serializedObject.FindProperty("money");
        weight = serializedObject.FindProperty("weight");
        size = serializedObject.FindProperty("size");
        closeButton = serializedObject.FindProperty("closeButton");
        sortButton = serializedObject.FindProperty("sortButton");
        handworkButton = serializedObject.FindProperty("handworkButton");
        discardArea = serializedObject.FindProperty("discardArea");
        gridRect = serializedObject.FindProperty("gridRect");
        gridMask = serializedObject.FindProperty("gridMask");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(backpackWindow, new GUIContent("背包窗口"));
        EditorGUILayout.PropertyField(tabs, new GUIContent("标签"), true);
        EditorGUILayout.PropertyField(itemCellPrefab, new GUIContent("单元格预制体"));
        EditorGUILayout.PropertyField(itemCellsParent, new GUIContent("单元格放置根"));
        EditorGUILayout.PropertyField(money, new GUIContent("金钱文字"));
        EditorGUILayout.PropertyField(weight, new GUIContent("负重文字"));
        EditorGUILayout.PropertyField(size, new GUIContent("空间文字"));
        EditorGUILayout.PropertyField(closeButton, new GUIContent("关闭按钮"));
        EditorGUILayout.PropertyField(sortButton, new GUIContent("整理按钮"));
        EditorGUILayout.PropertyField(handworkButton, new GUIContent("制作按钮"));
        EditorGUILayout.PropertyField(discardArea, new GUIContent("丢弃区域"));
        EditorGUILayout.PropertyField(gridMask, new GUIContent("滚动视图遮罩"));
        EditorGUILayout.PropertyField(gridRect, new GUIContent("滚动视图"));
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
