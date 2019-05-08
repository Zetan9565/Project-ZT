using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemWindowBaseUI), true)]
public class ItemWinUIInspector : Editor
{
    ItemWindowBaseUI UI;

    SerializedProperty itemWindow;
    SerializedProperty icon;
    SerializedProperty nameText;
    SerializedProperty typeText;
    SerializedProperty effectText;
    SerializedProperty priceTitle;
    SerializedProperty priceText;
    SerializedProperty weightText;
    SerializedProperty mulFunTitle;
    SerializedProperty mulFunText;

    SerializedProperty gemstone_1;
    SerializedProperty gemstone_2;

    SerializedProperty durability;

    SerializedProperty descriptionText;
    SerializedProperty closeButton;

    SerializedProperty mulFunButton;
    SerializedProperty discardButton;
    SerializedProperty buttonsArea;

    private void OnEnable()
    {
        UI = target as ItemWindowBaseUI;

        itemWindow = serializedObject.FindProperty("itemWindow");
        icon = serializedObject.FindProperty("icon");
        nameText = serializedObject.FindProperty("nameText");
        typeText = serializedObject.FindProperty("typeText");
        effectText = serializedObject.FindProperty("effectText");
        priceTitle = serializedObject.FindProperty("priceTitle");
        priceText = serializedObject.FindProperty("priceText");
        weightText = serializedObject.FindProperty("weightText");
        mulFunTitle = serializedObject.FindProperty("mulFunTitle");
        mulFunText = serializedObject.FindProperty("mulFunText");

        gemstone_1 = serializedObject.FindProperty("gemstone_1");
        gemstone_2 = serializedObject.FindProperty("gemstone_2");

        descriptionText = serializedObject.FindProperty("descriptionText");
        durability = serializedObject.FindProperty("durability");

        if (UI is ItemWindowUI)
        {
            closeButton = serializedObject.FindProperty("closeButton");

            mulFunButton = serializedObject.FindProperty("mulFunButton");
            discardButton = serializedObject.FindProperty("discardButton");
            buttonsArea = serializedObject.FindProperty("buttonsArea");
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(itemWindow, new GUIContent("物品窗口"));
        EditorGUILayout.PropertyField(icon, new GUIContent("图标"));
        EditorGUILayout.PropertyField(nameText, new GUIContent("名称文本"));
        EditorGUILayout.PropertyField(typeText, new GUIContent("类型文本"));
        EditorGUILayout.PropertyField(effectText, new GUIContent("效果文本"));
        EditorGUILayout.PropertyField(priceTitle, new GUIContent("价格标题"));
        EditorGUILayout.PropertyField(priceText, new GUIContent("价格文本"));
        EditorGUILayout.PropertyField(weightText, new GUIContent("重量文本"));
        EditorGUILayout.PropertyField(mulFunTitle, new GUIContent("多功能标题"));
        EditorGUILayout.PropertyField(mulFunText, new GUIContent("多功能文本"));
        EditorGUILayout.PropertyField(gemstone_1, new GUIContent("第一宝石槽"));
        EditorGUILayout.PropertyField(gemstone_2, new GUIContent("第二宝石槽"));
        EditorGUILayout.PropertyField(descriptionText, new GUIContent("描述文本"));
        EditorGUILayout.PropertyField(durability, new GUIContent("耐久度"));
        if (UI is ItemWindowUI)
        {
            EditorGUILayout.PropertyField(closeButton, new GUIContent("关闭按钮"));
            EditorGUILayout.PropertyField(mulFunButton, new GUIContent("多功能按钮"));
            EditorGUILayout.PropertyField(discardButton, new GUIContent("丢弃按钮"));
            EditorGUILayout.PropertyField(buttonsArea, new GUIContent("按钮区域"));
        }
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}