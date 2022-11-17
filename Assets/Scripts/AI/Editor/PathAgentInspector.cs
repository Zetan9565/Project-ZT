using UnityEditor;
using UnityEngine;
using ZetanStudio;

[CustomEditor(typeof(PathAgent))]
public class PathAgentInspector : Editor
{
    SerializedProperty stoppingDistance;
    SerializedProperty pickNextWaypointDist;
    SerializedProperty autoRepath;
    SerializedProperty repathRate;
    SerializedProperty showSeeker;
    SerializedProperty gizmosPath;
    SerializedProperty gizmosDetail;

    private void OnEnable()
    {
        stoppingDistance = serializedObject.FindProperty("stoppingDistance");
        pickNextWaypointDist = serializedObject.FindProperty("pickNextWaypointDist");
        autoRepath = serializedObject.FindProperty("autoRepath");
        repathRate = serializedObject.FindProperty("repathRate");
        showSeeker = serializedObject.FindProperty("showSeeker");
        gizmosPath = serializedObject.FindProperty("gizmosPath");
        gizmosDetail = serializedObject.FindProperty("gizmosDetail");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(stoppingDistance);
        EditorGUILayout.PropertyField(pickNextWaypointDist);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(autoRepath);
        EditorGUILayout.PropertyField(showSeeker);
        EditorGUILayout.EndHorizontal();
        if (autoRepath.boolValue)
            EditorGUILayout.PropertyField(repathRate);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(gizmosPath);
        EditorGUILayout.PropertyField(gizmosDetail);
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}