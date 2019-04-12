using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;
    QuestGiver questGiver;

    SerializedProperty info;
    SerializedProperty dialogue;
    SerializedProperty questsStored;
    SerializedProperty questInstances;

    ReorderableList questList;

    float lineHeight;
    float lineHeightSpace;

    bool showQuestList;

    private void OnEnable()
    {
        talker = target as Talker;
        showQuestList = false;
        info = serializedObject.FindProperty("info");
        dialogue = serializedObject.FindProperty("defaultDialogue");
        if (talker is QuestGiver)
        {
            showQuestList = true;

            questsStored = serializedObject.FindProperty("questsStored");
            questInstances = serializedObject.FindProperty("questInstances");

            lineHeight = EditorGUIUtility.singleLineHeight;
            lineHeightSpace = lineHeight + 5;

            questGiver = talker as QuestGiver;

            HandlingQuestList();
        }
        else questGiver = null;
    }

    public override void OnInspectorGUI()
    {
        if (talker.Info)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("NPC名字：" + talker.Info.Name);
            EditorGUILayout.LabelField("NPC识别码：" + talker.Info.ID);
            if (questGiver) EditorGUILayout.LabelField("NPC任务数量：" + questGiver.QuestsStored.Count);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("NPC信息为空！", MessageType.Warning);
        }
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(info, new GUIContent("信息"));
        EditorGUILayout.PropertyField(dialogue, new GUIContent("默认对话"));
        if (talker.DefaultDialogue && talker.DefaultDialogue.Words != null && talker.DefaultDialogue.Words[0] != null)
        {
            //EditorGUILayout.HelpBox(talker.DefaultDialogue.Words[0].Words, MessageType.None);
            GUI.enabled = false;
            EditorGUILayout.TextArea(talker.DefaultDialogue.Words[0].Words);
            GUI.enabled = true;
        }
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (questGiver)
        {
            showQuestList = EditorGUILayout.Toggle("显示任务列表", showQuestList);
            if (showQuestList)
            {
                serializedObject.Update();
                questList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
                if (questGiver.QuestInstances.Count > 0)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(questInstances, new GUIContent("任务实例"));
                    if(questInstances.isExpanded)
                    {
                        EditorGUILayout.LabelField("任务数量", questGiver.QuestInstances.Count.ToString());
                        for (int i = 0; i < questGiver.QuestInstances.Count; i++)
                        {
                            EditorGUILayout.LabelField("任务" + (i + 1), questGiver.QuestInstances[i].Title);
                        }
                    }
                    GUI.enabled = true;
                }
            }
        }
    }

    void HandlingQuestList()
    {
        questList = new ReorderableList(serializedObject, questsStored, true, true, true, true);

        questList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (questGiver.QuestsStored[index])
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), questGiver.QuestsStored[index].Title);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            SerializedProperty quest = questsStored.GetArrayElementAtIndex(index);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), quest, new GUIContent(string.Empty));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.elementHeightCallback = (int index) =>
        {
            return 2 * lineHeightSpace;
        };

        questList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            questGiver.QuestsStored.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个任务吗？", "确定", "取消"))
            {
                questGiver.QuestsStored.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = questGiver.QuestsStored.FindAll(x => !x).Count;
            EditorGUI.LabelField(rect, "任务列表", "数量：" + questGiver.QuestsStored.Count +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        questList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }
}