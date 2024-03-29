using UnityEditor;
using UnityEngine;

namespace ZetanStudio.CharacterSystem.Editor
{
    [CustomEditor(typeof(SetContoller2DSpeedBehaviour))]
    public class SetContoller2DSpeedInspector : UnityEditor.Editor
    {
        SerializedProperty startTime;
        SerializedProperty endTime;
        SerializedProperty speedCurve;

        private void OnEnable()
        {
            startTime = serializedObject.FindProperty("startTime");
            endTime = serializedObject.FindProperty("endTime");
            speedCurve = serializedObject.FindProperty("speedCurve");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            float start = startTime.floatValue, end = endTime.floatValue;
            Utility.Editor.MinMaxSlider("起始时间", ref start, ref end, 0, 1);
            startTime.floatValue = start;
            endTime.floatValue = end;
            EditorGUILayout.PropertyField(speedCurve, new GUIContent("速度曲线", speedCurve.tooltip));
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}