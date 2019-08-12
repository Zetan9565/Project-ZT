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
    SerializedProperty dialogWords;
    ReorderableList wordsList;
    Dictionary<DialogueWords, ReorderableList> wordsBranchesLists = new Dictionary<DialogueWords, ReorderableList>();

    float lineHeight;
    float lineHeightSpace;

    TalkerInformation[] npcs;
    string[] npcNames;

    //bool showOriginal = false;

    bool cmpltEdit;

    private void OnEnable()
    {
        npcs = Resources.LoadAll<TalkerInformation>("");
        npcNames = npcs.Select(x => x.Name).ToArray();//Linq分离出NPC名字

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        dialogue = target as Dialogue;

        _ID = serializedObject.FindProperty("_ID");
        dialogWords = serializedObject.FindProperty("words");

        HandlingWordsList();
    }

    public override void OnInspectorGUI()
    {
        if (dialogue.Words.Exists(w => w.TalkerType == TalkerType.NPC && (!w.TalkerInfo || string.IsNullOrEmpty(w.Words)) ||
            w.Branches.Exists(b => b && !b.IsValid)))
            EditorGUILayout.HelpBox("该对话存在未补全语句。", MessageType.Warning);
        else
            EditorGUILayout.HelpBox("该对话已完整。", MessageType.Info);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
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
        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
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
        wordsList = new ReorderableList(serializedObject, dialogWords, true, true, true, true);

        wordsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            SerializedProperty words = dialogWords.GetArrayElementAtIndex(index);
            SerializedProperty talkerType = words.FindPropertyRelative("talkerType");
            SerializedProperty talkerInfo = words.FindPropertyRelative("talkerInfo");
            SerializedProperty w_words = words.FindPropertyRelative("words");
            SerializedProperty indexOfRrightBranch = words.FindPropertyRelative("indexOfRrightBranch");
            SerializedProperty wordsWhenChusWB = words.FindPropertyRelative("wordsWhenChusWB");
            SerializedProperty branches = words.FindPropertyRelative("branches");

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
                w_words, new GUIContent(string.Empty));
            lineCount += 2;
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                branches, new GUIContent("选项\t\t" + (branches.arraySize > 0 ? "数量: " + branches.arraySize : "无选项")));
            lineCount++;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (branches.isExpanded)
            {
                branches.serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                if (branches.arraySize > 1)
                    //仅在所有选项都为选择型时才支持选取正确选项
                    if (dialogue.Words[index].Branches.TrueForAll(x => x.OptionType == WordsOptionType.Choice))
                    {
                        indexOfRrightBranch.intValue = EditorGUI.IntSlider(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                            "正确选项序号", indexOfRrightBranch.intValue, 0, branches.arraySize - 1);
                        lineCount++;
                        if (indexOfRrightBranch.intValue > -1)
                        {
                            EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight), "选择错误时说的话：");
                            lineCount++;
                            wordsWhenChusWB.stringValue = EditorGUI.TextArea(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                                wordsWhenChusWB.stringValue);
                            lineCount++;
                        }
                    }
                if (EditorGUI.EndChangeCheck())
                    branches.serializedObject.ApplyModifiedProperties();
                ReorderableList branchesList;
                if (!wordsBranchesLists.ContainsKey(dialogue.Words[index]))
                {
                    branchesList = new ReorderableList(words.serializedObject, branches, true, true, true, true);
                    wordsBranchesLists.Add(dialogue.Words[index], branchesList);
                    branchesList.drawElementCallback = (_rect, _index, _isActive, _isFocused) =>
                    {
                        branches.serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        SerializedProperty branch = branches.GetArrayElementAtIndex(_index);
                        SerializedProperty title = branch.FindPropertyRelative("title");
                        SerializedProperty optionType = branch.FindPropertyRelative("optionType");
                        SerializedProperty hasWords = branch.FindPropertyRelative("hasWords");
                        SerializedProperty b_talkerType = branch.FindPropertyRelative("talkerType");
                        SerializedProperty b_talkerInfo = branch.FindPropertyRelative("talkerInfo");
                        SerializedProperty b_words = branch.FindPropertyRelative("words");
                        SerializedProperty dialogue = branch.FindPropertyRelative("dialogue");
                        SerializedProperty specifyIndex = branch.FindPropertyRelative("specifyIndex");
                        SerializedProperty goBack = branch.FindPropertyRelative("goBack");
                        SerializedProperty indexToGo = branch.FindPropertyRelative("indexToGo");
                        SerializedProperty itemToSubmit = branch.FindPropertyRelative("itemToSubmit");
                        SerializedProperty itemCanGet = branch.FindPropertyRelative("itemCanGet");
                        SerializedProperty onlyForQuest = branch.FindPropertyRelative("onlyForQuest");
                        SerializedProperty bindedQuest = branch.FindPropertyRelative("bindedQuest");
                        SerializedProperty showOnlyWhenNotHave = branch.FindPropertyRelative("showOnlyWhenNotHave");
                        SerializedProperty deleteWhenCmplt = branch.FindPropertyRelative("deleteWhenCmplt");
                        string label = string.IsNullOrEmpty(title.stringValue) ? "(空标题)" : title.stringValue;
                        EditorGUI.PropertyField(new Rect(_rect.x + 8, _rect.y, _rect.width / 2, lineHeight),
                            branch, new GUIContent(label));
                        EditorGUI.PropertyField(new Rect(_rect.x + 8 + _rect.width / 2, _rect.y, _rect.width / 2, lineHeight),
                            optionType, new GUIContent(string.Empty));
                        if (optionType.intValue == (int)WordsOptionType.Choice && index == dialogWords.arraySize - 1)
                        {
                            if (EditorUtility.DisplayDialog("选项类型错误", "最后一句不支持选择型选项。", "确定"))
                                optionType.intValue = (int)this.dialogue.Words[index].Branches[_index].OptionType;
                        }
                        if (branch.isExpanded)
                        {
                            int _lineCount = 1;
                            EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                title, new GUIContent("选项标题"));
                            _lineCount++;
                            if (optionType.intValue == (int)WordsOptionType.Choice)
                            {
                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                    hasWords, new GUIContent("有话说"));
                                _lineCount++;
                            }
                            if (!(optionType.intValue == (int)WordsOptionType.Choice && !hasWords.boolValue))
                            {
                                if (optionType.intValue == (int)WordsOptionType.BranchDialogue)
                                {
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        dialogue, new GUIContent("分支对话"));
                                    _lineCount++;
                                    if (this.dialogue.Words[index].Branches[_index].Dialogue)
                                    {
                                        BranchDialogue dialog = this.dialogue.Words[index].Branches[_index];
                                        specifyIndex.intValue = EditorGUI.IntSlider(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            "指定句子序号", specifyIndex.intValue, -1, dialog.Dialogue.Words.Count - 1);
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
                                            if (this.dialogue.Words[index].Branches[_index].ItemToSubmit.item)
                                            {
                                                EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    "道具名称", this.dialogue.Words[index].Branches[_index].ItemToSubmit.ItemName);
                                                _lineCount++;
                                            }
                                            SerializedProperty amounts = itemToSubmit.FindPropertyRelative("amount");
                                            EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                amounts, new GUIContent("需提交的数量"));
                                            _lineCount++;
                                            if (amounts.intValue < 1) amounts.intValue = 1;
                                        }
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            itemCanGet.FindPropertyRelative("item"), new GUIContent("可获得的道具"));
                                        _lineCount++;
                                        if (this.dialogue.Words[index].Branches[_index].ItemCanGet.item)
                                        {
                                            EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                "道具名称", this.dialogue.Words[index].Branches[_index].ItemCanGet.ItemName);
                                            _lineCount++;
                                        }
                                        SerializedProperty amountg = itemCanGet.FindPropertyRelative("amount");
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            amountg, new GUIContent("可获得的数量"));
                                        _lineCount++;
                                        if (amountg.intValue < 1) amountg.intValue = 1;
                                        if (optionType.intValue == (int)WordsOptionType.OnlyGet)
                                        {
                                            EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                onlyForQuest, new GUIContent("只在任务时显示"));
                                            _lineCount++;
                                            if (onlyForQuest.boolValue)
                                            {
                                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                    bindedQuest, new GUIContent("相关任务"));
                                                _lineCount++;
                                                if (this.dialogue.Words[index].Branches[_index].BindedQuest)
                                                {
                                                    EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                        "任务标题", this.dialogue.Words[index].Branches[_index].BindedQuest.Title);
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
                                        b_talkerName = this.dialogue.Words[index].Branches[_index] == null ? "(空)" : !this.dialogue.Words[index].TalkerInfo ?
                                        "(空谈话人)" : this.dialogue.Words[index].Branches[_index].TalkerName + "说";
                                    else b_talkerName = "玩家说";
                                    EditorGUI.LabelField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight), b_talkerName);
                                    EditorGUI.PropertyField(new Rect(_rect.x + _rect.width / 2, _rect.y + lineHeightSpace * _lineCount, _rect.width / 2, lineHeight),
                                        b_talkerType, new GUIContent(string.Empty));
                                    _lineCount++;
                                    if (b_talkerType.enumValueIndex == (int)TalkerType.NPC)
                                    {
                                        if (this.dialogue.Words[index].Branches[_index].TalkerInfo) b_talkerInfo.objectReferenceValue =
                                            npcs[EditorGUI.Popup(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            "谈话人", GetNPCIndex(this.dialogue.Words[index].Branches[_index].TalkerInfo), npcNames)];
                                        else if (npcs.Length > 0) b_talkerInfo.objectReferenceValue =
                                             npcs[EditorGUI.Popup(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight), "谈话人", 0, npcNames)];
                                        else EditorGUI.Popup(new Rect(_rect.x + _rect.width / 2f, _rect.y, _rect.width / 2f, lineHeight), "谈话人", 0, new string[] { "无可用谈话人" });
                                        _lineCount++;
                                        GUI.enabled = false;
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            b_talkerInfo, new GUIContent("引用资源"));
                                        GUI.enabled = true;
                                        _lineCount++;
                                    }
                                    b_words.stringValue = EditorGUI.TextField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        b_words.stringValue);
                                    _lineCount++;
                                }
                                if ((optionType.intValue != (int)WordsOptionType.SubmitAndGet || optionType.intValue != (int)WordsOptionType.OnlyGet) && indexOfRrightBranch.intValue < 0)
                                {
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        goBack, new GUIContent("返回至原对话"));
                                    _lineCount++;
                                    if (goBack.boolValue && optionType.intValue != (int)WordsOptionType.Choice)//选择型选项无法指定返回序号
                                    {
                                        indexToGo.intValue = EditorGUI.IntSlider(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            "返回至的对话序号", indexToGo.intValue, -1, dialogWords.arraySize - 1);
                                        _lineCount++;
                                    }
                                }
                            }
                            if (optionType.intValue == (int)WordsOptionType.Choice)
                            {
                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                    deleteWhenCmplt, new GUIContent("对话完成时删除"));
                                _lineCount++;
                            }
                        }
                        if (EditorGUI.EndChangeCheck())
                            branches.serializedObject.ApplyModifiedProperties();
                    };

                    branchesList.elementHeightCallback = (_index) =>
                    {
                        int _lineCount = 1;
                        SerializedProperty branch = branches.GetArrayElementAtIndex(_index);
                        SerializedProperty optionType = branch.FindPropertyRelative("optionType");
                        SerializedProperty b_talkerType = branch.FindPropertyRelative("talkerType");
                        SerializedProperty b_talkerInfo = branch.FindPropertyRelative("talkerInfo");
                        SerializedProperty b_words = branch.FindPropertyRelative("words");
                        SerializedProperty dialogue = branch.FindPropertyRelative("dialogue");
                        SerializedProperty goBack = branch.FindPropertyRelative("goBack");
                        if (branch.isExpanded)
                        {
                            _lineCount++;//标题
                            if (optionType.intValue == (int)WordsOptionType.Choice)
                            {
                                _lineCount++;//有话
                            }
                            if (!(optionType.intValue == (int)WordsOptionType.Choice && !branch.FindPropertyRelative("hasWords").boolValue))
                            {
                                if (optionType.intValue == (int)WordsOptionType.BranchDialogue)
                                {
                                    _lineCount++;//对话
                                    if (this.dialogue.Words[index].Branches[_index].Dialogue)
                                        _lineCount += 2;//句子序号、序号句子
                                }
                                else
                                {
                                    if (optionType.intValue == (int)WordsOptionType.SubmitAndGet || optionType.intValue == (int)WordsOptionType.OnlyGet)
                                    {
                                        if (optionType.intValue == (int)WordsOptionType.SubmitAndGet)
                                        {
                                            _lineCount++;//需提交
                                            if (this.dialogue.Words[index].Branches[_index].ItemToSubmit.item)
                                                _lineCount++;//道具名称
                                            _lineCount++;//数量
                                        }
                                        _lineCount++;//可获得
                                        if (this.dialogue.Words[index].Branches[_index].ItemCanGet.item)
                                            _lineCount++;//道具名称
                                        _lineCount++;//可获得数量
                                        if (optionType.intValue == (int)WordsOptionType.OnlyGet)
                                        {
                                            _lineCount++;//只在
                                            if (branch.FindPropertyRelative("onlyForQuest").boolValue)
                                            {
                                                _lineCount++;//任务
                                                if (branch.FindPropertyRelative("bindedQuest").objectReferenceValue)
                                                    _lineCount++;//任务名
                                            }
                                        }
                                        _lineCount++;//对话粗体
                                    }
                                    _lineCount++;//谈话人名字
                                    if (b_talkerType.enumValueIndex == (int)TalkerType.NPC)
                                        _lineCount += 2;//谈话人选择、资源
                                    _lineCount++;//说的话
                                }
                                if ((optionType.intValue != (int)WordsOptionType.SubmitAndGet || optionType.intValue != (int)WordsOptionType.OnlyGet) && indexOfRrightBranch.intValue < 0)
                                {
                                    _lineCount++;//返回
                                    if (goBack.boolValue && optionType.intValue != (int)WordsOptionType.Choice)
                                        _lineCount++;//返回序号
                                }
                            }
                            if (optionType.intValue == (int)WordsOptionType.Choice)
                                _lineCount++;//完成删除
                        }
                        return _lineCount * lineHeightSpace;
                    };

                    branchesList.onAddCallback = (_list) =>
                    {
                        branches.serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        if (_list.index >= 0 && _list.index < dialogue.Words[index].Branches.Count)
                            dialogue.Words[index].Branches.Insert(_list.index, null);
                        else dialogue.Words[index].Branches.Add(null);
                        if (EditorGUI.EndChangeCheck())
                            branches.serializedObject.ApplyModifiedProperties();
                    };

                    branchesList.onRemoveCallback = (_list) =>
                    {
                        branches.serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        if (EditorUtility.DisplayDialog("删除", "确定删除这个分支吗？", "确定", "取消"))
                            branches.DeleteArrayElementAtIndex(_list.index);
                        if (EditorGUI.EndChangeCheck())
                            branches.serializedObject.ApplyModifiedProperties();
                    };

                    branchesList.drawHeaderCallback = (_rect) =>
                    {
                        int notCmpltCount = dialogue.Words[index].Branches.FindAll(x => !x.IsValid).Count;
                        EditorGUI.LabelField(_rect, "选项列表", "数量：" + dialogue.Words[index].Branches.Count.ToString() +
                            (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                    };

                    branchesList.drawNoneElementCallback = (_rect) =>
                    {
                        EditorGUI.LabelField(_rect, "空列表");
                    };
                }
                else branchesList = wordsBranchesLists[dialogue.Words[index]];
                words.serializedObject.Update();
                branchesList.DoList(new Rect(rect.x, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width, lineHeight * (branches.arraySize + 1)));
                words.serializedObject.ApplyModifiedProperties();
            }
        };

        wordsList.elementHeightCallback = (int index) =>
        {
            int lineCount = 1;
            if (dialogue.Words[index].TalkerType == TalkerType.NPC) lineCount += 2;//NPC、NPC名字
            lineCount += 4;//对话、分支
            SerializedProperty wordsSP = dialogWords.GetArrayElementAtIndex(index);
            SerializedProperty branches = wordsSP.FindPropertyRelative("branches");
            SerializedProperty indexOfRrightBranch = wordsSP.FindPropertyRelative("indexOfRrightBranch");
            float totalListHeight = 0.0f;
            if (branches.isExpanded)
            {
                if (branches.arraySize > 1)
                    if (dialogue.Words[index].Branches.TrueForAll(x => x && x.OptionType == WordsOptionType.Choice))
                    {
                        lineCount++;//正确分支
                        if (indexOfRrightBranch.intValue > -1)
                            lineCount += 2;//说的话、说的话填写
                    }
                if (wordsBranchesLists.ContainsKey(dialogue.Words[index]))
                    totalListHeight += wordsBranchesLists[dialogue.Words[index]].GetHeight();
            }
            return lineCount * lineHeightSpace + totalListHeight - 8;
        };

        wordsList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (list.index >= 0 && list.index < dialogue.Words.Count)
                dialogue.Words.Insert(list.index, new DialogueWords());
            else dialogue.Words.Add(new DialogueWords());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        wordsList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这句话吗？", "确定", "取消")) dialogue.Words.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        };

        wordsList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = dialogue.Words.FindAll(x => !x.TalkerInfo || string.IsNullOrEmpty(x.Words) || x.Branches.Exists(y => y && !y.IsValid)).Count;
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
        if (npcs.Contains(npc))
            return Array.IndexOf(npcs, npc);
        else return 0;
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