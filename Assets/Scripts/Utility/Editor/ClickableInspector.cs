using UnityEditor;
using UnityEngine;
using ZetanStudio.UI;

[CustomEditor(typeof(Clickable))]
public class ClickableInspector : Editor
{
    SerializedProperty isEnabled;
    SerializedProperty doubleClickInterval;
    SerializedProperty longPressTime;
    SerializedProperty onClick;
    SerializedProperty onRightClick;
    SerializedProperty onDoubleClick;
    SerializedProperty onLongPress;

    private void OnEnable()
    {
        isEnabled = serializedObject.FindProperty("isEnabled");
        doubleClickInterval = serializedObject.FindProperty("doubleClickInterval");
        longPressTime = serializedObject.FindProperty("longPressTime");
        onClick = serializedObject.FindProperty("onClick");
        onRightClick = serializedObject.FindProperty("onRightClick");
        onDoubleClick = serializedObject.FindProperty("onDoubleClick");
        onLongPress = serializedObject.FindProperty("onLongPress");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(isEnabled, new GUIContent("启用"));
        if (isEnabled.boolValue)
        {
            EditorGUILayout.PropertyField(doubleClickInterval, new GUIContent("双击间隔"));
            EditorGUILayout.PropertyField(longPressTime, new GUIContent("长按耗时"));
            EditorGUILayout.PropertyField(onClick, new GUIContent("左键点击时"));
            EditorGUILayout.PropertyField(onRightClick, new GUIContent("右键点击时"));
            EditorGUILayout.PropertyField(onDoubleClick, new GUIContent("左键双击时"));
            EditorGUILayout.PropertyField(onLongPress, new GUIContent("左键长按时"));
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}