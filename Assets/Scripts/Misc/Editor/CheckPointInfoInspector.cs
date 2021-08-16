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
        EditorGUILayout.PropertyField(_ID, new GUIContent("ʶ����"));
        EditorGUILayout.PropertyField(targetTag, new GUIContent("�������ǩ"));
        layer.intValue = EditorGUILayout.LayerField("����", layer.intValue);
        EditorGUILayout.PropertyField(scene, new GUIContent("Ŀ�곡��"));
        EditorGUILayout.PropertyField(triggerType, new GUIContent("��ײ������"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Box)
            EditorGUILayout.PropertyField(size, new GUIContent("��ײ����С"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Circle || triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
            EditorGUILayout.PropertyField(radius, new GUIContent("��ײ���뾶"));
        if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
            EditorGUILayout.PropertyField(height, new GUIContent("��ײ���߶�"));
        EditorGUILayout.PropertyField(positions, new GUIContent("λ�ñ�"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
