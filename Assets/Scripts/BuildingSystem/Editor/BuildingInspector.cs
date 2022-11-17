using UnityEngine;
using UnityEditor;
using ZetanStudio.StructureSystem;

[CustomEditor(typeof(Structure2D), true)]
public class BuildingInspector : Editor
{
    Structure2D building;

    SerializedProperty onDestroy;

    protected virtual void OnEnable()
    {
        building = target as Structure2D;
        onDestroy = serializedObject.FindProperty("onDestroy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("识别码", building.EntityID);
        EditorGUILayout.PropertyField(onDestroy, new GUIContent("销毁时"));
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}