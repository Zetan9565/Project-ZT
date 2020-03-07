using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(Quest))]
public class QuestInspector : Editor
{
    Quest quest;
    SerializedProperty _ID;
    SerializedProperty title;
    SerializedProperty description;
    SerializedProperty abandonable;
    SerializedProperty group;
    SerializedProperty questType;
    SerializedProperty repeatFrequancy;
    SerializedProperty timeUnit;
    SerializedProperty acceptConditions;
    SerializedProperty conditionRelational;
    SerializedProperty beginDialogue;
    SerializedProperty ongoingDialogue;
    SerializedProperty completeDialogue;
    SerializedProperty rewardMoney;
    SerializedProperty rewardEXP;
    SerializedProperty rewardItems;
    SerializedProperty _NPCToSubmit;
    SerializedProperty cmpltObjctvInOrder;
    SerializedProperty collectObjectives;
    SerializedProperty killObjectives;
    SerializedProperty talkObjectives;
    SerializedProperty moveObjectives;
    SerializedProperty submitObjectives;
    SerializedProperty customObjectives;

    ReorderableList acceptConditionList;

    ReorderableList rewardItemList;

    ReorderableList collectObjectiveList;
    ReorderableList killObjectiveList;
    ReorderableList talkObjectiveList;
    ReorderableList moveObjectiveList;
    ReorderableList submitObjectiveList;
    ReorderableList customObjectiveList;

    float lineHeight;
    float lineHeightSpace;

    int barIndex;

    Quest[] Quests
    {
        get => Resources.LoadAll<Quest>("");
    }

    TalkerInformation[] npcs;
    string[] npcNames;

    QuestGroup[] groups;
    string[] groupNames;

    private void OnEnable()
    {
        quest = target as Quest;
        npcs = Resources.LoadAll<TalkerInformation>("");
        npcNames = npcs.Select(x => x.name).ToArray();//Linq分离出NPC名字
        groups = Resources.LoadAll<QuestGroup>("");
        groupNames = groups.Select(x => x.name).ToArray();

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;
        _ID = serializedObject.FindProperty("_ID");
        title = serializedObject.FindProperty("title");
        description = serializedObject.FindProperty("description");
        abandonable = serializedObject.FindProperty("abandonable");
        group = serializedObject.FindProperty("group");
        questType = serializedObject.FindProperty("questType");
        repeatFrequancy = serializedObject.FindProperty("repeatFrequancy");
        timeUnit = serializedObject.FindProperty("timeUnit");
        acceptConditions = serializedObject.FindProperty("acceptConditions");
        conditionRelational = serializedObject.FindProperty("conditionRelational");
        beginDialogue = serializedObject.FindProperty("beginDialogue");
        ongoingDialogue = serializedObject.FindProperty("ongoingDialogue");
        completeDialogue = serializedObject.FindProperty("completeDialogue");
        rewardMoney = serializedObject.FindProperty("rewardMoney");
        rewardEXP = serializedObject.FindProperty("rewardEXP");
        rewardItems = serializedObject.FindProperty("rewardItems");
        _NPCToSubmit = serializedObject.FindProperty("_NPCToSubmit");
        cmpltObjctvInOrder = serializedObject.FindProperty("cmpltObjctvInOrder");
        collectObjectives = serializedObject.FindProperty("collectObjectives");
        killObjectives = serializedObject.FindProperty("killObjectives");
        talkObjectives = serializedObject.FindProperty("talkObjectives");
        moveObjectives = serializedObject.FindProperty("moveObjectives");
        submitObjectives = serializedObject.FindProperty("submitObjectives");
        customObjectives = serializedObject.FindProperty("customObjectives");
        HandlingAcceptConditionList();
        HandlingQuestRewardItemList();
        HandlingCollectObjectiveList();
        HandlingKillObjectiveList();
        HandlingTalkObjectiveList();
        HandlingMoveObjectiveList();
        HandlingSubmitObjectiveList();
        HandlingCustomObjectiveList();
    }

