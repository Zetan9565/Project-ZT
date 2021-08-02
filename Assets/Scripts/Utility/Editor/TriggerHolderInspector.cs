using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerHolder))]
public class TriggerHolderInspector : Editor
{
    SerializedProperty triggerName;
    SerializedProperty setStateAtFirst;
    SerializedProperty originalState;
    SerializedProperty triggerSetActions;
    SerializedProperty triggerResetActions;

    private void OnEnable()
    {
        triggerName = serializedObject.FindProperty("triggerName");
        setStateAtFirst = serializedObject.FindProperty("setStateAtFirst");
        originalState = serializedObject.FindProperty("originalState");
        triggerSetActions = serializedObject.FindProperty("triggerSetActions");
        triggerResetActions = serializedObject.FindProperty("triggerResetActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(triggerName, new GUIContent("触发器名称"));
        if (string.IsNullOrEmpty(triggerName.stringValue)) EditorGUILayout.HelpBox("触发器名称不能为空！", MessageType.Error);
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(setStateAtFirst, new GUIContent("初始化时置位"));
        if (setStateAtFirst.boolValue) EditorGUILayout.PropertyField(originalState, new GUIContent("初始触发状态"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(triggerSetActions, new GUIContent("触发器置位行为"));
        EditorGUILayout.PropertyField(triggerResetActions, new GUIContent("触发器复位行为"));
        if (Application.isPlaying) GUI.enabled = true;
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
