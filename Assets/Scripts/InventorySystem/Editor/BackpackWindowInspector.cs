using UnityEditor;
using UnityEngine;
using ZetanStudio.InventorySystem.UI;

[CustomEditor(typeof(BackpackWindow))]
public class BackpackWindowInspector : WindowInspector
{
    SerializedProperty pageSelector;
    SerializedProperty money;
    SerializedProperty weight;
    SerializedProperty size;

    SerializedProperty sortButton;
    SerializedProperty handworkButton;

    SerializedProperty discardButton;
    SerializedProperty grid;

    SerializedProperty searchInput;
    SerializedProperty searchButton;

    protected override void EnableOther()
    {
        pageSelector = serializedObject.FindProperty("pageSelector");
        money = serializedObject.FindProperty("money");
        weight = serializedObject.FindProperty("weight");
        size = serializedObject.FindProperty("size");
        sortButton = serializedObject.FindProperty("sortButton");
        handworkButton = serializedObject.FindProperty("handworkButton");
        discardButton = serializedObject.FindProperty("discardButton");
        grid = serializedObject.FindProperty("grid");
        searchInput = serializedObject.FindProperty("searchInput");
        searchButton = serializedObject.FindProperty("searchButton");
    }

    protected override void InspectOther()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(pageSelector, new GUIContent("换页下拉"));
        EditorGUILayout.PropertyField(money, new GUIContent("金钱文字"));
        EditorGUILayout.PropertyField(weight, new GUIContent("负重文字"));
        EditorGUILayout.PropertyField(size, new GUIContent("空间文字"));
        EditorGUILayout.PropertyField(sortButton, new GUIContent("整理按钮"));
        EditorGUILayout.PropertyField(handworkButton, new GUIContent("制作按钮"));
        EditorGUILayout.PropertyField(discardButton, new GUIContent("丢弃按钮"));
        EditorGUILayout.PropertyField(searchInput, new GUIContent("查找输入"));
        EditorGUILayout.PropertyField(searchButton, new GUIContent("查找按钮"));
        EditorGUILayout.PropertyField(grid, new GUIContent("滚动视图"));
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
