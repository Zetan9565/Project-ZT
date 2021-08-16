using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriggerEvents))]
public class TriggerEventsInspector : Editor
{
    SerializedProperty activated;
    SerializedProperty _3D;
    SerializedProperty OnEnter;
    SerializedProperty OnStay;
    SerializedProperty OnExit;
    SerializedProperty OnEnter2D;
    SerializedProperty OnStay2D;
    SerializedProperty OnExit2D;

    private void OnEnable()
    {
        activated = serializedObject.FindProperty("activated");
        _3D = serializedObject.FindProperty("_3D");
        OnEnter = serializedObject.FindProperty("OnEnter");
        OnExit = serializedObject.FindProperty("OnExit");
        OnStay = serializedObject.FindProperty("OnStay");
        OnEnter2D = serializedObject.FindProperty("OnEnter2D");
        OnExit2D = serializedObject.FindProperty("OnExit2D");
        OnStay2D = serializedObject.FindProperty("OnStay2D");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(activated, new GUIContent("启用"));
        EditorGUILayout.PropertyField(_3D, new GUIContent("3D碰撞器"));
        if (_3D.boolValue)
        {
            EditorGUILayout.PropertyField(OnEnter, new GUIContent("进入碰撞器时"));
            EditorGUILayout.PropertyField(OnStay, new GUIContent("停留碰撞器时"));
            EditorGUILayout.PropertyField(OnExit, new GUIContent("离开碰撞器时"));
        }
        else
        {
            EditorGUILayout.PropertyField(OnEnter2D, new GUIContent("进入碰撞器时"));
            EditorGUILayout.PropertyField(OnStay2D, new GUIContent("停留碰撞器时"));
            EditorGUILayout.PropertyField(OnExit2D, new GUIContent("离开碰撞器时"));
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}