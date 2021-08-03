using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(Dialogue))]
public class DialogueInspector : Editor
{
    Dialogue dialogue;
    SerializedProperty _ID;
    SerializedProperty storyDialogue;
    SerializedProperty useUnifiedNPC;
    SerializedProperty useCurrentTalkerInfo;
    SerializedProperty unifiedNPC;
    SerializedProperty dialogWords;
    ReorderableList wordsList;
    Dictionary<DialogueWords, ReorderableList> wordsOptionsLists = new Dictionary<DialogueWords, ReorderableList>();
    Dictionary<DialogueWords, ReorderableList> wordsEventsLists = new Dictionary<DialogueWords, ReorderableList>();

    float lineHeight;
    float lineHeightSpace;

    TalkerInformation[] npcs;
    string[] npcNames;

    private void OnEnable()
    {
        npcs = Resources.LoadAll<TalkerInformation>("");
        npcNames = npcs.Select(x => x.name).ToArray();//Linq分离出NPC名字

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        dialogue = target as Dialogue;

        _ID = serializedObject.FindProperty("_ID");
        storyDialogue = serializedObject.FindProperty("storyDialogue");
        useUnifiedNPC = serializedObject.FindProperty("useUnifiedNPC");
        useCurrentTalkerInfo = serializedObject.FindProperty("useCurrentTalkerInfo");
        unifiedNPC = serializedObject.FindProperty("unifiedNPC");
        dialogWords = serializedObject.FindProperty("words");

        HandlingWordsList();
    }

