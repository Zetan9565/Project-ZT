using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Building), true)]
public class BuildingInspector : Editor
{
    SerializedProperty IDStarter;
    SerializedProperty IDTail;
    new SerializedProperty name;
    SerializedProperty buildingFlagOffset;
    SerializedProperty onDestroy;

    protected virtual void OnEnable()
    {
        IDStarter = serializedObject.FindProperty("IDStarter");
        IDTail = serializedObject.FindProperty("IDTail");
        name = serializedObject.FindProperty("name");
        buildingFlagOffset = serializedObject.FindProperty("buildingFlagOffset");
        onDestroy = serializedObject.FindProperty("onDestroy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("ID前缀", IDStarter.stringValue);
        EditorGUILayout.LabelField("ID后缀", IDTail.stringValue);
        EditorGUILayout.LabelField("名称", name.stringValue);
        EditorGUILayout.PropertyField(buildingFlagOffset, new GUIContent("状态显示器偏移"));
        EditorGUILayout.PropertyField(onDestroy, new GUIContent("销毁时"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
