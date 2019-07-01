using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkillActionBehaviour))]
public class SkillSMBInspector : Editor
{
    SerializedProperty parentSkill;
    SerializedProperty actionIndex;

    private void OnEnable()
    {
        parentSkill = serializedObject.FindProperty("parentSkill");
        actionIndex = serializedObject.FindProperty("actionIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(parentSkill, new GUIContent("归属技能"));
        if (Application.isPlaying) GUI.enabled = false;
        if (parentSkill.objectReferenceValue)
        {
            actionIndex.intValue = EditorGUILayout.IntSlider("招式序号", actionIndex.intValue, -1, (parentSkill.objectReferenceValue as SkillInfomation).SkillActions.Count - 1);
            EditorGUILayout.HelpBox("务必严格对应动画顺序", MessageType.Info);
        }
        if (Application.isPlaying) GUI.enabled = true;
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}