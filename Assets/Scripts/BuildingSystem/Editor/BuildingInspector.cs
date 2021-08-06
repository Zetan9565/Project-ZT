using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Building), true)]
public class BuildingInspector : Editor
{
    Building building;

    SerializedProperty onDestroy;

    protected virtual void OnEnable()
    {
        building = target as Building;
        onDestroy = serializedObject.FindProperty("onDestroy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("识别码", building.EntityID);
        EditorGUILayout.PropertyField(onDestroy, new GUIContent("销毁时"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}