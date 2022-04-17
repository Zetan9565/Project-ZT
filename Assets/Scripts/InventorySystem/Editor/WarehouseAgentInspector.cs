using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Warehouse))]
public class WarehouseAgentInspector : BuildingInspector
{
    SerializedProperty defaultSize;

    protected override void OnEnable()
    {
        base.OnEnable();
        defaultSize = serializedObject.FindProperty("defaultSize");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(defaultSize, new GUIContent("默认容量"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}
