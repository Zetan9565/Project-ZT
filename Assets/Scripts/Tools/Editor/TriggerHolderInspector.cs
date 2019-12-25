using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerHolder))]
public class TriggerHolderInspector : Editor
{
    SerializedProperty triggerName;
    SerializedProperty setStateAtFirst;
    SerializedProperty originalState;
    SerializedProperty onTriggerSet;
    SerializedProperty onTriggerReset;

    private void OnEnable()
    {
        triggerName = serializedObject.FindProperty("triggerName");
        setStateAtFirst = serializedObject.FindProperty("setStateAtFirst");
        originalState = serializedObject.FindProperty("originalState");
        onTriggerSet = serializedObject.FindProperty("onTriggerSet");
        onTriggerReset = serializedObject.FindProperty("onTriggerReset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(triggerName, new GUIContent("触发器名称"));
        if (string.IsNullOrEmpty(triggerName.stringValue)) EditorGUILayout.HelpBox("触发器名称不能为空！", MessageType.Error);
        if (Application.isPlaying) GUI.enabled = false;
        EditorGUILayout.PropertyField(setStateAtFirst, new GUIContent("初始化时触发"));
        if (Application.isPlaying) GUI.enabled = true;
        if (setStateAtFirst.boolValue) EditorGUILayout.PropertyField(originalState, new GUIContent("初始触发状态"));
        EditorGUILayout.PropertyField(onTriggerSet, new GUIContent("触发器置位事件"));
        EditorGUILayout.PropertyField(onTriggerReset, new GUIContent("触发器复位事件"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
