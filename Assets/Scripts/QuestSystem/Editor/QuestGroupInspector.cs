using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
        _Name = serializedObject.FindProperty("_name");
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
        QuestGroup[] groups = Resources.LoadAll<QuestGroup>("Configuration");
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
        QuestGroup[] groups = Resources.LoadAll<QuestGroup>("Configuration");

        QuestGroup find = Array.Find(groups, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象虽是原对象但同ID超过一个 时为true
        return find != group || (find == group && Array.FindAll(groups, x => x.ID == _ID.stringValue).Length > 1);
    }

    private void ShowGroupQuests()
    {
        Quest[] quests = Resources.LoadAll<Quest>("Configuration").Where(x=>x.Group == group).ToArray();

        if (quests.Length > 0)
        {
            Array.Sort(quests, (x, y) => string.Compare(x.ID, y.ID));

            EditorGUILayout.LabelField("组内任务");
            EditorGUILayout.BeginVertical("Box");
            foreach (Quest quest in quests)
            {
                EditorGUILayout.LabelField(quest.Title);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
