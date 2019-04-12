using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;

[CustomEditor(typeof(EnemyInfomation))]
public class EnemyInfoInspector : Editor
{
    EnemyInfomation enemy;
    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty items;

    ReorderableList dropItemList;

    float lineHeight;
    float lineHeightSpace;

    bool showDropItemList = true;

    private void OnEnable()
    {
        enemy = target as EnemyInfomation;
        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        items = serializedObject.FindProperty("dropItems");

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        HandlingDropItemList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
        if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
        {
            if (ExistsID())
                EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
            if (GUILayout.Button("自动生成识别码"))
            {
                _ID.stringValue = GetAutoID();
                EditorGUI.FocusTextInControl(null);
            }
        }
        EditorGUILayout.PropertyField(_Name, new GUIContent("名称"));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        showDropItemList = EditorGUILayout.Toggle("显示掉落道具列表", showDropItemList);
        if (showDropItemList)
        {
            serializedObject.Update();
            dropItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }

    void HandlingDropItemList()
    {
        dropItemList = new ReorderableList(serializedObject, items, true, true, true, true);
        showDropItemList = true;
        dropItemList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            serializedObject.Update();
            SerializedProperty itemInfo = items.GetArrayElementAtIndex(index);
            if (enemy.DropItems[index] != null && enemy.DropItems[index].Item != null)
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width, lineHeight), itemInfo, new GUIContent(enemy.DropItems[index].Name));
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
            if (items.GetArrayElementAtIndex(index).isExpanded)
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
            enemy.DropItems.Add(new DropItemInfo() { Amount = 1 , DropRate = 100.0f});
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
            EditorGUI.LabelField(rect, "掉落道具列表", "数量：" + enemy.DropItems.Count.ToString() +
                (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
        };

        dropItemList.drawNoneElementCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, "空列表");
        };
    }


    string GetAutoID()
    {
        string newID = string.Empty;
        EnemyInfomation[] enemies = Resources.LoadAll<EnemyInfomation>("");
        for (int i = 1; i < 1000; i++)
        {
            newID = "ENMY" + i.ToString().PadLeft(3, '0');
            if (!Array.Exists(enemies, x => x.ID == newID))
                break;
        }
        return newID;
    }

    bool ExistsID()
    {
        List<EnemyInfomation> enemies = new List<EnemyInfomation>();
        foreach (EnemyInfomation enemy in Resources.LoadAll<EnemyInfomation>(""))
        {
            enemies.Add(enemy);
        }

        EnemyInfomation find = enemies.Find(x => x.ID == _ID.stringValue);
        if (!find) return false;//若没有找到，则ID可用
        //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
        return find != enemy || (find == enemy && enemies.FindAll(x => x.ID == _ID.stringValue).Count > 1);
    }
}