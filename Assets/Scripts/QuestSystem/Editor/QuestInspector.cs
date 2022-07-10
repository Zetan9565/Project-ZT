using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
using ZetanStudio;
using ZetanStudio.Extension.Editor;

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
    SerializedProperty rewardItems;
    SerializedProperty _NPCToSubmit;
    SerializedProperty inOrder;
    SerializedProperty objectives;

    ConditionGroupDrawer acceptConditionDrawer;
    ItemInfoListDrawer rewardDrawer;
    ReorderableList objectiveList;

    float lineHeight;
    float lineHeightSpace;

    int barIndex;

    Quest[] questCache;
    TalkerInformation[] talkerCache;

    TalkerInformation holder;

    AnimBool showState;

    private void OnEnable()
    {
        questCache = Resources.LoadAll<Quest>("Configuration");
        quest = target as Quest;
        talkerCache = Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.Enable).ToArray();
        holder = talkerCache.FirstOrDefault(x => x.QuestsStored.Contains(quest));

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
        rewardItems = serializedObject.FindProperty("rewardItems");
        _NPCToSubmit = serializedObject.FindProperty("_NPCToSubmit");
        inOrder = serializedObject.FindProperty("inOrder");
        objectives = serializedObject.FindProperty("objectives");
        acceptConditionDrawer = new ConditionGroupDrawer(acceptCondition, lineHeight, lineHeightSpace, "接取条件列表");
        rewardDrawer = new ItemInfoListDrawer(serializedObject, rewardItems, lineHeight, lineHeightSpace, "奖励列表");
        HandlingObjectiveList();
        showState = new AnimBool(objectives.isExpanded);
        AddAnimaListener(OnAnima);
    }

    private void OnDisable()
    {
        RemoveAnimaListener(OnAnima);
    }

    private void OnAnima()
    {
        Repaint();
        objectives.isExpanded = showState.target;
    }

    public override void OnInspectorGUI()
    {
        if (!CheckEditComplete())
            EditorGUILayout.HelpBox("该任务存在未补全信息。", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("该任务信息已完整。", MessageType.Info);
        barIndex = GUILayout.Toolbar(barIndex, new string[] { "基本", "条件", "奖励", "对话", "目标" });
        EditorGUILayout.Space();
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        switch (barIndex)
        {
            case 0:
                #region case 0 基本
                EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
                if (string.IsNullOrEmpty(_ID.stringValue) || Quest.IsIDDuplicate(quest, questCache))
                {
                    if (!string.IsNullOrEmpty(_ID.stringValue) && Quest.IsIDDuplicate(quest, questCache))
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
                EditorGUILayout.PropertyField(group, new GUIContent("归属组"));
                EditorGUILayout.PropertyField(_NPCToSubmit, new GUIContent("在此NPC处提交"));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                if (holder)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(new GUIContent("持有该任务的NPC"), holder, typeof(TalkerInformation), false);
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.LabelField("描述显示预览");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(Keywords.Editor.HandlingKeyWords(title.stringValue) + "\n" + Keywords.Editor.HandlingKeyWords(description.stringValue), new GUIStyle(EditorStyles.textArea) { wordWrap = true });
                EditorGUI.EndDisabledGroup();
                #endregion
                break;
            case 1:
                #region case 1 条件
                acceptConditionDrawer.DoLayoutDraw();
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                #endregion
                break;
            case 2:
                #region case 2 奖励
                EditorGUILayout.HelpBox("目前只设计10个道具奖励。", MessageType.Info);
                rewardDrawer.DoLayoutDraw();
                rewardDrawer.List.displayAdd = rewardItems.arraySize < 10;
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                #endregion
                break;
            case 3:
                #region case 3 对话
                EditorGUILayout.PropertyField(beginDialogue, new GUIContent("开始时的对话"));
                if (beginDialogue.objectReferenceValue)
                {
                    if (completeDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || beginDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("进行时或完成时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(questCache, x => x != quest && (x.BeginDialogue == beginDialogue.objectReferenceValue || x.CompleteDialogue == beginDialogue.objectReferenceValue
                                               || x.OngoingDialogue == beginDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
                }
                else
                {
                    NewDialogueFor(beginDialogue);
                }
                EditorGUILayout.PropertyField(ongoingDialogue, new GUIContent("进行中的对话"));
                if (ongoingDialogue.objectReferenceValue)
                {
                    if (ongoingDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || completeDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("开始时或完成时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(questCache, x => x != quest && (x.BeginDialogue == ongoingDialogue.objectReferenceValue || x.CompleteDialogue == ongoingDialogue.objectReferenceValue
                                               || x.OngoingDialogue == ongoingDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
                }
                else
                {
                    NewDialogueFor(ongoingDialogue);
                }
                EditorGUILayout.PropertyField(completeDialogue, new GUIContent("完成时的对话"));
                if (completeDialogue.objectReferenceValue)
                {
                    if (completeDialogue.objectReferenceValue == beginDialogue.objectReferenceValue || completeDialogue.objectReferenceValue == ongoingDialogue.objectReferenceValue)
                        EditorGUILayout.HelpBox("开始时或进行时已使用该对话，游戏中可能会产生逻辑错误。", MessageType.Warning);
                    else
                    {
                        Quest find = Array.Find(questCache, x => x != quest && (x.BeginDialogue == completeDialogue.objectReferenceValue || x.CompleteDialogue == completeDialogue.objectReferenceValue
                                             || x.OngoingDialogue == completeDialogue.objectReferenceValue));
                        if (find)
                        {
                            EditorGUILayout.HelpBox("已有任务使用该对话，游戏中可能会产生逻辑错误。\n配置路径：\n" + AssetDatabase.GetAssetPath(find), MessageType.Warning);
                        }
                    }
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
                EditorGUILayout.PropertyField(inOrder, new GUIContent("按顺序完成目标"));
                if (quest.InOrder)
                {
                    EditorGUILayout.HelpBox("勾选此项，则勾选按顺序的目标按执行顺序从小到大的顺序执行，若相同，则表示可以同时进行；" +
                        "若目标没有勾选按顺序，则表示该目标不受顺序影响。", MessageType.Info);
                }

                showState.target = EditorGUILayout.Foldout(objectives.isExpanded, "任务目标\t\t"
                    + (objectives.isExpanded ? string.Empty : (objectives.arraySize > 0 ? "数量：" + objectives.arraySize : "无")), true);
                if (EditorGUILayout.BeginFadeGroup(showState.faded))
                    objectiveList.DoLayoutList();
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.LabelField("目标显示预览");
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(Quest.Editor.GetObjectiveString(quest), new GUIStyle(EditorStyles.textArea) { wordWrap = true });
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                #endregion
                break;
        }

        void NewDialogueFor(SerializedProperty dialogue)
        {
            if (GUILayout.Button("新建"))
            {
                Dialogue dialogInstance = ZetanUtility.Editor.SaveFilePanel(CreateInstance<Dialogue>, "dialogue", ping: true);
                if (dialogInstance)
                {
                    dialogue.objectReferenceValue = dialogInstance;
                    EditorUtility.OpenPropertyEditor(dialogInstance);
                }
            }
        }
    }

    void HandlingObjectiveList()
    {
        objectiveList = new ReorderableList(serializedObject, objectives, true, true, true, true)
        {
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUI.PropertyField(rect, objectives.GetArrayElementAtIndex(index), true);
            },
            onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<Objective>())
                {
                    string name = type.GetCustomAttribute<Objective.NameAttribute>().name;
                    menu.AddItem(new GUIContent(name), false, OnAddOption, type);
                }
                menu.DropDown(rect);

                void OnAddOption(object data)
                {
                    Type type = (Type)data;
                    Objective objective = (Objective)Activator.CreateInstance(type);
                    type.GetField("priority", ZetanUtility.CommonBindingFlags).SetValue(objective, quest.Objectives.Count + 1);
                    quest.Objectives.Add(objective);
                }
            },
            onRemoveCallback = (list) =>
            {
                SerializedProperty displayName = objectives.GetArrayElementAtIndex(list.index).FindPropertyRelative("displayName");
                if (EditorUtility.DisplayDialog("删除", "确定删除目标 [ " + (string.IsNullOrEmpty(displayName.stringValue) ? "空标题" : displayName.stringValue) + " ] 吗？", "确定", "取消"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    serializedObject.ApplyModifiedProperties();
                }
                GUIUtility.ExitGUI();
            },
            onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            },
            elementHeightCallback = (index) =>
            {
                return EditorGUI.GetPropertyHeight(objectives.GetArrayElementAtIndex(index));
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
    }

    bool CheckEditComplete()
    {
        bool editComplete = true;

        editComplete &= !(string.IsNullOrEmpty(quest.ID) || string.IsNullOrEmpty(quest.Title) || string.IsNullOrEmpty(quest.Description));

        editComplete &= !quest.AcceptCondition.Conditions.Exists(x => !x.IsValid);

        editComplete &= quest.BeginDialogue && quest.OngoingDialogue && quest.CompleteDialogue;

        editComplete &= !quest.RewardItems.Exists(x => x.Item == null);

        editComplete &= !quest.Objectives.Exists(x => (!quest.InOrder || x.Display) && string.IsNullOrEmpty(x.DisplayName) || !x.IsValid);

        return editComplete;
    }

    public void AddAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        showState.valueChanged.AddListener(callback);
    }
    public void RemoveAnimaListener(UnityEngine.Events.UnityAction callback)
    {
        showState.valueChanged.RemoveListener(callback);
    }
}