    public override void OnInspectorGUI()
    {
        if (!CheckEditComplete())
            EditorGUILayout.HelpBox("该任务存在未补全信息。", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("该任务信息已完整。", MessageType.Info);
        barIndex = GUILayout.Toolbar(barIndex, new string[] { "基本", "条件", "奖励", "对话", "目标" });
        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        switch (barIndex)
        {
            case 0:
                #region case 0 基本
                EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
                if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
                {
                    if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                        EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
                    else
                        EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
                    if (GUILayout.Button("自动生成识别码"))
                    {
                        _ID.stringValue = GetAutoID();
                        EditorGUI.FocusTextInControl(null);
                    }
                }
                EditorGUILayout.PropertyField(title, new GUIContent("标题"));
                EditorGUILayout.PropertyField(description, new GUIContent("描述"));
                EditorGUILayout.PropertyField(abandonable, new GUIContent("可放弃"));
                EditorGUILayout.PropertyField(questType, new GUIContent("任务类型"));
                if (questType.enumValueIndex == (int)QuestType.Repeated)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(repeatFrequancy, new GUIContent("重复频率"));
                    EditorGUILayout.PropertyField(timeUnit, new GUIContent(string.Empty));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
                int oIndex = GetGroupIndex(group.objectReferenceValue as QuestGroup) + 1;
                List<int> indexes = new List<int>() { 0 };
                List<string> names = new List<string>() { "未指定" };
                for (int i = 1; i <= groupNames.Length; i++)
                {
                    indexes.Add(i);
                    names.Add(groupNames[i - 1]);
                }
                oIndex = EditorGUILayout.IntPopup("归属组", oIndex, names.ToArray(), indexes.ToArray());
                if (oIndex > 0 && oIndex <= groups.Length) group.objectReferenceValue = groups[oIndex - 1];
                else group.objectReferenceValue = null;
                if (group.objectReferenceValue)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(group, new GUIContent("引用资源"));
                    GUI.enabled = true;
                }
                EditorGUILayout.Space();
                oIndex = GetNPCIndex(_NPCToSubmit.objectReferenceValue as TalkerInformation) + 1;
                indexes = new List<int>() { 0 };
                names = new List<string>() { "接取处NPC" };
                for (int i = 1; i <= npcNames.Length; i++)
                {
                    indexes.Add(i);
                    names.Add(npcNames[i - 1]);
                }
                oIndex = EditorGUILayout.IntPopup("在此NPC处提交", oIndex, names.ToArray(), indexes.ToArray());
                if (oIndex > 0 && oIndex <= npcs.Length) _NPCToSubmit.objectReferenceValue = npcs[oIndex - 1];
                else _NPCToSubmit.objectReferenceValue = null;
                if (_NPCToSubmit.objectReferenceValue)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(_NPCToSubmit, new GUIContent("引用资源"));
                    GUI.enabled = true;
                }
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();//这一步一定要在DoLayoutList()之前做！否则无法修改DoList之前的数据
                #endregion
                break;
            case 1:
                #region case 1 条件
                serializedObject.Update();
                acceptConditionList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
                if (acceptConditions.arraySize > 0)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(conditionRelational, new GUIContent("条件关系表达式"));
                    GUI.enabled = false;
                    EditorGUILayout.TextArea("1、操作数为条件的序号\n2、运算符可使用 \"(\"、\")\"、\"+\"(或)、\"*\"(且)、\"~\"(非)" +
                        "\n3、未对非法输入进行处理，需规范填写\n4、例：(0 + 1) * ~2 表示满足条件0或1且不满足条件2\n5、为空时默认进行相互的“且”运算");
                    GUI.enabled = true;
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                }
                #endregion
                break;
            case 2:
                #region case 2 奖励
                EditorGUILayout.PropertyField(rewardMoney, new GUIContent("金钱奖励"));
                if (rewardMoney.intValue < 0) rewardMoney.intValue = 0;
                EditorGUILayout.PropertyField(rewardEXP, new GUIContent("经验奖励"));
                if (rewardEXP.intValue < 0) rewardEXP.intValue = 0;
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                EditorGUILayout.HelpBox("目前只设计10个道具奖励。", MessageType.Info);
                serializedObject.Update();
                rewardItemList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
                if (quest.RewardItems.Count >= 10)
                    rewardItemList.displayAdd = false;
                else rewardItemList.displayAdd = true;
                #endregion
                break;
            case 3:
                #region case 3 对话
                EditorGUILayout.PropertyField(beginDialogue, new GUIContent("开始时的对话"));
                if (quest.BeginDialogue)
                {
                    Quest find = Array.Find(Quests, x => x != quest && (x.BeginDialogue == quest.BeginDialogue || x.CompleteDialogue == quest.BeginDialogue
                                           || x.OngoingDialogue == quest.BeginDialogue));
                    if (find)
                    {
                        EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                    }
                    string dialogue = string.Empty;
                    for (int i = 0; i < quest.BeginDialogue.Words.Count; i++)
                    {
                        var words = quest.BeginDialogue.Words[i];
                        dialogue += "[" + words.TalkerName + "]说：\n-" + words.Words;
                        for (int j = 0; j < words.Options.Count; j++)
                        {
                            dialogue += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                        }
                        dialogue += i == quest.BeginDialogue.Words.Count - 1 ? string.Empty : "\n";
                    }
                    GUI.enabled = false;
                    EditorGUILayout.TextArea(dialogue);
                    GUI.enabled = true;
                }
                EditorGUILayout.PropertyField(ongoingDialogue, new GUIContent("进行中的对话"));
                if (quest.OngoingDialogue)
                {
                    Quest find = Array.Find(Quests, x => x != quest && (x.BeginDialogue == quest.OngoingDialogue || x.CompleteDialogue == quest.OngoingDialogue
                                           || x.OngoingDialogue == quest.OngoingDialogue));
                    if (find)
                    {
                        EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                    }
                    string dialogue = string.Empty;
                    for (int i = 0; i < quest.OngoingDialogue.Words.Count; i++)
                    {
                        var words = quest.OngoingDialogue.Words[i];
                        dialogue += "[" + words.TalkerName + "]说：\n-" + words.Words;
                        for (int j = 0; j < words.Options.Count; j++)
                        {
                            dialogue += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                        }
                        dialogue += i == quest.OngoingDialogue.Words.Count - 1 ? string.Empty : "\n";
                    }
                    GUI.enabled = false;
                    EditorGUILayout.TextArea(dialogue);
                    GUI.enabled = true;
                }
                EditorGUILayout.PropertyField(completeDialogue, new GUIContent("完成时的对话"));
                if (quest.CompleteDialogue)
                {
                    Quest find = Array.Find(Quests, x => x != quest && (x.BeginDialogue == quest.CompleteDialogue || x.CompleteDialogue == quest.CompleteDialogue
                                           || x.OngoingDialogue == quest.CompleteDialogue));
                    if (find)
                    {
                        EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                    }
                    string dialogue = string.Empty;
                    for (int i = 0; i < quest.CompleteDialogue.Words.Count; i++)
                    {
                        var words = quest.CompleteDialogue.Words[i];
                        dialogue += "[" + words.TalkerName + "]说：\n-" + words.Words;
                        for (int j = 0; j < words.Options.Count; j++)
                        {
                            dialogue += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                        }
                        dialogue += i == quest.CompleteDialogue.Words.Count - 1 ? string.Empty : "\n";
                    }
                    GUI.enabled = false;
                    EditorGUILayout.TextArea(dialogue);
                    GUI.enabled = true;
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                #endregion
                break;
            case 4:
                #region case 4 目标
                EditorGUILayout.PropertyField(cmpltObjctvInOrder, new GUIContent("按顺序完成目标"));
                if (quest.CmpltObjctvInOrder)
                {
                    EditorGUILayout.HelpBox("勾选此项，则勾选按顺序的目标按顺序码从小到大的顺序执行，若相同，则表示可以同时进行；" +
                        "若目标没有勾选按顺序，则表示该目标不受顺序影响。", MessageType.Info);
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();

                EditorGUILayout.PropertyField(collectObjectives, new GUIContent("收集类目标\t\t"
                    + (collectObjectives.isExpanded ? string.Empty : (collectObjectives.arraySize > 0 ? "数量：" + collectObjectives.arraySize : "无"))), false);
                if (collectObjectives.isExpanded)
                {
                    serializedObject.Update();
                    collectObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(killObjectives, new GUIContent("杀敌类目标\t\t"
                    + (killObjectives.isExpanded ? string.Empty : (killObjectives.arraySize > 0 ? "数量：" + killObjectives.arraySize : "无"))), false);
                if (killObjectives.isExpanded)
                {
                    serializedObject.Update();
                    killObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(talkObjectives, new GUIContent("谈话类目标\t\t"
                    + (talkObjectives.isExpanded ? string.Empty : (talkObjectives.arraySize > 0 ? "数量：" + talkObjectives.arraySize : "无"))), false);
                if (talkObjectives.isExpanded)
                {
                    serializedObject.Update();
                    talkObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(moveObjectives, new GUIContent("移动到点类目标\t"
                    + (moveObjectives.isExpanded ? string.Empty : (moveObjectives.arraySize > 0 ? "数量：" + moveObjectives.arraySize : "无"))), false);
                if (moveObjectives.isExpanded)
                {
                    serializedObject.Update();
                    moveObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(submitObjectives, new GUIContent("提交类目标\t\t"
                    + (submitObjectives.isExpanded ? string.Empty : (submitObjectives.arraySize > 0 ? "数量：" + submitObjectives.arraySize : "无"))), false);
                if (submitObjectives.isExpanded)
                {
                    serializedObject.Update();
                    submitObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(customObjectives, new GUIContent("自定义类目标\t"
                    + (customObjectives.isExpanded ? string.Empty : (customObjectives.arraySize > 0 ? "数量：" + customObjectives.arraySize : "无"))), false);
                if (customObjectives.isExpanded)
                {
                    serializedObject.Update();
                    customObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                #endregion
                break;
        }
    }

    void HandlingAcceptConditionList()
    {
        acceptConditionList = new ReorderableList(serializedObject, acceptConditions, true, true, true, true);
        acceptConditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (quest.AcceptConditions[index] != null)
            {
                switch (quest.AcceptConditions[index].AcceptCondition)
                {
                    case QuestCondition.CompleteQuest:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "完成任务");
                        break;
                    case QuestCondition.HasItem:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "拥有道具");
                        break;
                    case QuestCondition.LevelEquals:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级等于");
                        break;
                    /*case QuestCondition.LevelLargeOrEqualsThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级大于或等于");
                        break;*/
                    case QuestCondition.LevelLargeThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级大于");
                        break;
                    /*case QuestCondition.LevelLessOrEqualsThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级小于或等于");
                        break;*/
                    case QuestCondition.LevelLessThen:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级小于");
                        break;
                    case QuestCondition.TriggerSet:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器开启");
                        break;
                    case QuestCondition.TriggerReset:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器关闭");
                        break;
                    default:
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "未定义条件");
                        break;
                }
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty acceptCondition = acceptConditions.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                acceptCondition.FindPropertyRelative("acceptCondition"), new GUIContent(string.Empty), true);
            SerializedProperty level;
            SerializedProperty completeQuest;
            SerializedProperty ownedItem;

            switch (quest.AcceptConditions[index].AcceptCondition)
            {
                case QuestCondition.CompleteQuest:
                    completeQuest = acceptCondition.FindPropertyRelative("completeQuest");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), completeQuest, new GUIContent("需完成的任务"));
                    if (completeQuest.objectReferenceValue == target) completeQuest.objectReferenceValue = null;
                    if (quest.AcceptConditions[index].CompleteQuest)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "任务标题", quest.AcceptConditions[index].CompleteQuest.Title);
                    }
                    break;
                case QuestCondition.HasItem:
                    ownedItem = acceptCondition.FindPropertyRelative("ownedItem");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), ownedItem, new GUIContent("需拥有的道具"));
                    if (quest.AcceptConditions[index].OwnedItem)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "道具名称", quest.AcceptConditions[index].OwnedItem.name);
                    }
                    break;
                case QuestCondition.LevelEquals:
                //case QuestCondition.LevelLargeOrEqualsThen:
                case QuestCondition.LevelLargeThen:
                //case QuestCondition.LevelLessOrEqualsThen:
                case QuestCondition.LevelLessThen:
                    level = acceptCondition.FindPropertyRelative("level");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), level, new GUIContent("限制的等级"));
                    if (level.intValue < 1) level.intValue = 1;
                    break;
                case QuestCondition.TriggerSet:
                case QuestCondition.TriggerReset:
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                        acceptCondition.FindPropertyRelative("triggerName"), new GUIContent("触发器名称"));
                    break;
                default: break;
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acceptConditionList.elementHeightCallback = (int index) =>
        {
            switch (quest.AcceptConditions[index].AcceptCondition)
            {
                case QuestCondition.CompleteQuest:
                    if (quest.AcceptConditions[index].CompleteQuest)
                        return 3 * lineHeightSpace;
                    else return 2 * lineHeightSpace;
                case QuestCondition.HasItem:
                    if (quest.AcceptConditions[index].OwnedItem)
                        return 3 * lineHeightSpace;
                    else return 2 * lineHeightSpace;
                case QuestCondition.LevelEquals:
                //case QuestCondition.LevelLargeOrEqualsThen:
                case QuestCondition.LevelLargeThen:
                //case QuestCondition.LevelLessOrEqualsThen:
                case QuestCondition.LevelLessThen:
                case QuestCondition.TriggerSet:
                case QuestCondition.TriggerReset:
                    return 2 * lineHeightSpace;
                default: return lineHeightSpace;
            }
        };

        acceptConditionList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.AcceptConditions.Add(new QuestAcceptCondition());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acceptConditionList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个条件吗？", "确定", "取消"))
            {
                quest.AcceptConditions.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        acceptConditionList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.AcceptConditions.FindAll(x =>
            {
                switch (x.AcceptCondition)
                {
                    case QuestCondition.CompleteQuest:
                        if (x.CompleteQuest) return false;
                        else return true;
                    case QuestCondition.HasItem:
                        if (x.OwnedItem) return false;
                        else return true;
                    case QuestCondition.LevelEquals:
                    case QuestCondition.LevelLargeThen:
                    case QuestCondition.LevelLessThen:
                        //case QuestCondition.LevelLargeOrEqualsThen:
                        //case QuestCondition.LevelLessOrEqualsThen:
                        if (x.Level > 0) return false;
                        else return true;
                    case QuestCondition.TriggerSet:
                    case QuestCondition.TriggerReset:
                        if (!string.IsNullOrEmpty(x.TriggerName)) return false;
                        else return true;
                    default: return false;
                }
            }).Count;
            EditorGUI.LabelField(rect, "接取条件列表", "数量：" + acceptConditions.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        acceptConditionList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingQuestRewardItemList()
    {
        rewardItemList = new ReorderableList(serializedObject, rewardItems, true, true, true, true);
        rewardItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (quest.RewardItems[index] != null && quest.RewardItems[index].item != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), quest.RewardItems[index].item.name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty itemInfo = rewardItems.GetArrayElementAtIndex(index);
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                amount, new GUIContent("数量"));
            if (amount.intValue < 1) amount.intValue = 1;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        rewardItemList.elementHeightCallback = (int index) =>
        {
            return 2 * lineHeightSpace;
        };

        rewardItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.RewardItems.Add(new ItemInfo());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        rewardItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个奖励吗？", "确定", "取消"))
            {
                quest.RewardItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        rewardItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.RewardItems.FindAll(x => !x.item).Count;
            EditorGUI.LabelField(rect, "道具奖励列表", "数量：" + rewardItems.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        rewardItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingCollectObjectiveList()
    {
        collectObjectiveList = new ReorderableList(serializedObject, collectObjectives, true, true, true, true);
        collectObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = collectObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty item = objective.FindPropertyRelative("item");
            SerializedProperty checkBagAtStart = objective.FindPropertyRelative("checkBagAtStart");
            SerializedProperty loseItemAtSbmt = objective.FindPropertyRelative("loseItemAtSbmt");

            if (quest.CollectObjectives[index] != null)
            {
                if (quest.CollectObjectives[index].Display)
                    if (!string.IsNullOrEmpty(quest.CollectObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 3 / 4, lineHeight), objective,
                            new GUIContent(quest.CollectObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 3 / 4, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 3 / 4, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 80, rect.y, 80, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    amount, new GUIContent("目标数量"));
                if (amount.intValue < 1) amount.intValue = 1;
                lineCount++;
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    item, new GUIContent("目标道具"));
                lineCount++;
                if (quest.CollectObjectives[index].Item)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        "道具名称", quest.CollectObjectives[index].Item.name);
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    checkBagAtStart, new GUIContent("开始进行时检查数量"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    loseItemAtSbmt, new GUIContent("提交时失去相应道具"));
                lineCount++;
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        collectObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = collectObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;
            if (objective.isExpanded)
            {
                lineCount++;//目标数量
                if (cmpltObjctvInOrder.boolValue)
                    lineCount++;// 按顺序
                lineCount += 1;//顺序码
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 3;//目标道具、接取时检查、提交时失去
                if (quest.CollectObjectives[index].Item)
                    lineCount += 1;//道具名称
            }
            return lineCount * lineHeightSpace;
        };

        collectObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.CollectObjectives.Add(new CollectObjective());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        collectObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.CollectObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
            {
                quest.CollectObjectives.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        collectObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.CollectObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || !x.Item).Count;
            EditorGUI.LabelField(rect, "收集类目标列表", "数量：" + collectObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        collectObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingKillObjectiveList()
    {
        killObjectiveList = new ReorderableList(serializedObject, killObjectives, true, true, true, true);
        killObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = killObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty objectiveType = objective.FindPropertyRelative("objectiveType");
            SerializedProperty enemy = objective.FindPropertyRelative("enemy");
            SerializedProperty race = objective.FindPropertyRelative("race");

            if (quest.KillObjectives[index] != null)
            {
                if (quest.KillObjectives[index].Display)
                    if (!string.IsNullOrEmpty(quest.KillObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective,
                            new GUIContent(quest.KillObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    amount, new GUIContent("目标数量"));
                lineCount++;
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), objectiveType, new GUIContent("目标类型"));
                lineCount++;
                switch (objectiveType.enumValueIndex)
                {
                    case (int)KillObjectiveType.Specific:
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            enemy, new GUIContent("目标敌人"));
                        lineCount++;
                        if (quest.KillObjectives[index].Enemy)
                        {
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                "敌人名称", quest.KillObjectives[index].Enemy.name);
                            lineCount++;
                        }
                        break;
                    case (int)KillObjectiveType.Race:
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            race, new GUIContent("目标种族"));
                        lineCount++;
                        if (quest.KillObjectives[index].Race)
                        {
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                "种族名称", quest.KillObjectives[index].Race.name);
                            lineCount++;
                        }
                        break;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        killObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = killObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;//头
            if (objective.isExpanded)
            {
                lineCount++;//目标数量
                if (cmpltObjctvInOrder.boolValue)
                    lineCount++;//按顺序
                lineCount += 1;//顺序码
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 1;//目标类型
                switch (objective.FindPropertyRelative("objectiveType").enumValueIndex)
                {
                    case (int)KillObjectiveType.Specific:
                        lineCount += 1;//目标敌人
                        if (objective.FindPropertyRelative("enemy").objectReferenceValue)
                            lineCount += 1;//敌人名称
                        break;
                    case (int)KillObjectiveType.Race:
                        lineCount += 1;//目标种族
                        if (objective.FindPropertyRelative("race").objectReferenceValue)
                            lineCount += 1;//种族名称
                        break;
                }
                lineCount++;//图标
            }
            return lineCount * lineHeightSpace;
        };

        killObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.KillObjectives.Add(new KillObjective());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        killObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.KillObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
            {
                quest.KillObjectives.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        killObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.KillObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || !x.Enemy).Count;
            EditorGUI.LabelField(rect, "杀敌类目标列表", "数量：" + killObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        killObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingTalkObjectiveList()
    {
        talkObjectiveList = new ReorderableList(serializedObject, talkObjectives, true, true, true, true);
        talkObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = talkObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty _NPCToTalk = objective.FindPropertyRelative("_NPCToTalk");
            SerializedProperty dialogue = objective.FindPropertyRelative("dialogue");
            SerializedProperty cmpltOnlyWhenABDC = objective.FindPropertyRelative("cmpltOnlyWhenABDC");
            if (quest.TalkObjectives[index] != null)
            {
                if (quest.TalkObjectives[index].Display)
                    if (!string.IsNullOrEmpty(quest.TalkObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective,
                            new GUIContent(quest.TalkObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                int oIndex = GetNPCIndex(_NPCToTalk.objectReferenceValue as TalkerInformation) + 1;
                List<int> indexes = new List<int>() { 0 };
                List<string> names = new List<string>() { "未指定" };
                for (int i = 1; i <= npcNames.Length; i++)
                {
                    indexes.Add(i);
                    names.Add(npcNames[i - 1]);
                }
                oIndex = EditorGUI.IntPopup(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    "与此NPC交谈", oIndex, names.ToArray(), indexes.ToArray());
                lineCount++;
                if (oIndex > 0 && oIndex <= npcs.Length) _NPCToTalk.objectReferenceValue = npcs[oIndex - 1];
                else _NPCToTalk.objectReferenceValue = null;
                if (_NPCToTalk.objectReferenceValue)
                {
                    GUI.enabled = false;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), _NPCToTalk, new GUIContent("引用资源"));
                    GUI.enabled = true;
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    dialogue, new GUIContent("交谈时的对话"));
                lineCount++;
                if (quest.TalkObjectives[index].Dialogue)
                {
                    Quest find = Array.Find(Quests, x => x != quest && x.TalkObjectives.Exists(y => y.Dialogue == quest.TalkObjectives[index].Dialogue));
                    if (find)
                    {
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight * 2.4f),
                            "已有目标使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                        lineCount += 2;
                    }
                }
                if (quest.TalkObjectives[index].Dialogue && quest.TalkObjectives[index].Dialogue.Words[0] != null)
                {
                    GUI.enabled = false;
                    EditorGUI.TextArea(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        quest.TalkObjectives[index].Dialogue.Words[0].TalkerName + "说：" + quest.TalkObjectives[index].Dialogue.Words[0].Words);
                    GUI.enabled = true;
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        talkObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = talkObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                if (cmpltObjctvInOrder.boolValue)
                    lineCount++;//按顺序
                lineCount += 2;//顺序码、目标NPC
                if (objective.FindPropertyRelative("_NPCToTalk").objectReferenceValue) lineCount++;//引用资源
                lineCount++; //交谈时对话
                if (quest.TalkObjectives[index].Dialogue && Array.Exists(Quests, x => x != quest && x.TalkObjectives.Exists(y => y.Dialogue == quest.TalkObjectives[index].Dialogue)))
                    lineCount += 2;//逻辑错误
                if (quest.TalkObjectives[index].Dialogue && quest.TalkObjectives[index].Dialogue.Words[0] != null)
                    lineCount += 1;//对话的第一句
                lineCount++;
            }
            return lineCount * lineHeightSpace;
        };

        talkObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.TalkObjectives.Add(new TalkObjective());
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        };

        talkObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.TalkObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
                quest.TalkObjectives.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        talkObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.TalkObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || !x.NPCToTalk || !x.Dialogue).Count;
            EditorGUI.LabelField(rect, "谈话类目标列表", "数量：" + talkObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        talkObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingMoveObjectiveList()
    {
        moveObjectiveList = new ReorderableList(serializedObject, moveObjectives, true, true, true, true);
        moveObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = moveObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty pointID = objective.FindPropertyRelative("pointID");

            if (quest.MoveObjectives[index] != null)
            {
                if (quest.MoveObjectives[index].Display || !quest.CmpltObjctvInOrder)
                    if (!string.IsNullOrEmpty(quest.MoveObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective,
                            new GUIContent(quest.MoveObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    pointID, new GUIContent("目标地点识别码"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        moveObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = moveObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (cmpltObjctvInOrder.boolValue)
                    lineCount++;//按顺序
                lineCount++;//顺序码
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 2;//目标地点
            }
            return lineCount * lineHeightSpace;
        };

        moveObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.MoveObjectives.Add(new MoveObjective());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        moveObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.MoveObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
            {
                quest.MoveObjectives.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        moveObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.MoveObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || string.IsNullOrEmpty(x.PointID)).Count;
            EditorGUI.LabelField(rect, "移动到点类目标列表", "数量：" + moveObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        moveObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingSubmitObjectiveList()
    {
        submitObjectiveList = new ReorderableList(serializedObject, submitObjectives, true, true, true, true);
        submitObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = submitObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty _NPCToSubmit = objective.FindPropertyRelative("_NPCToSubmit");
            SerializedProperty itemToSubmit = objective.FindPropertyRelative("itemToSubmit");
            SerializedProperty wordsWhenSubmit = objective.FindPropertyRelative("wordsWhenSubmit");
            SerializedProperty talkerType = objective.FindPropertyRelative("talkerType");

            if (quest.SubmitObjectives[index] != null)
            {
                if (quest.SubmitObjectives[index].Display)
                    if (!string.IsNullOrEmpty(quest.SubmitObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective,
                            new GUIContent(quest.SubmitObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    amount, new GUIContent("目标数量"));
                if (amount.intValue < 1) amount.intValue = 1;
                lineCount++;
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                int oIndex = GetNPCIndex(_NPCToSubmit.objectReferenceValue as TalkerInformation) + 1;
                List<int> indexes = new List<int>() { 0 };
                List<string> names = new List<string>() { "未指定" };
                for (int i = 1; i <= npcNames.Length; i++)
                {
                    indexes.Add(i);
                    names.Add(npcNames[i - 1]);
                }
                oIndex = EditorGUI.IntPopup(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    "在此NPC处提交", oIndex, names.ToArray(), indexes.ToArray());
                lineCount++;
                if (oIndex > 0 && oIndex <= npcs.Length) _NPCToSubmit.objectReferenceValue = npcs[oIndex - 1];
                else _NPCToSubmit.objectReferenceValue = null;
                if (_NPCToSubmit.objectReferenceValue)
                {
                    GUI.enabled = false;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), _NPCToSubmit, new GUIContent("引用资源"));
                    GUI.enabled = true;
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    itemToSubmit, new GUIContent("需提交的道具"));
                lineCount++;
                if (quest.SubmitObjectives[index].ItemToSubmit)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        "道具名称", quest.SubmitObjectives[index].ItemToSubmit.name);
                    lineCount++;
                }
                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    new GUIContent("提交时的说的话"), new GUIStyle() { fontStyle = FontStyle.Bold });
                lineCount++;
                wordsWhenSubmit.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    wordsWhenSubmit.stringValue);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    talkerType, new GUIContent("谁说这句话"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        submitObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = submitObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;
            if (objective.isExpanded)
            {
                lineCount++;//目标数量
                if (cmpltObjctvInOrder.boolValue)
                    lineCount++;// 按顺序
                lineCount++;//顺序码、显示顺序
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 5;//NPC、目标道具、提交对话、对话人
                if (quest.SubmitObjectives[index].NPCToSubmit)
                    lineCount += 1;//NPC名字
                if (quest.SubmitObjectives[index].ItemToSubmit)
                    lineCount += 1;//道具名称
                lineCount++;
            }
            return lineCount * lineHeightSpace;
        };

        submitObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.SubmitObjectives.Add(new SubmitObjective());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        submitObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.SubmitObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
            {
                quest.SubmitObjectives.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        submitObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.SubmitObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || !x.NPCToSubmit || !x.ItemToSubmit
                                || string.IsNullOrEmpty(x.WordsWhenSubmit)).Count;
            EditorGUI.LabelField(rect, "提交类目标列表", "数量：" + submitObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        submitObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingCustomObjectiveList()
    {
        customObjectiveList = new ReorderableList(serializedObject, customObjectives, true, true, true, true);
        customObjectiveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty objective = customObjectives.GetArrayElementAtIndex(index);
            SerializedProperty display = objective.FindPropertyRelative("display");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty triggerName = objective.FindPropertyRelative("triggerName");
            SerializedProperty checkStateAtAcpt = objective.FindPropertyRelative("checkStateAtAcpt");

            if (quest.CustomObjectives[index] != null)
            {
                if (quest.CustomObjectives[index].Display || !quest.CmpltObjctvInOrder)
                    if (!string.IsNullOrEmpty(quest.CustomObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective,
                            new GUIContent(quest.CustomObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码：" : "显示顺序：") + orderIndex.intValue);
                if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width - 15, rect.y, 10, lineHeight), display.boolValue);
            }
            else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (display.boolValue || !quest.CmpltObjctvInOrder)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    displayName, new GUIContent("标题"));
                    lineCount++;
                }
                if (cmpltObjctvInOrder.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        inOrder, new GUIContent("按顺序"));
                    lineCount++;
                }
                orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "顺序码" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    triggerName, new GUIContent("触发器名称"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    checkStateAtAcpt, new GUIContent("接取时检查状态"));
                lineCount++;
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        customObjectiveList.elementHeightCallback = (int index) =>
        {
            SerializedProperty objective = customObjectives.GetArrayElementAtIndex(index);
            int lineCount = 1;
            if (objective.isExpanded)
            {
                if (cmpltObjctvInOrder.boolValue) lineCount++;//按顺序
                lineCount++;//顺序码
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 2;//触发器、状态
            }
            return lineCount * lineHeightSpace;
        };

        customObjectiveList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            quest.CustomObjectives.Add(new CustomObjective());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        customObjectiveList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + quest.CustomObjectives[list.index].DisplayName + " ] 吗？", "确定", "取消"))
            {
                quest.CustomObjectives.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        customObjectiveList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = quest.CustomObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || string.IsNullOrEmpty(x.TriggerName)).Count;
            EditorGUI.LabelField(rect, "自定义类目标列表", "数量：" + customObjectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        customObjectiveList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        Quest[] quests = Resources.LoadAll<Quest>("");
        for (int i = 1; i < 1000; i++)
        {
            newID = "QEST" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(quests, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        Quest[] quests = Resources.LoadAll<Quest>("");

        Quest find = Array.Find(quests, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != quest || (find == quest && Array.FindAll(quests, x => x.ID == _ID.stringValue).Length > 1);
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title) || string.IsNullOrEmpty(quest.Description));

        editComplete &= !quest.AcceptConditions.Exists(x =>
        {
            switch (x.AcceptCondition)
            {
                case QuestCondition.CompleteQuest:
                    if (x.CompleteQuest) return false;
                    else return true;
                case QuestCondition.HasItem:
                    if (x.OwnedItem) return false;
                    else return true;
                case QuestCondition.LevelEquals:
                case QuestCondition.LevelLargeThen:
                case QuestCondition.LevelLessThen:
                    //case QuestCondition.LevelLargeOrEqualsThen:
                    //case QuestCondition.LevelLessOrEqualsThen:
                    if (x.Level > 0) return false;
                    else return true;
                case QuestCondition.TriggerSet:
                case QuestCondition.TriggerReset:
                    if (!string.IsNullOrEmpty(x.TriggerName)) return false;
                    else return true;
                default: return false;
            }
        });

        editComplete &= quest.BeginDialogue && quest.OngoingDialogue && quest.CompleteDialogue;

        editComplete &= !quest.RewardItems.Exists(x => x.item == null);

        editComplete &= !quest.CollectObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.Item);

        editComplete &= !quest.KillObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.Enemy);

        editComplete &= !quest.TalkObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.NPCToTalk || x.Dialogue == null);

        editComplete &= !quest.MoveObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || string.IsNullOrEmpty(x.PointID));

        editComplete &= !quest.SubmitObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.NPCToSubmit || !x.ItemToSubmit || string.IsNullOrEmpty(x.WordsWhenSubmit));

        editComplete &= !quest.CustomObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || string.IsNullOrEmpty(x.TriggerName));
        return editComplete;
    }

    int GetNPCIndex(TalkerInformation npc)
    {
        return Array.IndexOf(npcs, npc);
    }

    int GetGroupIndex(QuestGroup group)
    {
        return Array.IndexOf(groups, group);
    }
}