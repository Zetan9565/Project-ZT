using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;

    SerializedProperty info;
    SerializedProperty questInstances;

    private void OnEnable()
    {
        talker = target as Talker;
        info = serializedObject.FindProperty("info");
        questInstances = serializedObject.FindProperty("data").FindPropertyRelative("questInstances");
    }

    public override void OnInspectorGUI()
    {
        if (talker.Info)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("NPC名字：" + talker.TalkerName);
            EditorGUILayout.LabelField("NPC识别码：" + talker.TalkerID);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("NPC信息为空！", MessageType.Error);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(info, new GUIContent("信息"));
        if (Application.isPlaying)
        {
            EditorGUILayout.PropertyField(questInstances, new GUIContent("任务实例"));
            if (questInstances.isExpanded)
            {
                EditorGUILayout.BeginVertical("Box");
                for (int i = 0; i < talker.QuestInstances.Count; i++)
                    EditorGUILayout.LabelField("实例" + i.ToString(), talker.QuestInstances[i].Title);
                EditorGUILayout.EndVertical();
            }
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}