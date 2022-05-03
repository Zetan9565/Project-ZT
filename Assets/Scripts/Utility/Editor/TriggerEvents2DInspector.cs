using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerEvents2D))]
public class TriggerEvents2DInspector : Editor
{
    SerializedProperty activated;
    SerializedProperty OnEnter;
    SerializedProperty OnStay;
    SerializedProperty OnExit;

    private void OnEnable()
    {
        activated = serializedObject.FindProperty("activated");
        OnEnter = serializedObject.FindProperty("OnEnter");
        OnExit = serializedObject.FindProperty("OnExit");
        OnStay = serializedObject.FindProperty("OnStay");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(activated, new GUIContent("启用"));
        EditorGUILayout.PropertyField(OnEnter, new GUIContent("进入碰撞器时"));
        EditorGUILayout.PropertyField(OnStay, new GUIContent("停留碰撞器时"));
        EditorGUILayout.PropertyField(OnExit, new GUIContent("离开碰撞器时"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}