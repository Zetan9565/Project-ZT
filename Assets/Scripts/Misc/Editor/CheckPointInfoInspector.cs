using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CheckPointInformation))]
public class CheckPointInfoInspector : Editor
{
    SerializedProperty _ID;
    SerializedProperty targetTag;
    SerializedProperty layer;
    SerializedProperty scene;
    SerializedProperty positions;
    SerializedProperty triggerType;
    SerializedProperty size;
    SerializedProperty radius;
    SerializedProperty height;

    private void OnEnable()
    {
        _ID = serializedObject.FindProperty("_ID");
        targetTag = serializedObject.FindProperty("targetTag");
        layer = serializedObject.FindProperty("layer");
        scene = serializedObject.FindProperty("scene");
        positions = serializedObject.FindProperty("positions");
        triggerType = serializedObject.FindProperty("triggerType");
        size = serializedObject.FindProperty("size");
        radius = serializedObject.FindProperty("radius");
        height = serializedObject.FindProperty("height");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        EditorGUILayout.PropertyField(targetTag, new GUIContent("检测对象标签"));
        layer.intValue = EditorGUILayout.LayerField("检测层", layer.intValue);
        EditorGUILayout.PropertyField(scene, new GUIContent("目标场景"));
        EditorGUILayout.PropertyField(triggerType, new GUIContent("碰撞器类型"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Box)
            EditorGUILayout.PropertyField(size, new GUIContent("碰撞器大小"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Circle || triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
            EditorGUILayout.PropertyField(radius, new GUIContent("碰撞器半径"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
            EditorGUILayout.PropertyField(height, new GUIContent("碰撞器高度"));
        EditorGUILayout.PropertyField(positions, new GUIContent("位置表"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
