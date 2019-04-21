using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;

[CustomEditor(typeof(CharacterInfomation), true)]
public class CharacterInfoInspector : Editor
{
    CharacterInfomation character;
    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty sex;

    EnemyInfomation enemy;
    SerializedProperty dropItems;

    TalkerInfomation talker;
    SerializedProperty defalutDialogue;
    SerializedProperty canDEV_RLAT;
    SerializedProperty favoriteItemDialogue;
    SerializedProperty normalItemDialogue;
    SerializedProperty hateItemDialogue;
    SerializedProperty favoriteItems;
    SerializedProperty hateItems;
    SerializedProperty canMarry;

    PlayerInfomation player;
    SerializedProperty backpack;

    ReorderableList dropItemList;
    ReorderableList favoriteItemList;
    ReorderableList hateItemList;

    float lineHeight;
    float lineHeightSpace;

    private void OnEnable()
    {
        character = target as CharacterInfomation;
        enemy = target as EnemyInfomation;
        talker = target as TalkerInfomation;
        player = target as PlayerInfomation;

        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        sex = serializedObject.FindProperty("sex");

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        if (enemy)
        {
            dropItems = serializedObject.FindProperty("dropItems");
            HandlingDropItemList();
        }
        else if (talker)
        {
            defalutDialogue = serializedObject.FindProperty("defaultDialogue");
            canDEV_RLAT = serializedObject.FindProperty("canDEV_RLAT");
            favoriteItemDialogue = serializedObject.FindProperty("favoriteItemDialogue");
            normalItemDialogue = serializedObject.FindProperty("normalItemDialogue");
            hateItemDialogue = serializedObject.FindProperty("hateItemDialogue");
            favoriteItems = serializedObject.FindProperty("favoriteItems");
            hateItems = serializedObject.FindProperty("hateItems");
            canMarry = serializedObject.FindProperty("canMarry");
            HandlingFavoriteItemList();
            HandlingHateItemList();
        }
        else if (player)
        {
            backpack = serializedObject.FindProperty("backpack");
        }
    }

