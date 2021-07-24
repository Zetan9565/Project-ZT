using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Building), true)]
public class BuildingInspector : Editor
{
    Building building;

    SerializedProperty IDPrefix;
    SerializedProperty IDTail;
    SerializedProperty buildingFlagOffset;
    SerializedProperty onDestroy;

    protected virtual void OnEnable()
    {
        building = target as Building;
        IDPrefix = serializedObject.FindProperty("IDPrefix");
        IDTail = serializedObject.FindProperty("IDTail");
        buildingFlagOffset = serializedObject.FindProperty("buildingFlagOffset");
        onDestroy = serializedObject.FindProperty("onDestroy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("ID前缀", IDPrefix.stringValue);
        EditorGUILayout.LabelField("ID后缀", IDTail.stringValue);
        EditorGUILayout.LabelField("名称", building.name);
        EditorGUILayout.PropertyField(buildingFlagOffset, new GUIContent("状态显示器偏移"));
        EditorGUILayout.PropertyField(onDestroy, new GUIContent("销毁时"));
        if (target is Field) EditorGUILayout.PropertyField(serializedObject.FindProperty("collider"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
