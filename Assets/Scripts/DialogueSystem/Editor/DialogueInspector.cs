using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System;

[CustomEditor(typeof(Dialogue))]
public class DialogueInspector : Editor
{
    Dialogue dialogue;
    SerializedProperty words;
    ReorderableList wordsList;

    float lineHeight;
    float lineHeightSpace;

    TalkerInfomation[] npcs;
    string[] npcNames;

    //bool showOriginal = false;

    bool cmpltEdit;

    private void OnEnable()
    {
        npcs = Resources.LoadAll<TalkerInfomation>("");
        npcNames = npcs.Select(x => x.Name).ToArray();//Linq分离出NPC名字

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        dialogue = target as Dialogue;

        words = serializedObject.FindProperty("words");

        HandlingWordsList();
    }

    public override void OnInspectorGUI()
    {
        if (dialogue.Words.Exists(x => x.TalkerType == TalkerType.NPC && (!x.TalkerInfo || string.IsNullOrEmpty(x.Words))))
            EditorGUILayout.HelpBox("该对话存在未补全语句。", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("该对话已完整。", MessageType.Info);
        serializedObject.Update();
        //EditorGUI.BeginChangeCheck();
        wordsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        /*showOriginal = EditorGUILayout.Toggle("显示原始列表", showOriginal);
        if (showOriginal)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(words, new GUIContent("对话列表(原始)"), true);
            GUI.enabled = true;
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();*/
    }

    void HandlingWordsList()
    {
        wordsList = new ReorderableList(serializedObject, words, true, true, true, true);

        wordsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            SerializedProperty wordsSP = this.words.GetArrayElementAtIndex(index);
            SerializedProperty talkerType = wordsSP.FindPropertyRelative("talkerType");
            SerializedProperty talkerInfo = wordsSP.FindPropertyRelative("talkerInfo");
            SerializedProperty words = wordsSP.FindPropertyRelative("words");

            EditorGUI.BeginChangeCheck();
            string talkerName;
            if (talkerType.enumValueIndex == (int)TalkerType.NPC)
                talkerName = dialogue.Words[index] == null ? "(空)" : !dialogue.Words[index].TalkerInfo ? "(空谈话人)" : dialogue.Words[index].TalkerName;
            else talkerName = "玩家";
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), talkerName);

            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, lineHeight), talkerType, new GUIContent(string.Empty));
            int lineCount = 1;
            if (talkerType.enumValueIndex == (int)TalkerType.NPC)
            {
                if (dialogue.Words[index].TalkerInfo) talkerInfo.objectReferenceValue =
                    npcs[EditorGUI.Popup(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "谈话人", GetNPCIndex(dialogue.Words[index].TalkerInfo), npcNames)];
                else if (npcs.Length > 0) talkerInfo.objectReferenceValue =
                     npcs[EditorGUI.Popup(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "谈话人", 0, npcNames)];
                else EditorGUI.Popup(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight), "谈话人", 0, new string[] { "无可用谈话人" });
                lineCount++;
                GUI.enabled = false;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    talkerInfo, new GUIContent("引用资源"));
                GUI.enabled = true;
                lineCount++;
            }
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount - lineHeight, rect.width, lineHeight * 4),
                words, new GUIContent(string.Empty));

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        wordsList.elementHeightCallback = (int index) =>
        {
            int lineCount = 1;
            if (dialogue.Words[index].TalkerType == TalkerType.NPC)
            {
                lineCount += 2;
            }
            lineCount += 3;
            return lineCount * 0.95f * lineHeightSpace;
        };

        wordsList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            dialogue.Words.Add(new DialogueWords());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        wordsList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这句话吗？", "确定", "取消"))
            {
                dialogue.Words.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        wordsList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = dialogue.Words.FindAll(x => !x.TalkerInfo || string.IsNullOrEmpty(x.Words)).Count;
            EditorGUI.LabelField(rect, "对话列表", "数量：" + dialogue.Words.Count +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        wordsList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    int GetNPCIndex(TalkerInfomation npc)
    {
        if (npcs.Contains(npc))
            return Array.IndexOf(npcs, npc);
        else return 0;
    }
}