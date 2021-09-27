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
    SerializedProperty objectives;

    ConditionGroupDrawer acceptConditionDrawer;
    ItemAmountListDrawer rewardDrawer;
    ReorderableList objectiveList;

    float lineHeight;
    float lineHeightSpace;

    int barIndex;

    Quest[] allQuests;

    TalkerInformation[] npcs;
    ItemBase[] items;

    TalkerInformation holder;

    AnimBool[] showState;

    ObjectSelectionDrawer<TalkerInformation> npcSelector;
    ObjectSelectionDrawer<QuestGroup> groupSelector;

    private void OnEnable()
    {
        allQuests = Resources.LoadAll<Quest>("Configuration");
        quest = target as Quest;
        npcs = Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.Enable).ToArray();
        holder = npcs.FirstOrDefault(x => x.QuestsStored.Contains(quest));
        items = Resources.LoadAll<ItemBase>("Configuration");

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
        objectives = serializedObject.FindProperty("objectives");
        groupSelector = new ObjectSelectionDrawer<QuestGroup>(group, "_name", "Configuration", "归属组", "无");
        npcSelector = new ObjectSelectionDrawer<TalkerInformation>(_NPCToSubmit, "_name", npcs, "在此NPC处提交", "接取处NPC");
        acceptConditionDrawer = new ConditionGroupDrawer(serializedObject, acceptCondition, lineHeight, lineHeightSpace, "接取条件列表");
        rewardDrawer = new ItemAmountListDrawer(serializedObject, rewardItems, lineHeight, lineHeightSpace, "奖励列表");
        HandlingObjectiveList();
        showState = new AnimBool[1];
        showState[0] = new AnimBool(objectives.isExpanded);
        AddAnimaListener(OnAnima);
    }

    private void OnDisable()
    {
        RemoveAnimaListener(OnAnima);
    }

    private void OnAnima()
    {
        Repaint();
        objectives.isExpanded = showState[0].target;
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
                groupSelector.DoLayoutDraw();
                npcSelector.DoLayoutDraw();
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
                        EditorUtility.OpenPropertyEditor(beginDialogue.objectReferenceValue);
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
                        EditorUtility.OpenPropertyEditor(ongoingDialogue.objectReferenceValue);
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
                        EditorUtility.OpenPropertyEditor(completeDialogue.objectReferenceValue);
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

                EditorGUILayout.PropertyField(objectives, new GUIContent("任务目标\t\t"
                    + (objectives.isExpanded ? string.Empty : (objectives.arraySize > 0 ? "数量：" + objectives.arraySize : "无"))), false);
                showState[0].target = objectives.isExpanded;
                if (EditorGUILayout.BeginFadeGroup(showState[0].faded))
                {
                    serializedObject.Update();
                    objectiveList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.LabelField("目标显示预览");
                GUI.enabled = false;
                EditorGUILayout.TextArea(quest.GetObjectiveString());
                GUI.enabled = true;
                #endregion
                break;
        }

        void NewDialogueFor(SerializedProperty dialogue)
        {
            if (GUILayout.Button("新建"))
            {
                Dialogue dialogInstance = ZetanEditorUtility.SaveFilePanel(CreateInstance<Dialogue>, "dialogue", true);
                if (dialogInstance)
                {
                    dialogue.objectReferenceValue = dialogInstance;
                    EditorUtility.OpenPropertyEditor(dialogInstance);
                }
            }
        }

        void PreviewDialogue(Dialogue dialogue)
        {
            string dialoguePreview = string.Empty;
            for (int i = 0; i < dialogue.Words.Count; i++)
            {
                var words = dialogue.Words[i];
                dialoguePreview += "[" + words.TalkerName + "]说：\n-" + MiscFuntion.HandlingKeyWords(words.Content, false, npcs);
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

    void HandlingObjectiveList()
    {
        objectiveList = new ReorderableList(serializedObject, objectives, true, true, true, true)
        {
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty objective = objectives.GetArrayElementAtIndex(index);
                SerializedProperty display = objective.FindPropertyRelative("display");
                SerializedProperty displayName = objective.FindPropertyRelative("displayName");
                SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
                SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
                SerializedProperty auxiliaryPos = objective.FindPropertyRelative("auxiliaryPos");
                SerializedProperty amount = objective.FindPropertyRelative("amount");
                SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
                SerializedProperty orderIndex = objective.FindPropertyRelative("orderIndex");
                int type = GetObjectiveType(objective.managedReferenceFullTypename);
                string typePrefix = type switch
                {
                    0 => "[集]",
                    1 => "[杀]",
                    2 => "[谈]",
                    3 => "[移]",
                    4 => "[给]",
                    5 => "[触]",
                    _ => string.Empty,
                };
                if (objective != null)
                {
                    if (display.boolValue)
                    {
                        if (!string.IsNullOrEmpty(displayName.stringValue))
                            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective,
                                new GUIContent(typePrefix + displayName.stringValue));
                        else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent(typePrefix + "(空标题)"));
                    }
                    else EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.8f, lineHeight), objective, new GUIContent(typePrefix + "(被隐藏的目标)"));
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
                    if (type == 2 && amount.intValue > 1) amount.intValue = 1;
                    lineCount++;
                    if (cmpltObjctvInOrder.boolValue)
                    {
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            inOrder, new GUIContent("按顺序"));
                        lineCount++;
                    }
                    if (cmpltObjctvInOrder.boolValue && inOrder.boolValue)
                        orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "执行顺序", orderIndex.intValue, 1, objectives.arraySize);
                    else
                    {
                        GUI.enabled = false;
                        orderIndex.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "显示顺序", index + 1, 1, objectives.arraySize);
                        GUI.enabled = true;
                    }
                    lineCount++;
                    if (type != 5)//触发器目标没有位置功能
                    {
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
                        lineCount++;
                        if (showMapIcon.boolValue || type == 3)
                        {
                            if (showMapIcon.boolValue)
                                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * (lineCount - 1), rect.width * 0.5f, lineHeight),
                                    canNavigate, new GUIContent("可导航"));
                            if (type != 3)
                            {
                                if (type != 2 && type != 4)//交谈、提交类不需要辅助位置，会使用交谈NPC的位置代替
                                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                       auxiliaryPos, new GUIContent("辅助位置", "用于显示地图图标、导航等"));
                            }
                            else
                                auxiliaryPos.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                "检查点", auxiliaryPos.objectReferenceValue, typeof(CheckPointInformation), false);
                            if (type != 2 && type != 4) lineCount++;
                        }
                    }
                    HandlingObjectiveType();
                }
                if (!inOrder.boolValue) orderIndex.intValue = index + 1;
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

                void HandlingObjectiveType()
                {
                    switch (type)
                    {
                        case 0:
                            #region 收集类目标
                            SerializedProperty itemToCollect = objective.FindPropertyRelative("itemToCollect");
                            SerializedProperty checkBagAtStart = objective.FindPropertyRelative("checkBagAtStart");
                            SerializedProperty loseItemAtSbmt = objective.FindPropertyRelative("loseItemAtSbmt");
                            new ObjectSelectionDrawer<ItemBase>(itemToCollect, "_name",
                                                                i => ZetanUtility.GetInspectorName(i.ItemType), items,
                                                                "目标道具").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                                checkBagAtStart, new GUIContent("开始进行时检查数量"));
                            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                                loseItemAtSbmt, new GUIContent("提交时失去相应道具"));
                            lineCount++;
                            #endregion
                            break;
                        case 1:
                            #region 杀敌类目标
                            SerializedProperty killType = objective.FindPropertyRelative("killType");
                            SerializedProperty enemy = objective.FindPropertyRelative("enemy");
                            SerializedProperty race = objective.FindPropertyRelative("race");
                            SerializedProperty group = objective.FindPropertyRelative("group");
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), killType, new GUIContent("目标类型"));
                            lineCount++;
                            switch (killType.enumValueIndex)
                            {
                                case (int)KillObjectiveType.Specific:
                                    new ObjectSelectionDrawer<EnemyInformation>(enemy, "_name", e => e.Race ? e.Race.Name : string.Empty, "Configuration", "目标敌人").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                                    lineCount++;
                                    break;
                                case (int)KillObjectiveType.Race:
                                    new ObjectSelectionDrawer<EnemyRace>(race, "_name", "Configuration", "目标种族").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                                    lineCount++;
                                    break;
                                case (int)KillObjectiveType.Group:
                                    new ObjectSelectionDrawer<EnemyGroup>(group, "_name", "Configuration", "目标组合").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                                    lineCount++;
                                    break;
                                default: break;
                            }
                            #endregion
                            break;
                        case 2:
                            #region 交谈类目标
                            SerializedProperty _NPCToTalk = objective.FindPropertyRelative("_NPCToTalk");
                            SerializedProperty dialogue = objective.FindPropertyRelative("dialogue");
                            new ObjectSelectionDrawer<TalkerInformation>(_NPCToTalk, "_name", npcs, "与此NPC交谈").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), dialogue, new GUIContent("交谈时的对话"));
                            lineCount++;
                            if (dialogue.objectReferenceValue is Dialogue dialog)
                            {
                                Quest find = Array.Find(allQuests, x => x != quest && x.Objectives.Exists(y => y is TalkObjective to && to.Dialogue == dialog));
                                if (find)
                                {
                                    EditorGUI.HelpBox(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight * 2.4f),
                                        "已有目标使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                                    lineCount += 2;
                                }
                                if (dialog.Words != null && dialog.Words[0])
                                {
                                    GUI.enabled = false;
                                    EditorGUI.TextArea(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                        dialog.Words[0].TalkerName + "说：" + MiscFuntion.HandlingKeyWords(dialog.Words[0].Content));
                                    GUI.enabled = true;
                                    lineCount++;
                                }
                            }
                            #endregion;
                            break;
                        case 3:
                            #region 检查点目标
                            SerializedProperty itemToUseHere = objective.FindPropertyRelative("itemToUseHere");
                            new ObjectSelectionDrawer<ItemBase>(itemToUseHere, "_name", i => ZetanUtility.GetInspectorName(i.ItemType), items,
                                                                "需在此处使用的道具").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            #endregion
                            break;
                        case 4:
                            #region 提交类目标
                            SerializedProperty _NPCToSubmit = objective.FindPropertyRelative("_NPCToSubmit");
                            SerializedProperty itemToSubmit = objective.FindPropertyRelative("itemToSubmit");
                            SerializedProperty wordsWhenSubmit = objective.FindPropertyRelative("wordsWhenSubmit");
                            SerializedProperty talkerType = objective.FindPropertyRelative("talkerType");
                            new ObjectSelectionDrawer<TalkerInformation>(_NPCToSubmit, "_name", npcs, "与此NPC交谈").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            new ObjectSelectionDrawer<ItemBase>(itemToSubmit, "_name", i => ZetanUtility.GetInspectorName(i.ItemType), items,
                                                                "需提交的道具").DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight), new GUIContent("提交时的说的话"));
                            talkerType.enumValueIndex = EditorGUI.Popup(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                                talkerType.enumValueIndex, new string[] { "NPC说", "玩家说" });
                            lineCount++;
                            wordsWhenSubmit.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                wordsWhenSubmit.stringValue);
                            lineCount++;
                            #endregion
                            break;
                        case 5:
                            #region 自定义目标
                            SerializedProperty triggerName = objective.FindPropertyRelative("triggerName");
                            SerializedProperty stateToCheck = objective.FindPropertyRelative("stateToCheck");
                            SerializedProperty checkStateAtAcpt = objective.FindPropertyRelative("checkStateAtAcpt");
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                                triggerName, new GUIContent("触发器名称"));
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                                stateToCheck, new GUIContent("触发器置位状态"));
                            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                                checkStateAtAcpt, new GUIContent("接取时检查状态"));
                            lineCount++;
                            #endregion
                            break;
                        default:
                            break;
                    }
                }
            },
            onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("收集目标"), false, OnAddOption, 0);
                menu.AddItem(new GUIContent("杀敌目标"), false, OnAddOption, 1);
                menu.AddItem(new GUIContent("谈话目标"), false, OnAddOption, 2);
                menu.AddItem(new GUIContent("检查点目标"), false, OnAddOption, 3);
                menu.AddItem(new GUIContent("道具提交目标"), false, OnAddOption, 4);
                menu.AddItem(new GUIContent("触发器目标"), false, OnAddOption, 5);
                menu.DropDown(rect);

                void OnAddOption(object data)
                {
                    int type = (int)data;
                    switch (type)
                    {
                        case 0:
                            quest.Objectives.Add(new CollectObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        case 1:
                            quest.Objectives.Add(new KillObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        case 2:
                            quest.Objectives.Add(new TalkObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        case 3:
                            quest.Objectives.Add(new MoveObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        case 4:
                            quest.Objectives.Add(new SubmitObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        case 5:
                            quest.Objectives.Add(new TriggerObjective());
                            objectiveList.Select(objectiveList.count);
                            break;
                        default:
                            break;
                    }
                }
            },
            onRemoveCallback = (list) =>
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                SerializedProperty displayName = objectives.GetArrayElementAtIndex(list.index).FindPropertyRelative("displayName");
                if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + (string.IsNullOrEmpty(displayName.stringValue) ? "空标题" : displayName.stringValue) + " ] 吗？", "确定", "取消"))
                {
                    objectives.DeleteArrayElementAtIndex(list.index);
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            },
            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },
            elementHeightCallback = (index) =>
            {
                SerializedProperty objective = objectives.GetArrayElementAtIndex(index);
                int type = GetObjectiveType(objective.managedReferenceFullTypename);
                int lineCount = 1;
                if (objective.isExpanded)
                {
                    lineCount++;//目标数量
                    if (cmpltObjctvInOrder.boolValue)
                        lineCount++;// 按顺序
                    lineCount += 1;//执行顺序
                    if (type != 5)
                    {
                        lineCount += 1;//可导航
                        if (objective.FindPropertyRelative("showMapIcon").boolValue || type == 3)
                            if (type != 2 && type != 4) lineCount++;//辅助位置
                    }
                    if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
                    HandlingType();
                }
                return lineCount * lineHeightSpace;

                void HandlingType()
                {
                    switch (type)
                    {
                        case 0:
                            lineCount += 2;//目标道具、接取时检查、提交时失去
                            break;
                        case 1:
                            lineCount += 1;//目标类型
                            switch (objective.FindPropertyRelative("killType").enumValueIndex)
                            {
                                case (int)KillObjectiveType.Specific:
                                case (int)KillObjectiveType.Race:
                                case (int)KillObjectiveType.Group:
                                    lineCount++;
                                    break;
                                default: break;
                            }
                            break;
                        case 2:
                            lineCount++;//目标NPC
                            lineCount++; //交谈时对话
                            SerializedProperty dialogue = objective.FindPropertyRelative("dialogue");
                            if (dialogue.objectReferenceValue is Dialogue dialog)
                            {
                                if (Array.Exists(allQuests, x => x != quest && x.Objectives.Exists(y => y is TalkObjective to && to.Dialogue == dialog)))
                                    lineCount += 2;//逻辑错误
                                if (dialog.Words != null && dialog.Words[0] != null)
                                    lineCount += 1;//对话的第一句
                            }
                            break;
                        case 3:
                            lineCount++;//道具
                            break;
                        case 4:
                            lineCount += 4;//NPC、目标道具、提交对话、对话人
                            break;
                        case 5:
                            lineCount += 2;//触发器、状态、检查
                            break;
                        default:
                            break;
                    }
                }
            },
            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = quest.Objectives.FindAll(x => !x.IsValid).Count;
                EditorGUI.LabelField(rect, "任务目标列表", "数量：" + objectives.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
            },
            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };

        int GetObjectiveType(string managedReferenceFullTypename)
        {
            if (managedReferenceFullTypename.Contains(typeof(CollectObjective).Name))
                return 0;
            if (managedReferenceFullTypename.Contains(typeof(KillObjective).Name))
                return 1;
            if (managedReferenceFullTypename.Contains(typeof(TalkObjective).Name))
                return 2;
            if (managedReferenceFullTypename.Contains(typeof(MoveObjective).Name))
                return 3;
            if (managedReferenceFullTypename.Contains(typeof(SubmitObjective).Name))
                return 4;
            if (managedReferenceFullTypename.Contains(typeof(TriggerObjective).Name))
                return 5;
            return -1;
        }
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title) || string.IsNullOrEmpty(quest.Description));

        editComplete &= !quest.AcceptCondition.Conditions.Exists(x => !x.IsValid);

        editComplete &= quest.BeginDialogue && quest.OngoingDialogue && quest.CompleteDialogue;

        editComplete &= !quest.RewardItems.Exists(x => x.item == null);

        editComplete &= !quest.Objectives.Exists(x => (!quest.CmpltObjctvInOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        return editComplete;
    }

    private void AddAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        foreach (var state in showState)
        {
            state.valueChanged.AddListener(callback);
        }
    }
    private void RemoveAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        foreach (var state in showState)
        {
            state.valueChanged.RemoveListener(callback);
        }
    }

}