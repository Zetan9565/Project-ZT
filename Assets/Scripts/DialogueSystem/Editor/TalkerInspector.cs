using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Talker), true)]
public class TalkerInspector : Editor
{
    Talker talker;
    QuestGiver questGiver;

    SerializedProperty info;
    SerializedProperty questsStored;
    SerializedProperty questInstances;

    ReorderableList questList;

    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        talker = target as Talker;
        info = serializedObject.FindProperty("info");
        if (talker is QuestGiver)
        {
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
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (questGiver && talker.Info)
        {
            EditorGUILayout.PropertyField(questsStored, new GUIContent("持有任务\t\t" + (questsStored.arraySize > 0 ? "数量：" + questsStored.arraySize : "无")));
            if (questsStored.isExpanded)
            {
                serializedObject.Update();
                questList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
            if (questInstances.arraySize > 0)
            {
                EditorGUILayout.PropertyField(questInstances, new GUIContent("任务实例\t\t数量：" + questInstances.arraySize));
                GUI.enabled = false;
                if (questInstances.isExpanded)
                    for (int i = 0; i < questGiver.QuestInstances.Count; i++)
                        EditorGUILayout.LabelField(questGiver.QuestInstances[i].Title);
                GUI.enabled = true;
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
                questGiver.QuestsStored.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        questList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = questGiver.QuestsStored.FindAll(x => !x).Count;
            EditorGUI.LabelField(rect, "任务列表", notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty);
        };

        questList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }
}