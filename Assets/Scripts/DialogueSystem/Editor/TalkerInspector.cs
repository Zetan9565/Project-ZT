using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;

    SerializedProperty questFlagOffset;

    private void OnEnable()
    {
        talker = target as Talker;
        questFlagOffset = serializedObject.FindProperty("questFlagOffset");
    }

    public override void OnInspectorGUI()
    {
        if (talker.Data)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("NPC识别码：" + talker.TalkerID);
            EditorGUILayout.LabelField("NPC名字：" + talker.TalkerName);
            EditorGUILayout.EndVertical();
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(questFlagOffset, new GUIContent("任务状态器位置偏移"));
        if (Application.isPlaying && talker.QuestInstances != null && talker.QuestInstances.Count > 0)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("任务实例", new GUIStyle { fontStyle = FontStyle.Bold });
            for (int i = 0; i < talker.QuestInstances.Count; i++)
                EditorGUILayout.LabelField("实例 " + i.ToString(), talker.QuestInstances[i].Info.Title);
            EditorGUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
    }
}