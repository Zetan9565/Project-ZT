using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Building), true)]
public class BuildingInspector : Editor
{
    SerializedProperty IDStarter;
    SerializedProperty IDTail;
    new SerializedProperty name;
    SerializedProperty buildingFlag;

    protected virtual void OnEnable()
    {
        IDStarter = serializedObject.FindProperty("IDStarter");
        IDTail = serializedObject.FindProperty("IDTail");
        name = serializedObject.FindProperty("name");
        buildingFlag = serializedObject.FindProperty("buildingFlag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("ID前缀", IDStarter.stringValue);
        EditorGUILayout.LabelField("ID后缀", IDTail.stringValue);
        EditorGUILayout.LabelField("名称", name.stringValue);
        EditorGUILayout.PropertyField(buildingFlag, new GUIContent("状态显示器"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
