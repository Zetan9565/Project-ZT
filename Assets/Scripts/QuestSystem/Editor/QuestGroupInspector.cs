using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestGroup))]
public class QuestGroupInspector : Editor
{
    QuestGroup group;

    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty questsInGoup;

    private void OnEnable()
    {
        group = target as QuestGroup;

        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        questsInGoup = serializedObject.FindProperty("questsInGoup");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("组名"));
        ShowGroupQuests();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    private string GetAutoID()
    {
        string newID = string.Empty;
        QuestGroup[] groups = Resources.LoadAll<QuestGroup>("");
        for (int i = 1; i < 1000; i++)
        {
            newID = "QGRP" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(groups, x => x.ID == newID))
                break;
        }
        return newID;
    }

    private bool ExistsID()
    {
        List<QuestGroup> groups = new List<QuestGroup>();
        foreach (QuestGroup item in Resources.LoadAll<QuestGroup>(""))
        {
            groups.Add(item);
        }

        QuestGroup find = groups.Find(x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象虽是原对象但同ID超过一个 时为true
        return find != group || (find == group && groups.FindAll(x => x.ID == _ID.stringValue).Count > 1);
    }

    private void ShowGroupQuests()
    {
        List<Quest> quests = new List<Quest>();
        foreach (Quest quest in Resources.LoadAll<Quest>(""))
        {
            if (quest.Group == group)
                quests.Add(quest);
        }

        if (quests.Count > 0)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("组内任务");
            foreach (Quest quest in quests)
            {
                EditorGUILayout.LabelField(quest.Title);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