    public override void OnInspectorGUI()
    {
        if (string.IsNullOrEmpty(dialogue.ID) || useUnifiedNPC.boolValue && !useCurrentTalkerInfo.boolValue && !unifiedNPC.objectReferenceValue || dialogue.Words.Exists(w => w && !w.IsValid))
        {
            EditorGUILayout.HelpBox("该对话存在未补全信息。", MessageType.Warning);
        }
        else if (dialogue.Words.Count < 1) EditorGUILayout.HelpBox("无效的空对话。", MessageType.Error);
        else EditorGUILayout.HelpBox("该对话已完整。", MessageType.Info);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID()) EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            else EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(storyDialogue, new GUIContent("剧情用对话"));
        if (storyDialogue.boolValue)
        {
            EditorGUILayout.HelpBox("在进行该对话时将不会显示返回、结束等按钮，且在完成对话时会自动关闭对话窗口。", MessageType.Info);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(useUnifiedNPC, new GUIContent("使用统一的NPC"));
        if (useUnifiedNPC.boolValue && !storyDialogue.boolValue)
        {
            EditorGUILayout.PropertyField(useCurrentTalkerInfo, new GUIContent("统一为当前对话人"));
        }
        EditorGUILayout.EndHorizontal();
        if (useUnifiedNPC.boolValue && !useCurrentTalkerInfo.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            if (dialogue.UnifiedNPC) unifiedNPC.objectReferenceValue = npcs[EditorGUILayout.Popup(GetNPCIndex(dialogue.UnifiedNPC), npcNames)];
            else if (npcs.Length > 0) unifiedNPC.objectReferenceValue = npcs[EditorGUILayout.Popup(0, npcNames)];
            else EditorGUILayout.Popup(0, new string[] { "无可用谈话人" });
            GUI.enabled = false;
            EditorGUILayout.PropertyField(unifiedNPC, new GUIContent(string.Empty));
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        wordsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    void HandlingWordsList()
    {
        wordsList = new ReorderableList(serializedObject, dialogWords, true, true, true, true);

        wordsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            SerializedProperty words = dialogWords.GetArrayElementAtIndex(index);
            SerializedProperty talkerType = words.FindPropertyRelative("talkerType");
            SerializedProperty talkerInfo = words.FindPropertyRelative("talkerInfo");
            SerializedProperty content = words.FindPropertyRelative("content");
            SerializedProperty indexOfCorrectOption = words.FindPropertyRelative("indexOfCorrectOption");
            SerializedProperty wrongChoiceWords = words.FindPropertyRelative("wrongChoiceWords");
            SerializedProperty options = words.FindPropertyRelative("options");
            SerializedProperty events = words.FindPropertyRelative("events");

            string talkerName;
            if (talkerType.enumValueIndex == (int)TalkerType.NPC || talkerType.enumValueIndex == (int)TalkerType.UnifiedNPC)
            {
                if (!useUnifiedNPC.boolValue)
                    talkerName = dialogue.Words[index] == null ? "(空)" : !dialogue.Words[index].TalkerInfo ? "(空谈话人)" : (dialogue.Words[index].TalkerName + "说");
                else talkerName = !dialogue.UnifiedNPC ? "(空)" : (useCurrentTalkerInfo.boolValue ? "NPC说" : dialogue.UnifiedNPC.name + "说");
            }
            else talkerName = "玩家说";
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2, lineHeight), words, new GUIContent(talkerName));
            if (!words.isExpanded)
            {
                int os = options.arraySize;
                int ts = events.arraySize;
                if (os > 0 || ts > 0)
                {
                    System.Text.StringBuilder label = new System.Text.StringBuilder("(");
                    label.Append(os > 0 ? os + "个选项" : string.Empty);
                    label.Append(os > 0 && ts > 0 ? ", " : string.Empty);
                    label.Append(ts > 0 ? ts + "个事件" : string.Empty);
                    label.Append(')');
                    EditorGUI.LabelField(new Rect(rect.width / 2, rect.y, rect.width / 2, lineHeight), label.ToString());
                }
            }
            int oIndex = talkerType.enumValueIndex == 1 ? 0 : (useUnifiedNPC.boolValue ? 1 : (GetNPCIndex(talkerInfo.objectReferenceValue as TalkerInformation) + 1));
            List<int> indexes = new List<int>() { 0 };
            List<string> names = new List<string>() { "玩家" };
            if (!useUnifiedNPC.boolValue)
            {
                for (int i = 1; i <= npcNames.Length; i++)
                {
                    indexes.Add(i);
                    names.Add(npcNames[i - 1]);
                }
            }
            else
            {
                indexes.Add(1);
                names.Add("NPC");
            }
            oIndex = EditorGUI.IntPopup(new Rect(rect.x + rect.width * 0.75f, rect.y, rect.width / 4, lineHeight), oIndex, names.ToArray(), indexes.ToArray());
            if (oIndex > 0)
            {
                if (!useUnifiedNPC.boolValue)
                {
                    talkerType.enumValueIndex = 0;
                    if (oIndex <= npcs.Length) talkerInfo.objectReferenceValue = npcs[oIndex - 1];
                    else talkerInfo.objectReferenceValue = null;
                }
                else talkerType.enumValueIndex = 2;
            }
            else talkerType.enumValueIndex = 1;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            if (words.isExpanded)
            {
                words.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                int lineCount = 1;
                if (talkerType.enumValueIndex == (int)TalkerType.NPC && !useUnifiedNPC.boolValue)
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2f, lineHeight), "引用资源");
                    GUI.enabled = false;
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2f, lineHeight),
                        talkerInfo, new GUIContent(string.Empty));
                    GUI.enabled = true;
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount - lineHeight, rect.width, lineHeight * 4),
                    content, new GUIContent(string.Empty));
                lineCount += 2;
                if (EditorGUI.EndChangeCheck()) words.serializedObject.ApplyModifiedProperties();
                if (GUI.Button(new Rect(rect.x, rect.y + lineHeightSpace * (lineCount + 0.5f), rect.width, lineHeight), "插入关键字"))
                {
                    GUI.FocusControl(null);
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < npcs.Length; i++)
                        menu.AddItem(new GUIContent($"人名/{CharacterInformation.GetSexString(npcs[i].Sex)}/{npcs[i].name}"), false, OnSelectNpc, i);
                    var items = Resources.LoadAll<ItemBase>("");
                    for (int i = 0; i < items.Length; i++)
                        menu.AddItem(new GUIContent($"道具/{ItemBase.GetItemTypeString(items[i].ItemType)}/{items[i].name}"), false, OnSelectItem, i);
                    var enemies = Resources.LoadAll<EnemyInformation>("");
                    for (int i = 0; i < enemies.Length; i++)
                        menu.AddItem(new GUIContent(string.Format("敌人/{0}{1}", enemies[i].Race ? enemies[i].Race.name + "/" : string.Empty, enemies[i].name)),
                            false, OnSelectEnemy, i);

                    menu.DropDown(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2f, lineHeight));

                    void OnSelectNpc(object i)
                    {
                        int ni = (int)i;
                        content.stringValue += $"{{[NPC]{npcs[ni].ID}}}";
                        serializedObject.ApplyModifiedProperties();
                    }

                    void OnSelectItem(object i)
                    {
                        int ii = (int)i;
                        content.stringValue += $"{{[ITEM]{items[ii].ID}}}";
                        serializedObject.ApplyModifiedProperties();
                    }

                    void OnSelectEnemy(object i)
                    {
                        int ei = (int)i;
                        content.stringValue += $"{{[ENMY]{enemies[ei].ID}}}";
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                lineCount++;
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + 12, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                    options, new GUIContent("选项\t\t" + (options.isExpanded ? string.Empty : (options.arraySize > 0 ? "数量: " + options.arraySize : "无选项"))));
                lineCount++;
                if (EditorGUI.EndChangeCheck()) words.serializedObject.ApplyModifiedProperties();
                ReorderableList optionsList = null;
                if (options.isExpanded)
                {
                    options.serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    //仅在选择型选项大于1个时才支持选取正确选项
                    if (dialogue.Words[index].Options.FindAll(x => x.OptionType == WordsOptionType.Choice).Count > 1)
                    {
                        List<int> choiceIndexes = new List<int>() { -1 };
                        List<string> choiceIndexStrings = new List<string>() { "不指定" };
                        var wordsOptions = dialogue.Words[index].Options;
                        for (int i = 0; i < wordsOptions.Count; i++)
                            if (wordsOptions[i].OptionType == WordsOptionType.Choice)
                            {
                                choiceIndexes.Add(i);
                                choiceIndexStrings.Add("[ " + i + " ] " + wordsOptions[i].Title);
                            }
                        if (!choiceIndexes.Contains(indexOfCorrectOption.intValue)) indexOfCorrectOption.intValue = choiceIndexes[1];
                        indexOfCorrectOption.intValue = EditorGUI.IntPopup(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                            "正确选项序号", indexOfCorrectOption.intValue, choiceIndexStrings.ToArray(), choiceIndexes.ToArray());
                        lineCount++;
                        if (indexOfCorrectOption.intValue > -1)
                        {
                            EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                                "选择其它选项时说的话：");
                            lineCount++;
                            wrongChoiceWords.stringValue = EditorGUI.TextArea(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                                wrongChoiceWords.stringValue);
                            lineCount++;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                        options.serializedObject.ApplyModifiedProperties();
                    wordsOptionsLists.TryGetValue(dialogue.Words[index], out optionsList);
                    if (optionsList == null)
                    {
                        optionsList = new ReorderableList(words.serializedObject, options, true, true, true, true);
                        wordsOptionsLists.Add(dialogue.Words[index], optionsList);
                        optionsList.drawElementCallback = (_rect, _index, _isActive, _isFocused) =>
                        {
                            options.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            SerializedProperty option = options.GetArrayElementAtIndex(_index);
                            SerializedProperty title = option.FindPropertyRelative("title");
                            SerializedProperty optionType = option.FindPropertyRelative("optionType");
                            SerializedProperty hasWordsToSay = option.FindPropertyRelative("hasWordsToSay");
                            SerializedProperty b_talkerType = option.FindPropertyRelative("talkerType");
                            SerializedProperty b_words = option.FindPropertyRelative("words");
                            SerializedProperty dialogue = option.FindPropertyRelative("dialogue");
                            SerializedProperty specifyIndex = option.FindPropertyRelative("specifyIndex");
                            SerializedProperty goBack = option.FindPropertyRelative("goBack");
                            SerializedProperty indexToGoBack = option.FindPropertyRelative("indexToGoBack");
                            SerializedProperty itemToSubmit = option.FindPropertyRelative("itemToSubmit");
                            SerializedProperty itemCanGet = option.FindPropertyRelative("itemCanGet");
                            SerializedProperty onlyForQuest = option.FindPropertyRelative("onlyForQuest");
                            SerializedProperty bindedQuest = option.FindPropertyRelative("bindedQuest");
                            SerializedProperty showOnlyWhenNotHave = option.FindPropertyRelative("showOnlyWhenNotHave");
                            SerializedProperty deleteWhenCmplt = option.FindPropertyRelative("deleteWhenCmplt");
                            string label = string.IsNullOrEmpty(title.stringValue) ? "(空标题)" : title.stringValue;
                            EditorGUI.PropertyField(new Rect(_rect.x + 8, _rect.y, _rect.width / 2, lineHeight),
                                option, new GUIContent(label));
                            EditorGUI.PropertyField(new Rect(_rect.x + 8 + _rect.width / 2, _rect.y, _rect.width / 2 - 8, lineHeight),
                                optionType, new GUIContent(string.Empty));
                            if (optionType.intValue == (int)WordsOptionType.Choice && index == dialogWords.arraySize - 1)
                            {
                                if (EditorUtility.DisplayDialog("选项类型错误", "最后一句不支持选择型选项。", "确定"))
                                    optionType.intValue = (int)this.dialogue.Words[index].Options[_index].OptionType;
                            }
                            if (option.isExpanded)
                            {
                                int _lineCount = 1;
                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                    title, new GUIContent("选项标题"));
                                _lineCount++;
                                if (optionType.intValue == (int)WordsOptionType.Choice)
                                {
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        hasWordsToSay, new GUIContent("有话说"));
                                    _lineCount++;
                                }
                                if (!(optionType.intValue == (int)WordsOptionType.Choice && !hasWordsToSay.boolValue))
                                {
                                    if (optionType.intValue == (int)WordsOptionType.BranchDialogue)
                                    {
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            dialogue, new GUIContent("分支对话"));
                                        _lineCount++;
                                        if ((dialogue.objectReferenceValue as Dialogue) == this.dialogue)
                                        {
                                            if (EditorUtility.DisplayDialog("编辑错误", "使用该对话将造成闭环。", "确定"))
                                                dialogue.objectReferenceValue = this.dialogue.Words[index].Options[_index].Dialogue;
                                        }
                                        if (this.dialogue.Words[index].Options[_index].IsValid)
                                        {
                                            WordsOption dialog = this.dialogue.Words[index].Options[_index];
                                            specifyIndex.intValue = EditorGUI.IntSlider(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                "指定句子序号", specifyIndex.intValue, 0, dialog.Dialogue.Words.Count - 1);
                                            _lineCount++;
                                            GUI.enabled = false;
                                            EditorGUI.TextField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                dialog.Dialogue.Words[specifyIndex.intValue < 0 ? 0 : specifyIndex.intValue].ToString());
                                            GUI.enabled = true;
                                            _lineCount++;
                                        }
                                    }
                                    else
                                    {
                                        if (optionType.intValue == (int)WordsOptionType.SubmitAndGet || optionType.intValue == (int)WordsOptionType.OnlyGet)
                                        {
                                            if (optionType.intValue == (int)WordsOptionType.SubmitAndGet)
                                            {
                                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    itemToSubmit.FindPropertyRelative("item"), new GUIContent("需提交的道具"));
                                                _lineCount++;
                                                if (this.dialogue.Words[index].Options[_index].ItemToSubmit.item)
                                                {
                                                    EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                        "道具名称", this.dialogue.Words[index].Options[_index].ItemToSubmit.ItemName);
                                                    _lineCount++;
                                                    SerializedProperty amounts = itemToSubmit.FindPropertyRelative("amount");
                                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                        amounts, new GUIContent("需提交的数量"));
                                                    _lineCount++;
                                                    if (amounts.intValue < 1) amounts.intValue = 1;
                                                }
                                            }
                                            EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                itemCanGet.FindPropertyRelative("item"), new GUIContent("可获得的道具"));
                                            _lineCount++;
                                            if (this.dialogue.Words[index].Options[_index].ItemCanGet.item)
                                            {
                                                EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    "道具名称", this.dialogue.Words[index].Options[_index].ItemCanGet.ItemName);
                                                _lineCount++;
                                                SerializedProperty amountg = itemCanGet.FindPropertyRelative("amount");
                                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    amountg, new GUIContent("可获得的数量"));
                                                _lineCount++;
                                                if (amountg.intValue < 1) amountg.intValue = 1;
                                            }
                                            if (optionType.intValue == (int)WordsOptionType.OnlyGet)
                                            {
                                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                showOnlyWhenNotHave, new GUIContent("只在未持有时显示"));
                                                _lineCount++;
                                            }
                                            if (showOnlyWhenNotHave.boolValue || optionType.intValue == (int)WordsOptionType.SubmitAndGet)
                                            {
                                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    onlyForQuest, new GUIContent("只在任务时显示"));
                                                _lineCount++;
                                                if (onlyForQuest.boolValue)
                                                {
                                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                        bindedQuest, new GUIContent("相关任务"));
                                                    _lineCount++;
                                                    if (this.dialogue.Words[index].Options[_index].BindedQuest)
                                                    {
                                                        EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                            "任务标题", this.dialogue.Words[index].Options[_index].BindedQuest.Title);
                                                        _lineCount++;
                                                    }
                                                }
                                            }
                                            EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                optionType.intValue == (int)WordsOptionType.SubmitAndGet ? "提交时的对话" : "获得时的对话",
                                                new GUIStyle() { fontStyle = FontStyle.Bold });
                                            _lineCount++;
                                        }
                                        string b_talkerName;
                                        if (b_talkerType.enumValueIndex == (int)TalkerType.NPC)
                                            b_talkerName = (this.dialogue.UseUnifiedNPC ? this.dialogue.UnifiedNPC.name : this.dialogue.Words[index].TalkerName) + "说";
                                        else b_talkerName = "玩家说";
                                        EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight), b_talkerName);
                                        b_talkerType.enumValueIndex = EditorGUI.IntPopup(new Rect(_rect.x + _rect.width / 2, _rect.y + lineHeightSpace * _lineCount, _rect.width / 2, lineHeight),
                                          b_talkerType.enumValueIndex, new string[] { "NPC", "玩家" }, new int[] { 0, 1 });
                                        _lineCount++;
                                        b_words.stringValue = EditorGUI.TextField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            b_words.stringValue);
                                        _lineCount++;
                                    }
                                    if ((optionType.intValue != (int)WordsOptionType.Choice && optionType.intValue != (int)WordsOptionType.SubmitAndGet && optionType.intValue != (int)WordsOptionType.OnlyGet
                                    || optionType.intValue == (int)WordsOptionType.Choice && indexOfCorrectOption.intValue >= 0) && !this.dialogue.Words[index].NeedToChusCorrectOption)
                                    {
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            goBack, new GUIContent("返回至原对话"));
                                        _lineCount++;
                                        if (goBack.boolValue && optionType.intValue != (int)WordsOptionType.Choice)//选择型选项无法指定返回序号
                                        {
                                            indexToGoBack.intValue = EditorGUI.IntSlider(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                new GUIContent("返回至的对话序号", "-1表示返回出现该选项的原句子"), indexToGoBack.intValue, -1, dialogWords.arraySize - 1);
                                            _lineCount++;
                                        }
                                    }
                                }
                                if (optionType.intValue == (int)WordsOptionType.Choice && indexOfCorrectOption.intValue != _index)
                                {
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        deleteWhenCmplt, new GUIContent("对话完成时删除"));
                                    _lineCount++;
                                }
                            }
                            if (EditorGUI.EndChangeCheck()) options.serializedObject.ApplyModifiedProperties();
                        };

                        optionsList.elementHeightCallback = (_index) =>
                        {
                            int _lineCount = 1;
                            SerializedProperty option = options.GetArrayElementAtIndex(_index);
                            SerializedProperty optionType = option.FindPropertyRelative("optionType");
                            SerializedProperty b_talkerType = option.FindPropertyRelative("talkerType");
                            SerializedProperty b_words = option.FindPropertyRelative("words");
                            SerializedProperty dialogue = option.FindPropertyRelative("dialogue");
                            SerializedProperty goBack = option.FindPropertyRelative("goBack");
                            if (option.isExpanded)
                            {
                                _lineCount++;//标题
                                if (optionType.intValue == (int)WordsOptionType.Choice)
                                {
                                    _lineCount++;//有话
                                }
                                if (!(optionType.intValue == (int)WordsOptionType.Choice && !option.FindPropertyRelative("hasWordsToSay").boolValue))
                                {
                                    if (optionType.intValue == (int)WordsOptionType.BranchDialogue)
                                    {
                                        _lineCount++;//对话
                                        if (this.dialogue.Words[index].Options[_index].IsValid)
                                            _lineCount += 2;//句子序号、序号句子
                                    }
                                    else
                                    {
                                        if (optionType.intValue == (int)WordsOptionType.SubmitAndGet || optionType.intValue == (int)WordsOptionType.OnlyGet)
                                        {
                                            if (optionType.intValue == (int)WordsOptionType.SubmitAndGet)
                                            {
                                                _lineCount++;//需提交
                                                if (this.dialogue.Words[index].Options[_index].ItemToSubmit.item)
                                                    _lineCount += 2;//道具名称、数量
                                            }
                                            _lineCount++;//可获得
                                            if (this.dialogue.Words[index].Options[_index].ItemCanGet.item)
                                                _lineCount += 2;//道具名称、可获得数量
                                            if (optionType.intValue == (int)WordsOptionType.OnlyGet)
                                                _lineCount++; //只在未持有
                                            if (option.FindPropertyRelative("showOnlyWhenNotHave").boolValue || optionType.intValue == (int)WordsOptionType.SubmitAndGet)
                                            {
                                                _lineCount++;//只在任务
                                                if (option.FindPropertyRelative("onlyForQuest").boolValue)
                                                {
                                                    _lineCount++;//任务
                                                    if (option.FindPropertyRelative("bindedQuest").objectReferenceValue)
                                                        _lineCount++;//任务名
                                                }
                                            }
                                            _lineCount++;//对话粗体
                                        }
                                        _lineCount += 2;//谈话人名字、说的话
                                    }
                                    if ((optionType.intValue != (int)WordsOptionType.Choice && optionType.intValue != (int)WordsOptionType.SubmitAndGet && optionType.intValue != (int)WordsOptionType.OnlyGet
                                    || optionType.intValue == (int)WordsOptionType.Choice && indexOfCorrectOption.intValue >= 0) && !this.dialogue.Words[index].NeedToChusCorrectOption)
                                    {
                                        _lineCount++;//返回
                                        if (goBack.boolValue && optionType.intValue != (int)WordsOptionType.Choice)
                                            _lineCount++;//返回序号
                                    }
                                }
                                if (optionType.intValue == (int)WordsOptionType.Choice && indexOfCorrectOption.intValue != _index)
                                    _lineCount++;//完成删除
                            }
                            return _lineCount * lineHeightSpace;
                        };

                        optionsList.onAddDropdownCallback = (_rect, _list) =>
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("分支/一句"), false, OnAddOption, 0);
                            menu.AddItem(new GUIContent("分支/一段"), false, OnAddOption, 1);
                            menu.AddItem(new GUIContent("道具/提交并获得"), false, OnAddOption, 3);
                            menu.AddItem(new GUIContent("道具/仅获得"), false, OnAddOption, 4);
                            menu.AddItem(new GUIContent("选择项"), false, OnAddOption, 2);
                            menu.DropDown(_rect);

                            void OnAddOption(object data)
                            {
                                int type = (int)data;
                                options.serializedObject.Update();
                                EditorGUI.BeginChangeCheck();
                                if (type == (int)WordsOptionType.Choice && index == dialogWords.arraySize - 1)
                                    EditorUtility.DisplayDialog("选项类型错误", "最后一句不支持选择型选项。", "确定");
                                else if (_list.index >= 0 && _list.index + 1 < dialogue.Words[index].Options.Count)
                                    dialogue.Words[index].Options.Insert(_list.index + 1, new WordsOption((WordsOptionType)type));
                                else dialogue.Words[index].Options.Add(new WordsOption((WordsOptionType)type));
                                if (EditorGUI.EndChangeCheck())
                                    options.serializedObject.ApplyModifiedProperties();
                            }
                        };

                        optionsList.onRemoveCallback = (_list) =>
                        {
                            options.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            if (EditorUtility.DisplayDialog("删除", "确定删除这个选项吗？", "确定", "取消"))
                                options.DeleteArrayElementAtIndex(_list.index);
                            if (EditorGUI.EndChangeCheck())
                                options.serializedObject.ApplyModifiedProperties();
                        };

                        optionsList.drawHeaderCallback = (_rect) =>
                        {
                            int notCmpltCount = dialogue.Words[index].Options.FindAll(x => !x.IsValid).Count;
                            EditorGUI.LabelField(_rect, "选项列表", "数量：" + dialogue.Words[index].Options.Count.ToString() +
                                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                        };

                        optionsList.drawNoneElementCallback = (_rect) =>
                        {
                            EditorGUI.LabelField(_rect, "空列表");
                        };
                    }
                    words.serializedObject.Update();
                    optionsList.DoList(new Rect(rect.x, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width, lineHeight * (options.arraySize + 1)));
                    words.serializedObject.ApplyModifiedProperties();
                }
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + 12, rect.y + (optionsList == null ? 0 : optionsList.GetHeight()) + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                    events, new GUIContent("事件\t\t" + (events.isExpanded ? string.Empty : (events.arraySize > 0 ? "数量: " + events.arraySize : "无事件"))));
                lineCount++;
                if (EditorGUI.EndChangeCheck()) words.serializedObject.ApplyModifiedProperties();
                if (events.isExpanded)
                {
                    events.serializedObject.Update();
                    wordsEventsLists.TryGetValue(dialogue.Words[index], out ReorderableList eventsList);
                    if (eventsList == null)
                    {
                        eventsList = new ReorderableList(words.serializedObject, events, true, true, true, true);
                        wordsEventsLists.Add(dialogue.Words[index], eventsList);
                        eventsList.drawElementCallback = (_rect, _index, _isActive, _isFocused) =>
                        {
                            events.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            SerializedProperty trigger = events.GetArrayElementAtIndex(_index);
                            SerializedProperty eventType = trigger.FindPropertyRelative("eventType");
                            SerializedProperty wordsTrigrName = trigger.FindPropertyRelative("wordsTrigrName");
                            SerializedProperty triggerActType = trigger.FindPropertyRelative("triggerActType");
                            SerializedProperty toWhom = trigger.FindPropertyRelative("toWhom");
                            SerializedProperty favorabilityValue = trigger.FindPropertyRelative("favorabilityValue");
                            EditorGUI.PropertyField(new Rect(_rect.x, _rect.y, _rect.width, lineHeight), eventType, new GUIContent("事件类型"));
                            switch ((WordsEventType)eventType.intValue)
                            {
                                case WordsEventType.Trigger:
                                    EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace, 28, lineHeight), "名称");
                                    EditorGUI.PropertyField(new Rect(_rect.x + 28, _rect.y + lineHeightSpace, _rect.width / 1.5f - 28, lineHeight), wordsTrigrName, new GUIContent(string.Empty));
                                    EditorGUI.LabelField(new Rect(_rect.x + 2 + _rect.width / 1.5f, _rect.y + lineHeightSpace, 38, lineHeight), "操作");
                                    EditorGUI.PropertyField(new Rect(_rect.x + 30 + _rect.width / 1.5f, _rect.y + lineHeightSpace, _rect.width - 30 - _rect.width / 1.5f, lineHeight),
                                        triggerActType, new GUIContent(string.Empty));
                                    break;
                                case WordsEventType.GetAmity:
                                case WordsEventType.LoseAmity:
                                    oIndex = GetNPCIndex(toWhom.objectReferenceValue as TalkerInformation);
                                    indexes.Clear();
                                    names.Clear();
                                    for (int i = 0; i < npcNames.Length; i++)
                                    {
                                        indexes.Add(i);
                                        names.Add(npcNames[i]);
                                    }
                                    EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace, 28, lineHeight), "对谁");
                                    oIndex = EditorGUI.IntPopup(new Rect(_rect.x + 28, _rect.y + lineHeightSpace, _rect.width / 2 - 28, lineHeight), oIndex, names.ToArray(), indexes.ToArray());
                                    if (oIndex >= 0 && oIndex < npcs.Length) toWhom.objectReferenceValue = npcs[oIndex];
                                    else toWhom.objectReferenceValue = null;
                                    EditorGUI.LabelField(new Rect(_rect.x + 2 + _rect.width / 2, _rect.y + lineHeightSpace, 40, lineHeight), "好感值");
                                    EditorGUI.PropertyField(new Rect(_rect.x + 42 + _rect.width / 2, _rect.y + lineHeightSpace, _rect.width / 2 - 42, lineHeight),
                                        favorabilityValue, new GUIContent(string.Empty));
                                    break;
                                default:
                                    break;
                            }
                            if (EditorGUI.EndChangeCheck()) events.serializedObject.ApplyModifiedProperties();
                        };

                        eventsList.elementHeightCallback = (_index) =>
                        {
                            return lineHeightSpace * 2;
                        };

                        eventsList.onRemoveCallback = (_list) =>
                        {
                            events.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            if (EditorUtility.DisplayDialog("删除", "确定删除这个事件吗？", "确定", "取消"))
                                events.DeleteArrayElementAtIndex(_list.index);
                            if (EditorGUI.EndChangeCheck()) events.serializedObject.ApplyModifiedProperties();
                        };

                        eventsList.onAddCallback = (_list) =>
                        {
                            events.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            dialogue.Words[index].Events.Add(new WordsEvent());
                            if (EditorGUI.EndChangeCheck()) events.serializedObject.ApplyModifiedProperties();
                        };

                        eventsList.drawHeaderCallback = (_rect) =>
                        {
                            int notCmpltCount = dialogue.Words[index].Events.FindAll(x => !x.IsValid).Count;
                            EditorGUI.LabelField(_rect, "事件列表", "数量：" + dialogue.Words[index].Events.Count.ToString() +
                                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                        };

                        eventsList.drawNoneElementCallback = (_rect) =>
                        {
                            EditorGUI.LabelField(_rect, "空列表");
                        };
                    }
                    words.serializedObject.Update();
                    eventsList.DoList(new Rect(rect.x, rect.y + (optionsList == null ? 0 : optionsList.GetHeight()) + lineHeightSpace * lineCount + lineHeight - 5,
                        rect.width, lineHeight * (events.arraySize + 1)));
                    words.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), content.stringValue);
                GUI.enabled = true;
            }
        };

        wordsList.elementHeightCallback = (int index) =>
        {
            int lineCount = 1;
            float listHeight = 0.0f;
            SerializedProperty words = dialogWords.GetArrayElementAtIndex(index);
            if (words.isExpanded)
            {
                SerializedProperty options = words.FindPropertyRelative("options");
                SerializedProperty events = words.FindPropertyRelative("events");
                SerializedProperty indexOfCorrectOption = words.FindPropertyRelative("indexOfCorrectOption");
                if (dialogue.Words[index].TalkerType == TalkerType.NPC && !useUnifiedNPC.boolValue) lineCount += 1;//NPC选择
                lineCount += 5;//对话、按钮、选项
                if (options.isExpanded)
                {
                    if (options.arraySize > 1)
                        if (dialogue.Words[index].Options.FindAll(x => x && x.OptionType == WordsOptionType.Choice).Count > 1)
                        {
                            lineCount++;//正确分支
                            if (indexOfCorrectOption.intValue > -1)
                                lineCount += 2;//说的话、说的话填写
                        }
                    if (wordsOptionsLists.TryGetValue(dialogue.Words[index], out var optionList))
                        listHeight += optionList.GetHeight();
                }
                lineCount++;//事件
                if (events.isExpanded)
                {
                    if (wordsEventsLists.TryGetValue(dialogue.Words[index], out var eventsList))
                    {
                        listHeight += eventsList.GetHeight();
                    }
                }
                listHeight -= 8;
            }
            else lineCount++;
            return lineCount * lineHeightSpace + listHeight;
        };

        wordsList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (list.index >= 0 && list.index + 1 < dialogue.Words.Count)
                dialogue.Words.Insert(list.index + 1, new DialogueWords());
            else dialogue.Words.Add(new DialogueWords());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        wordsList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这句话吗？", "确定", "取消"))
            {
                if (dialogue.Words.Count > 1 && list.index == dialogue.Words.Count - 1 && dialogue.Words[dialogue.Words.Count - 2].Options.Exists(x => x && x.OptionType == WordsOptionType.Choice))
                    EditorUtility.DisplayDialog("错误", "上一句存在选择型选项，无法删除该句。", "确定");
                else dialogue.Words.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        };

        wordsList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = dialogue.Words.FindAll(w => w.TalkerType == TalkerType.NPC && (!w.TalkerInfo || string.IsNullOrEmpty(w.Content)) || w.Options.Exists(b => b && !b.IsValid)).Count;
            EditorGUI.LabelField(rect, "对话列表", "数量：" + dialogue.Words.Count +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        wordsList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    int GetNPCIndex(TalkerInformation npc)
    {
        return Array.IndexOf(npcs, npc);
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        Dialogue[] dialogues = Resources.LoadAll<Dialogue>("");
        for (int i = 1; i < 1000000; i++)
        {
            newID = "DIALG" + i.ToString().PadLeft(6, '0');
            if (!Array.Exists(dialogues, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        Dialogue[] dialogues = Resources.LoadAll<Dialogue>("");

        Dialogue find = Array.Find(dialogues, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != dialogue || (find == dialogue && Array.FindAll(dialogues, x => x.ID == _ID.stringValue).Length > 1);
    }
}