using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZetanStudio.CharacterSystem.Editor
{
    using DialogueSystem;
    using QuestSystem;

    public partial class CharacterInfoInspector
    {
        TalkerInformation talker;
        SerializedProperty defalutDialogue;
        SerializedProperty conditionDialogues;
        SerializedProperty canDEV_RLAT;
        SerializedProperty normalItemDialogue;
        SerializedProperty normalIntimacyValue;
        SerializedProperty giftDialogues;
        SerializedProperty affectiveItems;
        SerializedProperty canMarry;
        SerializedProperty isWarehouseAgent;
        SerializedProperty warehouseCapcity;
        SerializedProperty isVendor;
        SerializedProperty shop;

        SerializedProperty questsStored;

        int barIndex;

        ReorderableList conditionDialogList;
        ReorderableList giftDialoguesList;
        ReorderableList affectiveItemsList;
        ReorderableList questList;

        Dictionary<ConditionDialogue, ConditionGroupDrawer> conditionDrawers;

        List<TalkerInformation> allTalkers;

        void TalkerInfoEnable()
        {
            if (talker)
            {
                allTalkers = Resources.LoadAll<TalkerInformation>("Configuration").ToList();
                allTalkers.Remove(talker);
            }
            defalutDialogue = serializedObject.FindProperty("defaultDialogue");
            conditionDialogues = serializedObject.FindProperty("conditionDialogues");
            canDEV_RLAT = serializedObject.FindProperty("canDEV_RLAT");
            normalItemDialogue = serializedObject.FindProperty("normalItemDialogue");
            normalIntimacyValue = serializedObject.FindProperty("normalIntimacyValue");
            giftDialogues = serializedObject.FindProperty("giftDialogues");
            affectiveItems = serializedObject.FindProperty("affectiveItems");
            canMarry = serializedObject.FindProperty("canMarry");
            isWarehouseAgent = serializedObject.FindProperty("isWarehouseAgent");
            isVendor = serializedObject.FindProperty("isVendor");
            warehouseCapcity = serializedObject.FindProperty("warehouseCapcity");
            shop = serializedObject.FindProperty("shop");
            questsStored = serializedObject.FindProperty("questsStored");
            conditionDrawers = new Dictionary<ConditionDialogue, ConditionGroupDrawer>();
            HandlingConditionDialogueList();
            HandlingGiftDialogueList();
            HandlingAffectiveItemsList();
            HandlingQuestList();
        }

        void TalkerInfoHeader()
        {
            if (talker.AffectiveItems.Exists(x => talker.AffectiveItems.Find(y => y.Item == x.Item && y != x)))
                EditorGUILayout.HelpBox("喜讨道具存在重复！", MessageType.Warning);
            else if (!talker.IsValid)
                EditorGUILayout.HelpBox("该NPC信息未补全。", MessageType.Warning);
            else EditorGUILayout.HelpBox("该NPC信息已完整。", MessageType.Info);
        }

        void DrawTalkerInfo()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            bool enableBef = enable.boolValue;
            EditorGUILayout.PropertyField(enable, new GUIContent("启用", "若启用，将在场景中生成实体"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (enable.boolValue)
            {
                barIndex = GUILayout.Toolbar(barIndex, new string[] { "基本", "对话", "功能", "任务", "亲密度" });
                EditorGUILayout.Space();
            }
            if (enableBef != enable.boolValue) barIndex = 0;
            switch (barIndex)
            {
                case 0:
                    #region case 0
                    serializedObject.UpdateIfRequiredOrScript();
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
                    EditorGUILayout.PropertyField(_name, new GUIContent("名称"));
                    EditorGUILayout.PropertyField(sex, new GUIContent("性别"));
                    if (enable.boolValue)
                    {
                        sceneSelector.DoLayoutDraw();
                        EditorGUILayout.PropertyField(position, new GUIContent("位置"));
                        EditorGUILayout.PropertyField(prefab, new GUIContent("预制件"));
                        EditorGUILayout.PropertyField(SMParams, new GUIContent("状态机参数"));
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    #endregion
                    break;
                case 1:
                    #region case 1
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(defalutDialogue, new GUIContent("默认对话"));
                    //if (talker.DefaultDialogue)
                    //{
                    //    if (talker.DefaultDialogue.Words.Exists(x => x.NeedToChusCorrectOption))
                    //        EditorGUILayout.HelpBox("该对话有选择型分支，不建议用作默认对话。", MessageType.Warning);
                    //    string dialogue = string.Empty;
                    //    for (int i = 0; i < talker.DefaultDialogue.Words.Count; i++)
                    //    {
                    //        var words = talker.DefaultDialogue.Words[i];
                    //        dialogue += "[" + words.TalkerName + "]说：\n-" + words.Content;
                    //        for (int j = 0; j < words.Options.Count; j++)
                    //        {
                    //            dialogue += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                    //        }
                    //        dialogue += i == talker.DefaultDialogue.Words.Count - 1 ? string.Empty : "\n";
                    //    }
                    //    GUI.enabled = false;
                    //    EditorGUILayout.TextArea(dialogue);
                    //    GUI.enabled = true;
                    //}
                    EditorGUILayout.PropertyField(conditionDialogues, new GUIContent("条件触发对话" + (conditionDialogues.isExpanded ? string.Empty : "\t\t数量：" + conditionDialogues.arraySize.ToString())), false);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    if (conditionDialogues.isExpanded)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        conditionDialogList.DoLayoutList();
                        serializedObject.ApplyModifiedProperties();
                    }
                    #endregion
                    break;
                case 2:
                    #region case 2
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (talker)
                    {
                        EditorGUILayout.PropertyField(isWarehouseAgent, new GUIContent("是仓库管理员"));
                        if (isWarehouseAgent.boolValue)
                        {
                            EditorGUILayout.PropertyField(warehouseCapcity, new GUIContent("仓库容量"));
                        }
                        EditorGUILayout.PropertyField(isVendor, new GUIContent("是商贩"));
                        if (isVendor.boolValue)
                        {
                            EditorGUILayout.PropertyField(shop, new GUIContent("商铺信息"));
                            if (talker.Shop)
                            {
                                EditorGUILayout.BeginVertical("Box");
                                EditorGUILayout.LabelField("商店名称", talker.Shop.ShopName);
                                if (talker.Shop.Commodities.Count > 0)
                                {
                                    EditorGUILayout.LabelField("商品列表", new GUIStyle { fontStyle = FontStyle.Bold });
                                    for (int i = 0; i < talker.Shop.Commodities.Count; i++)
                                        if (talker.Shop.Commodities[i].IsValid)
                                            EditorGUILayout.LabelField("商品 " + (i + 1), talker.Shop.Commodities[i].Item.Name);
                                }
                                if (talker.Shop.Acquisitions.Count > 0)
                                {
                                    EditorGUILayout.LabelField("收购品列表", new GUIStyle { fontStyle = FontStyle.Bold });
                                    for (int i = 0; i < talker.Shop.Acquisitions.Count; i++)
                                        if (talker.Shop.Acquisitions[i].IsValid)
                                            EditorGUILayout.LabelField("收购品 " + (i + 1), talker.Shop.Acquisitions[i].Item.Name);
                                }
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    #endregion
                    break;
                case 3:
                    #region case 3
                    if (talker)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        questList.DoLayoutList();
                        serializedObject.ApplyModifiedProperties();
                    }
                    #endregion
                    break;
                case 4:
                    #region case 4
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(canDEV_RLAT, new GUIContent("可培养感情"));
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    if (canDEV_RLAT.boolValue)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(normalItemDialogue, new GUIContent("赠送中性物品时的对话"));
                        //if (talker.NormalItemDialogue)
                        //{
                        //    string dialogue = string.Empty;
                        //    for (int i = 0; i < talker.NormalItemDialogue.Words.Count; i++)
                        //    {
                        //        var words = talker.NormalItemDialogue.Words[i];
                        //        dialogue += "[" + words.TalkerName + "]说：\n-" + words.Content;
                        //        for (int j = 0; j < words.Options.Count; j++)
                        //        {
                        //            dialogue += "\n--(选项" + (j + 1) + ")" + words.Options[j].Title;
                        //        }
                        //        dialogue += i == talker.NormalItemDialogue.Words.Count - 1 ? string.Empty : "\n";
                        //    }
                        //    GUI.enabled = false;
                        //    EditorGUILayout.TextArea(dialogue);
                        //    GUI.enabled = true;
                        //}
                        EditorGUILayout.PropertyField(normalIntimacyValue, new GUIContent("中性物品增加的亲密值"));
                        EditorGUILayout.PropertyField(giftDialogues, new GUIContent("赠送物品时的对话\t\t" + (giftDialogues.arraySize > 0 ? "数量：" + giftDialogues.arraySize : "无")), false);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                        if (giftDialogues.isExpanded)
                        {
                            serializedObject.UpdateIfRequiredOrScript();
                            giftDialoguesList.DoLayoutList();
                            serializedObject.ApplyModifiedProperties();
                        }
                        EditorGUILayout.PropertyField(affectiveItems, new GUIContent("亲密值道具\t\t" + (affectiveItems.arraySize > 0 ? "数量：" + affectiveItems.arraySize : "无")), false);
                        if (affectiveItems.isExpanded)
                        {
                            serializedObject.UpdateIfRequiredOrScript();
                            affectiveItemsList.DoLayoutList();
                            serializedObject.ApplyModifiedProperties();
                        }
                        serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(canMarry, new GUIContent("可结婚"));
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                    #endregion
                    break;
            }
        }

        void HandlingConditionDialogueList()
        {
            conditionDialogList = new ReorderableList(serializedObject, conditionDialogues, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty conditionDialogue = conditionDialogues.GetArrayElementAtIndex(index);
                    SerializedProperty dialogue = conditionDialogue.FindPropertyRelative("dialogue");
                    SerializedProperty condition = conditionDialogue.FindPropertyRelative("condition");
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2, lineHeight),
                        conditionDialogue, new GUIContent("触发对话" + (conditionDialogue.isExpanded ? string.Empty : "(展开填写条件)")), false);
                    EditorGUI.PropertyField(new Rect(rect.x + 8 + rect.width / 2, rect.y, rect.width / 2 - 8, lineHeight),
                        dialogue, new GUIContent(string.Empty));
                    int lineCount = 1;
                    if (dialogue.objectReferenceValue)
                    {
                        Dialogue dialog = dialogue.objectReferenceValue as Dialogue;
                        if (dialog.Entry)
                        {
                            GUI.enabled = false;
                            EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), $"[{dialog.Entry.Talker}]说：{dialog.Entry.Text}");
                            GUI.enabled = true;
                            lineCount++;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    if (conditionDialogue.isExpanded)
                    {
                        if (!conditionDrawers.TryGetValue(talker.ConditionDialogues[index], out var drawer))
                        {
                            drawer = new ConditionGroupDrawer(condition, lineHeight, lineHeightSpace);
                            conditionDrawers.Add(talker.ConditionDialogues[index], drawer);
                        }
                        serializedObject.UpdateIfRequiredOrScript();
                        drawer.DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                        serializedObject.ApplyModifiedProperties();
                    }
                },

                elementHeightCallback = (index) =>
                {
                    SerializedProperty conditionDialogue = conditionDialogues.GetArrayElementAtIndex(index);
                    SerializedProperty dialogue = conditionDialogue.FindPropertyRelative("dialogue");
                    int lineCount = 1;
                    if (dialogue.objectReferenceValue)
                        lineCount++;
                    float listHeight = 0;
                    if (conditionDialogue.isExpanded)
                    {
                        if (conditionDrawers.TryGetValue(talker.ConditionDialogues[index], out var drawer))
                            listHeight = drawer.GetDrawHeight();
                    }
                    return lineHeightSpace * lineCount + listHeight;
                },

                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (EditorUtility.DisplayDialog("删除", "确定删除这个对话吗？", "确定", "取消"))
                    {
                        conditionDrawers.Remove(talker.ConditionDialogues[list.index]);
                        conditionDialogues.DeleteArrayElementAtIndex(list.index);
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                },

                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },

                drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = talker.ConditionDialogues.FindAll(x => !x.Dialogue || !x.Condition.IsValid).Count;
                    EditorGUI.LabelField(rect, "条件对话列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
                },

                drawNoneElementCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "空列表");
                }
            };
        }

        void HandlingGiftDialogueList()
        {
            giftDialoguesList = new ReorderableList(serializedObject, giftDialogues, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty giftDialogue = giftDialogues.GetArrayElementAtIndex(index);
                    SerializedProperty dialogue = giftDialogue.FindPropertyRelative("dialogue");
                    SerializedProperty lowerBound = giftDialogue.FindPropertyRelative("lowerBound");
                    SerializedProperty upperBound = giftDialogue.FindPropertyRelative("upperBound");
                    EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width / 2, lineHeight),
                        giftDialogue, new GUIContent($"对话[{lowerBound.intValue}~{upperBound.intValue}]"));
                    EditorGUI.PropertyField(new Rect(rect.x + 8 + rect.width / 2, rect.y, rect.width / 2 - 8, lineHeight),
                        dialogue, new GUIContent(string.Empty));
                    int lineCount = 1;
                    if (dialogue.objectReferenceValue)
                    {
                        Dialogue dialog = dialogue.objectReferenceValue as Dialogue;
                        if (dialog.Entry)
                        {
                            GUI.enabled = false;
                            EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), $"[{dialog.Entry.Talker}]说：{dialog.Entry.Text}");
                            GUI.enabled = true;
                            lineCount++;
                        }
                    }
                    if (giftDialogue.isExpanded)
                    {
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            lowerBound, new GUIContent("增加值高于此值时"));
                        lineCount++;
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                            upperBound, new GUIContent("增加值低于此值时"));
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                },

                elementHeightCallback = (index) =>
                {
                    SerializedProperty giftDialogue = giftDialogues.GetArrayElementAtIndex(index);
                    SerializedProperty dialogue = giftDialogue.FindPropertyRelative("dialogue");
                    int lineCount = 2;
                    if (giftDialogue.isExpanded)
                    {
                        lineCount++;
                        if (dialogue.objectReferenceValue)
                            lineCount++;
                    }
                    return lineHeightSpace * lineCount;
                },

                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (EditorUtility.DisplayDialog("删除", "确定删除这个对话吗？", "确定", "取消"))
                        giftDialogues.DeleteArrayElementAtIndex(list.index);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                },

                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },

                drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = talker.AffectiveItems.FindAll(x => !x.Item).Count;
                    EditorGUI.LabelField(rect, "送礼对话列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
                },

                drawNoneElementCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "空列表");
                }
            };
        }

        void HandlingAffectiveItemsList()
        {
            affectiveItemsList = new ReorderableList(serializedObject, affectiveItems, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    if (talker.AffectiveItems[index].Item != null)
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), talker.AffectiveItems[index].Item.Name);
                    else
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty favoriteItem = affectiveItems.GetArrayElementAtIndex(index);
                    SerializedProperty item = favoriteItem.FindPropertyRelative("item");
                    SerializedProperty intimacyValue = favoriteItem.FindPropertyRelative("intimacyValue");
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                        item, new GUIContent(string.Empty));
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                        intimacyValue, new GUIContent("亲密值"));
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                },

                elementHeightCallback = (index) =>
                {
                    return 2 * lineHeightSpace;
                },

                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
                        affectiveItems.DeleteArrayElementAtIndex(list.index);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                },

                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },

                drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = talker.AffectiveItems.FindAll(x => !x.Item).Count;
                    EditorGUI.LabelField(rect, "亲密值道具列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
                },

                drawNoneElementCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "空列表");
                }
            };
        }

        void HandlingQuestList()
        {
            questList = new ReorderableList(serializedObject, questsStored, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    SerializedProperty quest = questsStored.GetArrayElementAtIndex(index);
                    if (quest.objectReferenceValue != null)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), talker.QuestsStored[index].Title);
                        if (GUI.Button(new Rect(rect.x + rect.width * 0.8f, rect.y, rect.width * 0.2f, lineHeight), "编辑"))
                        {
                            EditorUtility.OpenPropertyEditor(quest.objectReferenceValue);
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
                        if (GUI.Button(new Rect(rect.x + rect.width * 0.8f, rect.y, rect.width * 0.2f, lineHeight), "新建"))
                        {
                            Quest questInstance = Utility.Editor.SaveFilePanel(CreateInstance<Quest>, "quest", ping: true);
                            if (questInstance)
                            {
                                quest.objectReferenceValue = questInstance;
                                SerializedObject newQuest = new SerializedObject(quest.objectReferenceValue);
                                SerializedProperty _ID = newQuest.FindProperty("_ID");
                                SerializedProperty title = newQuest.FindProperty("title");
                                _ID.stringValue = Quest.GetAutoID();
                                title.stringValue = $"{_name.stringValue}的委托";
                                newQuest.ApplyModifiedProperties();

                                EditorUtility.OpenPropertyEditor(questInstance);
                            }
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    Quest qBef = quest.objectReferenceValue as Quest;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), quest, new GUIContent(string.Empty));
                    if (qBef != quest.objectReferenceValue as Quest && talker.QuestsStored.Contains(quest.objectReferenceValue as Quest))
                    {
                        EditorUtility.DisplayDialog("错误", "已添加该任务。", "确定");
                        quest.objectReferenceValue = qBef;
                    }
                    else
                    {
                        var conflictTalker = allTalkers.Find(x => x.QuestsStored.Count > 0 && x.QuestsStored.Contains(quest.objectReferenceValue as Quest));
                        if (conflictTalker && conflictTalker != talker)
                        {
                            EditorUtility.DisplayDialog("错误", "[" + conflictTalker.Name + "]已使用该任务，无法添加。", "确定");
                            quest.objectReferenceValue = qBef;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                },

                elementHeightCallback = (int index) =>
                {
                    return 2 * lineHeightSpace;
                },

                onAddCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (list.index + 1 < talker.QuestsStored.Count)
                        talker.QuestsStored.Insert(list.index + 1, null);
                    else talker.QuestsStored.Add(null);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    if (EditorUtility.DisplayDialog("删除", "确定删除这个任务吗？", "确定", "取消"))
                        talker.QuestsStored.RemoveAt(list.index);
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                },

                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },

                drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = talker.QuestsStored.FindAll(x => !x).Count;
                    EditorGUI.LabelField(rect, "任务列表", "数量：" + talker.QuestsStored.Count + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
                },

                drawNoneElementCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "空列表");
                }
            };
        }
    }
}