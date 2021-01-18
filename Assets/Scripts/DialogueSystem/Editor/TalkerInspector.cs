using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;

    SerializedProperty info;
    SerializedProperty questFlagOffset;

    TalkerInformation[] npcs;
    string[] npcNames;

    private void OnEnable()
    {
        npcs = Resources.LoadAll<TalkerInformation>("");
        npcNames = npcs.Select(x => x.name).ToArray();//Linq分离出NPC名字

        talker = target as Talker;
        info = serializedObject.FindProperty("info");
        questFlagOffset = serializedObject.FindProperty("questFlagOffset");
    }

    public override void OnInspectorGUI()
    {
        if (info.objectReferenceValue)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("NPC识别码：" + talker.TalkerID);
            EditorGUILayout.EndVertical();
        }
        else EditorGUILayout.HelpBox("NPC信息为空！", MessageType.Error);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        if (talker.Info) info.objectReferenceValue = npcs[EditorGUILayout.Popup("NPC信息", GetNPCIndex(talker.Info), npcNames)];
        else if (npcs.Length > 0) info.objectReferenceValue = npcs[EditorGUILayout.Popup("NPC信息", 0, npcNames)];
        else EditorGUILayout.Popup(0, new string[] { "无可用谈话人" });
        GUI.enabled = false;
        EditorGUILayout.PropertyField(info, new GUIContent("引用资源"));
        GUI.enabled = true;
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

    int GetNPCIndex(TalkerInformation npc)
    {
        if (npcs.Contains(npc))
            return Array.IndexOf(npcs, npc);
        else return -1;
    }
}