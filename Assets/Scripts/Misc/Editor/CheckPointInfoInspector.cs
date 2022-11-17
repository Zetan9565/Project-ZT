using UnityEngine;
using UnityEditor;

namespace ZetanStudio.Editor
{
    public partial class DestinationInfoInspector : UnityEditor.Editor
    {
        SerializedProperty targetTag;
        SerializedProperty layer;
        SerializedProperty triggerType;
        SerializedProperty size;
        SerializedProperty radius;
        SerializedProperty height;

        void CheckPointEnable()
        {
            targetTag = serializedObject.FindProperty("targetTag");
            layer = serializedObject.FindProperty("layer");
            triggerType = serializedObject.FindProperty("triggerType");
            size = serializedObject.FindProperty("size");
            radius = serializedObject.FindProperty("radius");
            height = serializedObject.FindProperty("height");
        }

        void DrawCheckPoint()
        {
            EditorGUILayout.PropertyField(triggerType, new GUIContent("碰撞器类型"));
            if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Box)
                EditorGUILayout.PropertyField(size, new GUIContent("碰撞器大小"));
            if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Circle || triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
                EditorGUILayout.PropertyField(radius, new GUIContent("碰撞器半径"));
            if (triggerType.enumValueIndex == (int)CheckPointTriggerType.Capsule)
                EditorGUILayout.PropertyField(height, new GUIContent("碰撞器高度"));
            targetTag.stringValue = EditorGUILayout.TagField("检测对象标签", targetTag.stringValue);
            layer.intValue = EditorGUILayout.LayerField("检测层", layer.intValue);
        }
    }
}