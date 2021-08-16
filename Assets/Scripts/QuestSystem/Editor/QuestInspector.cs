using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor.AnimatedValues;

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
    SerializedProperty acceptCondition;
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

    ConditionGroupDrawer acceptConditionDrawer;

    ItemAmountListDrawer rewardDrawer;

    ReorderableList collectObjectiveList;
    ReorderableList killObjectiveList;
    ReorderableList talkObjectiveList;
    ReorderableList moveObjectiveList;
    ReorderableList submitObjectiveList;
    ReorderableList customObjectiveList;

    float lineHeight;
    float lineHeightSpace;

    int barIndex;

    Quest[] allQuests;

    TalkerInformation[] npcs;
    string[] npcNames;

    QuestGroup[] groups;
    string[] groupNames;

    TalkerInformation holder;

    AnimBool[] showStates;

    CharacterSelectionDrawer<TalkerInformation> npcSelector;

    private void OnEnable()
    {
        allQuests = Resources.LoadAll<Quest>("Configuration");
        quest = target as Quest;
        npcs = Resources.LoadAll<TalkerInformation>("Configuration");
        npcNames = npcs.Select(x => x.name).ToArray();//Linq分离出NPC名字
        groups = Resources.LoadAll<QuestGroup>("Configuration");
        groupNames = groups.Select(x => x.name).ToArray();
        holder = Resources.LoadAll<TalkerInformation>("Configuration").FirstOrDefault(x => x.QuestsStored.Contains(quest));

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;
        _ID = serializedObject.FindProperty("_ID");
        title = serializedObject.FindProperty("title");
        description = serializedObject.FindProperty("description");
        abandonable = serializedObject.FindProperty("abandonable");
        group = serializedObject.FindProperty("group");
        questType = serializedObject.FindProperty("questType");
        repeatFrequancy = serializedObject.FindProperty("repeatFrequancy");
        timeUnit = serializedObject.FindProperty("timeUnit");
        acceptCondition = serializedObject.FindProperty("acceptCondition");
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
        npcSelector = new CharacterSelectionDrawer<TalkerInformation>(serializedObject, _NPCToSubmit);
        acceptConditionDrawer = new ConditionGroupDrawer(serializedObject, acceptCondition, lineHeight, lineHeightSpace, "接取条件列表");
        rewardDrawer = new ItemAmountListDrawer(serializedObject, rewardItems, lineHeight, lineHeightSpace, "奖励列表");
        HandlingCollectObjectiveList();
        HandlingKillObjectiveList();
        HandlingTalkObjectiveList();
        HandlingMoveObjectiveList();
        HandlingSubmitObjectiveList();
        HandlingCustomObjectiveList();
        showStates = new AnimBool[6];
        showStates[0] = new AnimBool(collectObjectives.isExpanded);
        showStates[1] = new AnimBool(killObjectives.isExpanded);
        showStates[2] = new AnimBool(talkObjectives.isExpanded);
        showStates[3] = new AnimBool(moveObjectives.isExpanded);
        showStates[4] = new AnimBool(submitObjectives.isExpanded);
        showStates[5] = new AnimBool(customObjectives.isExpanded);
        AddAnimaListener(OnAnima);
    }

    private void OnDisable()
    {
        RemoveAnimaListener(OnAnima);
    }

    private void OnAnima()
    {
        Repaint();
        collectObjectives.isExpanded = showStates[0].target;
        killObjectives.isExpanded = showStates[1].target;
        talkObjectives.isExpanded = showStates[2].target;
        moveObjectives.isExpanded = showStates[3].target;
        submitObjectives.isExpanded = showStates[4].target;
        customObjectives.isExpanded = showStates[5].target;
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
                if (string.IsNullOrEmpty(_ID.stringValue) || Quest.IsIDDuplicate(quest, allQuests))
                {
                    if (!string.IsNullOrEmpty(_ID.stringValue) && Quest.IsIDDuplicate(quest, allQuests))
                        EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
                    else
                        EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
                    if (GUILayout.Button("自动生成识别码"))
                    {
                        _ID.stringValue = Quest.GetAutoID();
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
                npcSelector.DoLayoutDraw("在此NPC处提交", "接取处NPC");
                EditorGUILayout.PropertyField(_NPCToSubmit, new GUIContent("引用资源"));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();//这一步一定要在DoLayoutList()之前做！否则无法修改DoList之前的数据
                if (holder)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(new GUIContent("持有该任务的NPC"), holder, typeof(TalkerInformation), false);
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
                break;
            case 1:
                #region case 1 条件
                acceptConditionDrawer.DoLayoutDraw();
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
                rewardDrawer.DoLayoutDraw();
                rewardDrawer.List.displayAdd = rewardItems.arraySize < 10;
                #endregion
                break;
            case 3:
                #region case 3 对话
                EditorGUILayout.PropertyField(beginDialogue, new GUIContent("开始时的对话"));
                if (beginDialogue.objectReferenceValue)
                {
                    if (GUILayout.Button("编辑"))
                    {
                        DialogueEditor.CreateWindow(beginDialogue.objectReferenceValue as Dialogue);
                    }
                    if (completeDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || beginDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("进行时或完成时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(allQuests, x => x != quest && (x.BeginDialogue == beginDialogue.objectReferenceValue || x.CompleteDialogue == beginDialogue.objectReferenceValue
                                               || x.OngoingDialogue == beginDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
                    PreviewDialogue(beginDialogue.objectReferenceValue as Dialogue);
                }
                else
                {
                    NewDialogueFor(beginDialogue);
                }
                EditorGUILayout.PropertyField(ongoingDialogue, new GUIContent("进行中的对话"));
                if (ongoingDialogue.objectReferenceValue)
                {
                    if (GUILayout.Button("编辑"))
                    {
                        DialogueEditor.CreateWindow(ongoingDialogue.objectReferenceValue as Dialogue);
                    }
                    if (ongoingDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || completeDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("开始时或完成时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(allQuests, x => x != quest && (x.BeginDialogue == ongoingDialogue.objectReferenceValue || x.CompleteDialogue == ongoingDialogue.objectReferenceValue
                                               || x.OngoingDialogue == ongoingDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
                    PreviewDialogue(ongoingDialogue.objectReferenceValue as Dialogue);
                }
                else
                {
                    NewDialogueFor(ongoingDialogue);
                }
                EditorGUILayout.PropertyField(completeDialogue, new GUIContent("完成时的对话"));
                if (completeDialogue.objectReferenceValue)
                {
                    if (GUILayout.Button("编辑"))
                    {
                        DialogueEditor.CreateWindow(completeDialogue.objectReferenceValue as Dialogue);
                    }
                    if (completeDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || completeDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("开始时或进行时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(allQuests, x => x != quest && (x.BeginDialogue == completeDialogue.objectReferenceValue || x.CompleteDialogue == completeDialogue.objectReferenceValue
                                             || x.OngoingDialogue == completeDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
                    PreviewDialogue(completeDialogue.objectReferenceValue as Dialogue);
                }
                else
                {
                    NewDialogueFor(completeDialogue);
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
                    EditorGUILayout.HelpBox("勾选此项，则勾选按顺序的目标按执行顺序从小到大的顺序执行，若相同，则表示可以同时进行；" +
                        "若目标没有勾选按顺序，则表示该目标不受顺序影响。", MessageType.Info);
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();

                EditorGUILayout.PropertyField(collectObjectives, new GUIContent("收集类目标\t\t"
                    + (collectObjectives.isExpanded ? string.Empty : (collectObjectives.arraySize > 0 ? "数量：" + collectObjectives.arraySize : "无"))), false);
                showStates[0].target = collectObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[0].faded))
                {
                    serializedObject.Update();
                    collectObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(killObjectives, new GUIContent("杀敌类目标\t\t"
                    + (killObjectives.isExpanded ? string.Empty : (killObjectives.arraySize > 0 ? "数量：" + killObjectives.arraySize : "无"))), false);
                showStates[1].target = killObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[1].faded))
                {
                    serializedObject.Update();
                    killObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(talkObjectives, new GUIContent("谈话类目标\t\t"
                    + (talkObjectives.isExpanded ? string.Empty : (talkObjectives.arraySize > 0 ? "数量：" + talkObjectives.arraySize : "无"))), false);
                showStates[2].target = talkObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[2].faded))
                {
                    serializedObject.Update();
                    talkObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(moveObjectives, new GUIContent("移动到点类目标\t"
                    + (moveObjectives.isExpanded ? string.Empty : (moveObjectives.arraySize > 0 ? "数量：" + moveObjectives.arraySize : "无"))), false);
                showStates[3].target = moveObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[3].faded))
                {
                    serializedObject.Update();
                    moveObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(submitObjectives, new GUIContent("提交类目标\t\t"
                    + (submitObjectives.isExpanded ? string.Empty : (submitObjectives.arraySize > 0 ? "数量：" + submitObjectives.arraySize : "无"))), false);
                showStates[4].target = submitObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[4].faded))
                {
                    serializedObject.Update();
                    submitObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(customObjectives, new GUIContent("自定义类目标\t"
                    + (customObjectives.isExpanded ? string.Empty : (customObjectives.arraySize > 0 ? "数量：" + customObjectives.arraySize : "无"))), false);
                showStates[5].target = customObjectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showStates[5].faded))
                {
                    serializedObject.Update();
                    customObjectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();
                #endregion
                break;
        }

        void NewDialogueFor(SerializedProperty dialogue)
        {
            if (GUILayout.Button("新建"))
            {
                if (EditorUtility.DisplayDialog("新建", "将在当前目录新建一个对话，是否继续？", "确定", "取消"))
                {
                    Dialogue dialogInstance = CreateInstance<Dialogue>();
                    AssetDatabase.CreateAsset(dialogInstance, AssetDatabase.GenerateUniqueAssetPath($"{AssetDatabase.GetAssetPath(target).Replace($"{target.name}.asset", "")}dialogue.asset"));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    dialogue.objectReferenceValue = dialogInstance;

                    DialogueEditor.CreateWindow(dialogInstance);
                }
            }
        }

        void PreviewDialogue(Dialogue dialogue)
        {
            string dialoguePreview = string.Empty;
            for (int i = 0; i < dialogue.Words.Count; i++)
            {
                var words = dialogue.Words[i];
                dialoguePreview += "[" + words.TalkerName + "]说：\n-" + MiscFuntion.HandlingContentWithKeyWords(words.Content, false, npcs);
                for (int j = 0; j < words.Options.Count; j++)
                {
                    dialoguePreview += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                }
                dialoguePreview += i == dialogue.Words.Count - 1 ? string.Empty : "\n";
            }
            GUI.enabled = false;
            EditorGUILayout.TextArea(dialoguePreview);
            GUI.enabled = true;
        }
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
                {
                    if (!string.IsNullOrEmpty(quest.CollectObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.CollectObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                }
                else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
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
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    checkBagAtStart, new GUIContent("开始进行时检查数量"));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    loseItemAtSbmt, new GUIContent("提交时失去相应道具"));
                lineCount++;
            }
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
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
                lineCount += 1;//执行顺序
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 2;//目标道具、接取时检查、提交时失去
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
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty objectiveType = objective.FindPropertyRelative("objectiveType");
            SerializedProperty enemy = objective.FindPropertyRelative("enemy");
            SerializedProperty race = objective.FindPropertyRelative("race");
            SerializedProperty group = objective.FindPropertyRelative("group");

            if (quest.KillObjectives[index] != null)
            {
                if (quest.KillObjectives[index].Display)
                    if (!string.IsNullOrEmpty(quest.KillObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.KillObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
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
                    case (int)KillObjectiveType.Group:
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            group, new GUIContent("目标组合"));
                        lineCount++;
                        if (quest.KillObjectives[index].Group)
                        {
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                "组合名称", quest.KillObjectives[index].Group.name);
                            lineCount++;
                        }
                        break;
                    default: break;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
                if (showMapIcon.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight),
                        canNavigate, new GUIContent("可导航"));
                    lineCount++;
                }
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
                lineCount += 1;//执行顺序
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
                    case (int)KillObjectiveType.Group:
                        lineCount += 1;//目标组合
                        if (objective.FindPropertyRelative("group").objectReferenceValue)
                            lineCount += 1;//组合名称
                        break;
                    default: break;
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
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
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
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.TalkObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
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
                    Quest find = Array.Find(allQuests, x => x != quest && x.TalkObjectives.Exists(y => y.Dialogue == quest.TalkObjectives[index].Dialogue));
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
                        quest.TalkObjectives[index].Dialogue.Words[0].TalkerName + "说：" + quest.TalkObjectives[index].Dialogue.Words[0].Content);
                    GUI.enabled = true;
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
                if (showMapIcon.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight),
                        canNavigate, new GUIContent("可导航"));
                    lineCount++;
                }
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
                lineCount += 2;//执行顺序、目标NPC
                if (objective.FindPropertyRelative("_NPCToTalk").objectReferenceValue) lineCount++;//引用资源
                lineCount++; //交谈时对话
                if (quest.TalkObjectives[index].Dialogue && Array.Exists(allQuests, x => x != quest && x.TalkObjectives.Exists(y => y.Dialogue == quest.TalkObjectives[index].Dialogue)))
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
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
            SerializedProperty displayName = objective.FindPropertyRelative("displayName");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty amount = objective.FindPropertyRelative("amount");
            SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
            SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
            SerializedProperty checkPoint = objective.FindPropertyRelative("checkPoint");

            if (quest.MoveObjectives[index] != null)
            {
                if (quest.MoveObjectives[index].Display || !quest.CmpltObjctvInOrder)
                    if (!string.IsNullOrEmpty(quest.MoveObjectives[index].DisplayName))
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.MoveObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
                    collectObjectives.arraySize + killObjectives.arraySize + talkObjectives.arraySize + moveObjectives.arraySize + submitObjectives.arraySize + customObjectives.arraySize);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), checkPoint, new GUIContent("目标检查点"));
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
                if (showMapIcon.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight),
                        canNavigate, new GUIContent("可导航"));
                    lineCount++;
                }
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
                lineCount++;//执行顺序
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 2;//目标地点、地图图标
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
            int notCmpltCount = quest.MoveObjectives.FindAll(x => string.IsNullOrEmpty(x.DisplayName) || !x.CheckPoint).Count;
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
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
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
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.SubmitObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
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
                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    new GUIContent("提交时的说的话"), new GUIStyle() { fontStyle = FontStyle.Bold });
                talkerType.enumValueIndex = EditorGUI.Popup(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    talkerType.enumValueIndex, new string[] { "NPC说", "玩家说" });
                lineCount++;
                wordsWhenSubmit.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    wordsWhenSubmit.stringValue);
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
                if (showMapIcon.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight),
                        canNavigate, new GUIContent("可导航"));
                    lineCount++;
                }
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
                lineCount++;//执行顺序、显示顺序
                if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                lineCount += 4;//NPC、目标道具、提交对话、对话人
                if (quest.SubmitObjectives[index].NPCToSubmit)
                    lineCount += 1;//NPC名字
                if (quest.SubmitObjectives[index].ItemToSubmit)
                    lineCount += 1;//道具名称
                lineCount++;//地图图标
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
                        EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                            new GUIContent(quest.CustomObjectives[index].DisplayName));
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(空标题)"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent("(被隐藏的目标)"));
                EditorGUI.LabelField(new Rect(rect.x + 8 + rect.width * 15 / 24, rect.y, rect.width * 24 / 15, lineHeight),
                    (cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序：" : "显示顺序：") + orderIndex.intValue);
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
                    cmpltObjctvInOrder.boolValue && inOrder.boolValue ? "执行顺序" : "显示顺序", orderIndex.intValue, 1,
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
                lineCount++;//执行顺序
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

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title) || string.IsNullOrEmpty(quest.Description));

        editComplete &= !quest.AcceptCondition.Conditions.Exists(x => !x.IsValid);

        editComplete &= quest.BeginDialogue && quest.OngoingDialogue && quest.CompleteDialogue;

        editComplete &= !quest.RewardItems.Exists(x => x.item == null);

        editComplete &= !quest.CollectObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        editComplete &= !quest.KillObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        editComplete &= !quest.TalkObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        editComplete &= !quest.MoveObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        editComplete &= !quest.SubmitObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        editComplete &= !quest.CustomObjectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

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

    public void AddAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        foreach (var state in showStates)
        {
            state.valueChanged.AddListener(callback);
        }
    }
    public void RemoveAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        foreach (var state in showStates)
        {
            state.valueChanged.RemoveListener(callback);
        }
    }

}