    public override void OnInspectorGUI()
    {
        if (enemy)
        {
            if (string.IsNullOrEmpty(enemy.Name) || string.IsNullOrEmpty(enemy.ID) || enemy.DropItems.Exists(x => !x.Item))
                EditorGUILayout.HelpBox("该敌人信息未补全。", MessageType.Warning);
            else EditorGUILayout.HelpBox("该敌人信息已完整。", MessageType.Info);
        }
        else if (talker)
        {
            if (talker.FavoriteItems.Exists(x => x.Item && talker.HateItems.Exists(y => y.Item && x.Item == y.Item)))
                EditorGUILayout.HelpBox("喜爱道具与讨厌道具存在冲突。", MessageType.Warning);
            else if (string.IsNullOrEmpty(talker.Name) || string.IsNullOrEmpty(talker.ID) || !talker.DefaultDialogue ||
                talker.CanDEV_RLAT && (!talker.NormalItemDialogue ||
                (talker.FavoriteItems.Count > 0 && (!talker.FavoriteItemDialogue.Level_1 || !talker.FavoriteItemDialogue.Level_2 || !talker.FavoriteItemDialogue.Level_3 || !talker.FavoriteItemDialogue.Level_4)) ||
                (talker.HateItems.Count > 0 && (!talker.HateItemDialogue.Level_1 || !talker.HateItemDialogue.Level_2 || !talker.HateItemDialogue.Level_3 || !talker.HateItemDialogue.Level_4)) ||
                  talker.FavoriteItems.Exists(x => !x.Item) || talker.HateItems.Exists(x => !x.Item)))
                EditorGUILayout.HelpBox("该NPC信息未补全。", MessageType.Warning);
            else EditorGUILayout.HelpBox("该NPC信息已完整。", MessageType.Info);
        }
        else if (string.IsNullOrEmpty(character.Name) || string.IsNullOrEmpty(character.ID))
            EditorGUILayout.HelpBox("该角色信息未补全。", MessageType.Warning);
        else EditorGUILayout.HelpBox("该角色信息已完整。", MessageType.Info);
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("名称"));
        if (!enemy) EditorGUILayout.PropertyField(sex, new GUIContent("性别"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        if (enemy)
        {
            EditorGUILayout.PropertyField(dropItems, new GUIContent("掉落道具\t\t" + (dropItems.arraySize > 0 ? "数量：" + dropItems.arraySize : "无")));
            if (dropItems.isExpanded)
            {
                serializedObject.Update();
                dropItemList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }
        else if (talker)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(defalutDialogue, new GUIContent("默认对话"));
            if (talker.DefaultDialogue)
            {
                string dialogue = string.Empty;
                foreach (DialogueWords word in talker.DefaultDialogue.Words)
                {
                    dialogue += "[" + word.TalkerName + "]说：\n-" + word.Words;
                    dialogue += talker.DefaultDialogue.Words.IndexOf(word) == talker.DefaultDialogue.Words.Count - 1 ? string.Empty : "\n";
                }
                GUI.enabled = false;
                EditorGUILayout.TextArea(dialogue);
                GUI.enabled = true;
            }
            EditorGUILayout.PropertyField(canDEV_RLAT, new GUIContent("可培养感情"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (canDEV_RLAT.boolValue)
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(normalItemDialogue, new GUIContent("赠送中性物品时的对话"), true);
                if (talker.NormalItemDialogue)
                {
                    string dialogue = string.Empty;
                    foreach (DialogueWords word in talker.NormalItemDialogue.Words)
                    {
                        dialogue += "[" + word.TalkerName + "]说：\n-" + word.Words;
                        dialogue += talker.NormalItemDialogue.Words.IndexOf(word) == talker.NormalItemDialogue.Words.Count - 1 ? string.Empty : "\n";
                    }
                    GUI.enabled = false;
                    EditorGUILayout.TextArea(dialogue);
                    GUI.enabled = true;
                }
                EditorGUILayout.PropertyField(favoriteItemDialogue, new GUIContent("赠送喜爱物品时的对话"));
                if (favoriteItemDialogue.isExpanded)
                {
                    SerializedProperty level_1 = favoriteItemDialogue.FindPropertyRelative("level_1");
                    EditorGUILayout.PropertyField(level_1, new GUIContent("稍微喜欢时的对话"));
                    SerializedProperty level_2 = favoriteItemDialogue.FindPropertyRelative("level_2");
                    EditorGUILayout.PropertyField(level_2, new GUIContent("喜欢时的对话"));
                    SerializedProperty level_3 = favoriteItemDialogue.FindPropertyRelative("level_3");
                    EditorGUILayout.PropertyField(level_3, new GUIContent("对其着迷时的对话"));
                    SerializedProperty level_4 = favoriteItemDialogue.FindPropertyRelative("level_4");
                    EditorGUILayout.PropertyField(level_4, new GUIContent("为其疯狂时的对话"));
                }
                EditorGUILayout.PropertyField(hateItemDialogue, new GUIContent("赠送讨厌物品时的对话"));
                if (hateItemDialogue.isExpanded)
                {
                    SerializedProperty level_1 = hateItemDialogue.FindPropertyRelative("level_1");
                    EditorGUILayout.PropertyField(level_1, new GUIContent("不喜欢时的对话"));
                    SerializedProperty level_2 = favoriteItemDialogue.FindPropertyRelative("level_2");
                    EditorGUILayout.PropertyField(level_2, new GUIContent("稍微讨厌时的对话"));
                    SerializedProperty level_3 = favoriteItemDialogue.FindPropertyRelative("level_3");
                    EditorGUILayout.PropertyField(level_3, new GUIContent("讨厌时的对话"));
                    SerializedProperty level_4 = favoriteItemDialogue.FindPropertyRelative("level_4");
                    EditorGUILayout.PropertyField(level_4, new GUIContent("非常厌恶时的对话"));
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(favoriteItems, new GUIContent("喜爱的道具\t\t" + (favoriteItems.arraySize > 0 ? "数量：" + favoriteItems.arraySize : "无")));
                if (favoriteItems.isExpanded)
                {
                    serializedObject.Update();
                    favoriteItemList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.PropertyField(hateItems, new GUIContent("讨厌的道具\t\t" + (hateItems.arraySize > 0 ? "数量：" + hateItems.arraySize : "无")));
                if (hateItems.isExpanded)
                {
                    serializedObject.Update();
                    hateItemList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.Space();
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(canMarry, new GUIContent("可结婚"));
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }
        else if (player)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("背包信息");
            SerializedProperty backpackSize = backpack.FindPropertyRelative("backpackSize");
            SerializedProperty weightLoad = backpack.FindPropertyRelative("weightLoad");
            backpackSize.FindPropertyRelative("max").intValue = EditorGUILayout.IntSlider("默认容量(格)", backpackSize.FindPropertyRelative("max").intValue, 30, 200);
            weightLoad.FindPropertyRelative("max").floatValue = EditorGUILayout.Slider("默认负重(WL)", weightLoad.FindPropertyRelative("max").floatValue, 100, 1000);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }

    void HandlingDropItemList()
    {
        dropItemList = new ReorderableList(serializedObject, dropItems, true, true, true, true);
        dropItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            SerializedProperty itemInfo = dropItems.GetArrayElementAtIndex(index);
            if (enemy.DropItems[index] != null && enemy.DropItems[index].Item != null)
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), itemInfo, new GUIContent(enemy.DropItems[index].ItemName));
            else
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), itemInfo, new GUIContent("(空)"));
            EditorGUI.BeginChangeCheck();
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
            SerializedProperty dropRate = itemInfo.FindPropertyRelative("dropRate");
            SerializedProperty onlyDropForQuest = itemInfo.FindPropertyRelative("onlyDropForQuest");
            SerializedProperty binedQuest = itemInfo.FindPropertyRelative("bindedQuest");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            if (itemInfo.isExpanded)
            {
                int lineCount = 1;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    amount, new GUIContent("最大掉落数量"));
                if (amount.intValue < 1) amount.intValue = 1;
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    dropRate, new GUIContent("掉落概率百分比"));
                if (dropRate.floatValue < 0) dropRate.floatValue = 0.0f;
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    onlyDropForQuest, new GUIContent("只在进行任务时掉落"));
                lineCount++;
                if (onlyDropForQuest.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                        binedQuest, new GUIContent("相关任务"));
                    lineCount++;
                    if (binedQuest.objectReferenceValue)
                    {
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "任务名称",
                            (binedQuest.objectReferenceValue as Quest).Title);
                        lineCount++;
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        dropItemList.elementHeightCallback = (int index) =>
        {
            int lineCount = 1;
            if (dropItems.GetArrayElementAtIndex(index).isExpanded)
            {
                lineCount += 3;//数量、百分比、只在
                if (enemy.DropItems[index].OnlyDropForQuest)
                {
                    lineCount++;//任务
                    if (enemy.DropItems[index].BindedQuest)
                        lineCount++;//任务标题
                }
            }
            return lineCount * lineHeightSpace;
        };

        dropItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            enemy.DropItems.Add(new DropItemInfo() { Amount = 1, DropRate = 100.0f });
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        dropItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个掉落道具吗？", "确定", "取消"))
            {
                enemy.DropItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        dropItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = enemy.DropItems.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "掉落道具列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
        };

        dropItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingFavoriteItemList()
    {
        favoriteItemList = new ReorderableList(serializedObject, favoriteItems, true, true, true, true);
        favoriteItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (talker.FavoriteItems[index].Item != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), talker.FavoriteItems[index].Item.Name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty favoriteItem = favoriteItems.GetArrayElementAtIndex(index);
            SerializedProperty item = favoriteItem.FindPropertyRelative("item");
            SerializedProperty favoriteLevel = favoriteItem.FindPropertyRelative("favoriteLevel");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                favoriteLevel, new GUIContent("喜欢程度"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.elementHeightCallback = (index) =>
        {
            return 2 * lineHeightSpace;
        };

        favoriteItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            talker.FavoriteItems.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                talker.FavoriteItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        favoriteItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = talker.FavoriteItems.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "喜爱道具列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
        };

        favoriteItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    void HandlingHateItemList()
    {
        hateItemList = new ReorderableList(serializedObject, hateItems, true, true, true, true);
        hateItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            if (talker.HateItems[index].Item != null)
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), talker.HateItems[index].Item.Name);
            else
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
            EditorGUI.BeginChangeCheck();
            SerializedProperty hateItem = hateItems.GetArrayElementAtIndex(index);
            SerializedProperty item = hateItem.FindPropertyRelative("item");
            SerializedProperty hateLevel = hateItem.FindPropertyRelative("hateLevel");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                hateLevel, new GUIContent("讨厌程度"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.elementHeightCallback = (index) =>
        {
            return 2 * lineHeightSpace;
        };

        hateItemList.onAddCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            talker.HateItems.Add(null);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.onRemoveCallback = (list) =>
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
            {
                talker.HateItems.RemoveAt(list.index);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        };

        hateItemList.drawHeaderCallback = (rect) =>
        {
            int notCmpltCount = talker.HateItems.FindAll(x => !x.Item).Count;
            EditorGUI.LabelField(rect, "讨厌道具列表", notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
        };

        hateItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        if (enemy)
        {
            EnemyInfomation[] enemies = Resources.LoadAll<EnemyInfomation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "ENMY" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(enemies, x => x.ID == newID))
                    break;
            }
        }
        else if (talker)
        {
            TalkerInfomation[] talkers = Resources.LoadAll<TalkerInfomation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "NPC" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(talkers, x => x.ID == newID))
                    break;
            }
        }
        else if (player)
        {
            PlayerInfomation[] players = Resources.LoadAll<PlayerInfomation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "PLAY" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(players, x => x.ID == newID))
                    break;
            }
        }
        else
        {
            CharacterInfomation[] characters = Resources.LoadAll<CharacterInfomation>("").Where(x => !(x is EnemyInfomation) && !(x is TalkerInfomation)).ToArray();
            for (int i = 1; i < 1000; i++)
            {
                newID = "CHAR" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(characters, x => x.ID == newID))
                    break;
            }
        }
        return newID;
    }

    bool ExistsID()
    {
        CharacterInfomation[] characters = Resources.LoadAll<CharacterInfomation>("");

        CharacterInfomation find = Array.Find(characters, x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
                                //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != character || (find == character && Array.FindAll(characters, x => x.ID == _ID.stringValue).Length > 1);
    }
}