﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BackpackUI))]
public class BackpackUIInspector : Editor
{
    SerializedProperty window;
    SerializedProperty pageSelector;
    SerializedProperty itemCellPrefab;
    SerializedProperty itemCellsParent;
    SerializedProperty money;
    SerializedProperty weight;
    SerializedProperty size;

    SerializedProperty closeButton;
    SerializedProperty sortButton;
    SerializedProperty handworkButton;

    SerializedProperty discardButton;
    SerializedProperty gridScrollRect;
    SerializedProperty gridMask;

    SerializedProperty searchInput;
    SerializedProperty searchButton;

    private void OnEnable()
    {
        window = serializedObject.FindProperty("window");
        pageSelector = serializedObject.FindProperty("pageSelector");
        itemCellPrefab = serializedObject.FindProperty("itemCellPrefab");
        itemCellsParent = serializedObject.FindProperty("itemCellsParent");
        money = serializedObject.FindProperty("money");
        weight = serializedObject.FindProperty("weight");
        size = serializedObject.FindProperty("size");
        closeButton = serializedObject.FindProperty("closeButton");
        sortButton = serializedObject.FindProperty("sortButton");
        handworkButton = serializedObject.FindProperty("handworkButton");
        discardButton = serializedObject.FindProperty("discardButton");
        gridScrollRect = serializedObject.FindProperty("gridScrollRect");
        gridMask = serializedObject.FindProperty("gridMask");
        searchInput = serializedObject.FindProperty("searchInput");
        searchButton = serializedObject.FindProperty("searchButton");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(window, new GUIContent("背包窗口"));
        EditorGUILayout.PropertyField(pageSelector, new GUIContent("换页下拉"));
        EditorGUILayout.PropertyField(itemCellPrefab, new GUIContent("单元格预制件"));
        EditorGUILayout.PropertyField(itemCellsParent, new GUIContent("单元格放置根"));
        EditorGUILayout.PropertyField(money, new GUIContent("金钱文字"));
        EditorGUILayout.PropertyField(weight, new GUIContent("负重文字"));
        EditorGUILayout.PropertyField(size, new GUIContent("空间文字"));
        EditorGUILayout.PropertyField(closeButton, new GUIContent("关闭按钮"));
        EditorGUILayout.PropertyField(sortButton, new GUIContent("整理按钮"));
        EditorGUILayout.PropertyField(handworkButton, new GUIContent("制作按钮"));
        EditorGUILayout.PropertyField(discardButton, new GUIContent("丢弃按钮"));
        EditorGUILayout.PropertyField(searchInput, new GUIContent("查找输入"));
        EditorGUILayout.PropertyField(searchButton, new GUIContent("查找按钮"));
        EditorGUILayout.PropertyField(gridMask, new GUIContent("滚动视图遮罩"));
        EditorGUILayout.PropertyField(gridScrollRect, new GUIContent("滚动视图"));
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
