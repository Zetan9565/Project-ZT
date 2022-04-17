using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DestinationInformation), true)]
[CanEditMultipleObjects]
public partial class DestinationInfoInspector : Editor
{
    SerializedProperty _ID;
    SerializedProperty scene;
    SerializedProperty positions;

    SceneSelectionDrawer sceneSelector;

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        sceneSelector.DoLayoutDraw();
        EditorGUILayout.PropertyField(positions, new GUIContent("位置表"));
        if (target is CheckPointInformation)
            DrawCheckPoint();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        _ID = serializedObject.FindProperty("_ID");
        scene = serializedObject.FindProperty("scene");
        positions = serializedObject.FindProperty("positions");
        sceneSelector = new SceneSelectionDrawer(scene,"场景");
        if (target is CheckPointInformation)
            CheckPointEnable();
    }
}