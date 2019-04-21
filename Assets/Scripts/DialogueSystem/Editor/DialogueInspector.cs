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

        _ID = serializedObject.FindProperty("_ID");
        dialogWords = serializedObject.FindProperty("words");

        HandlingWordsList();
    }

    public override void OnInspectorGUI()
    {
        if (dialogue.Words.Exists(x => x.TalkerType == TalkerType.NPC && (!x.TalkerInfo || string.IsNullOrEmpty(x.Words)) ||
            x.Branches.Exists(y => y != null && (!y.Dialogue || string.IsNullOrEmpty(y.Title)))))
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
            SerializedProperty wordsSP = this.dialogWords.GetArrayElementAtIndex(index);
            SerializedProperty talkerType = wordsSP.FindPropertyRelative("talkerType");
            SerializedProperty talkerInfo = wordsSP.FindPropertyRelative("talkerInfo");
            SerializedProperty words = wordsSP.FindPropertyRelative("words");
            SerializedProperty indexOfRrightBranch = wordsSP.FindPropertyRelative("indexOfRrightBranch");
            SerializedProperty wordsWhenChusWB = wordsSP.FindPropertyRelative("wordsWhenChusWB");
            SerializedProperty branches = wordsSP.FindPropertyRelative("branches");

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
            lineCount += 2;
            EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                branches, new GUIContent("分支"));
            lineCount++;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (branches.isExpanded)
            {
                if (index != dialogWords.arraySize - 1)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    if (branches.arraySize > 1)
                    {
                        indexOfRrightBranch.intValue = EditorGUI.IntSlider(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                            "正确分支序号", indexOfRrightBranch.intValue, -1, branches.arraySize - 1);
                        lineCount++;
                        if (indexOfRrightBranch.intValue > -1)
                        {
                            EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight), "分支选择错误时说的话：");
                            lineCount++;
                            wordsWhenChusWB.stringValue = EditorGUI.TextArea(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight),
                                wordsWhenChusWB.stringValue);
                            lineCount++;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    ReorderableList branchesList;
                    if (!wordsBranchesLists.ContainsKey(dialogue.Words[index]))
                    {
                        branchesList = new ReorderableList(wordsSP.serializedObject, branches, true, true, true, true);
                        wordsBranchesLists.Add(dialogue.Words[index], branchesList);
                        branchesList.drawElementCallback = (_rect, _index, _isActive, _isFocused) =>
                        {
                            wordsSP.serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            SerializedProperty branch = branches.GetArrayElementAtIndex(_index);
                            SerializedProperty title = branch.FindPropertyRelative("title");
                            SerializedProperty dialogue = branch.FindPropertyRelative("dialogue");
                            SerializedProperty specifyIndex = branch.FindPropertyRelative("specifyIndex");
                            SerializedProperty goBack = branch.FindPropertyRelative("goBack");
                            SerializedProperty indexToGo = branch.FindPropertyRelative("indexToGo");
                            SerializedProperty deleteWhenCmplt = branch.FindPropertyRelative("deleteWhenCmplt");
                            string label = string.IsNullOrEmpty(title.stringValue) ? "(空标题)" : title.stringValue;
                            EditorGUI.PropertyField(new Rect(_rect.x + 8, _rect.y, _rect.width / 2, lineHeight),
                                branch, new GUIContent(label));
                            if (branch.isExpanded)
                            {
                                int _lineCount = 1;
                                EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                    title, new GUIContent("分支标题"));
                                _lineCount++;
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
                                    if (indexOfRrightBranch.intValue < 0)
                                    {
                                        EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                            goBack, new GUIContent("返回至原对话"));
                                        _lineCount++;
                                        if (goBack.boolValue)
                                        {
                                            indexToGo.intValue = EditorGUI.IntSlider(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                                "返回至的对话序号", indexToGo.intValue, -1, dialogWords.arraySize - 1);
                                            _lineCount++;
                                        }
                                    }
                                    EditorGUI.PropertyField(new Rect(_rect.x, _rect.y + lineHeightSpace * _lineCount, _rect.width, lineHeight),
                                        deleteWhenCmplt, new GUIContent("对话完成时删除"));
                                }
                            }
                            if (EditorGUI.EndChangeCheck())
                                wordsSP.serializedObject.ApplyModifiedProperties();
                        };

                        branchesList.elementHeightCallback = (_index) =>
                        {
                            int _lineCount = 1;
                            SerializedProperty branch = branches.GetArrayElementAtIndex(_index);
                            SerializedProperty dialogue = branch.FindPropertyRelative("dialogue");
                            if (branch.isExpanded)
                            {
                                _lineCount += 2;
                                if (this.dialogue.Words[index].Branches[_index].Dialogue)
                                {
                                    _lineCount += 2;
                                    if (indexOfRrightBranch.intValue < 0)
                                    {
                                        _lineCount++;
                                        if (branch.FindPropertyRelative("goBack").boolValue)
                                            _lineCount++;
                                    }
                                    _lineCount++;
                                }
                            }
                            return _lineCount * lineHeightSpace;
                        };

                        branchesList.onAddCallback = (_list) =>
                        {
                            serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            dialogue.Words[index].Branches.Add(null);
                            if (EditorGUI.EndChangeCheck())
                                serializedObject.ApplyModifiedProperties();
                        };

                        branchesList.onRemoveCallback = (_list) =>
                        {
                            serializedObject.Update();
                            EditorGUI.BeginChangeCheck();
                            if (EditorUtility.DisplayDialog("删除", "确定删除这个分支吗？", "确定", "取消"))
                            {
                                dialogue.Words[index].Branches.RemoveAt(_list.index);
                            }
                            if (EditorGUI.EndChangeCheck())
                                serializedObject.ApplyModifiedProperties();
                        };

                        branchesList.drawHeaderCallback = (rect2) =>
                        {
                            int notCmpltCount = dialogue.Words[index].Branches.FindAll(x => !x.Dialogue).Count;
                            EditorGUI.LabelField(rect2, "分支列表", "数量：" + dialogue.Words[index].Branches.Count.ToString() +
                                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                        };

                        branchesList.drawNoneElementCallback = (rect2) =>
                        {
                            EditorGUI.LabelField(rect2, "空列表");
                        };
                    }
                    else branchesList = wordsBranchesLists[dialogue.Words[index]];
                    wordsSP.serializedObject.Update();
                    branchesList.DoList(new Rect(rect.x, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width, lineHeight * (branches.arraySize + 1)));
                    wordsSP.serializedObject.ApplyModifiedProperties();
                }
                else EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + lineHeightSpace * lineCount + lineHeight - 5, rect.width - 8, lineHeight), "最后一句不支持分支");
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
                if (index != dialogWords.arraySize - 1)
                {
                    if (branches.arraySize > 0)
                    {
                        lineCount++;
                        if (indexOfRrightBranch.intValue > -1)
                            lineCount += 2;
                    }
                    if (wordsBranchesLists.ContainsKey(dialogue.Words[index]))
                        totalListHeight += wordsBranchesLists[dialogue.Words[index]].GetHeight();
                }
                else lineCount++;
            return lineCount * lineHeightSpace + totalListHeight - 8;
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
            if (EditorUtility.DisplayDialog("删除", "确定删除这句话吗？", "确定", "取消")) dialogue.Words.RemoveAt(list.index);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        };

        wordsList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = dialogue.Words.FindAll(x => !x.TalkerInfo || string.IsNullOrEmpty(x.Words) || x.Branches.Exists(y => !y.Dialogue)).Count;
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
        Dialogue[] quests = Resources.LoadAll<Dialogue>("");

        Dialogue find = Array.Find(quests, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != dialogue || (find == dialogue && Array.FindAll(quests, x => x.ID == _ID.stringValue).Length > 1);
    }
}