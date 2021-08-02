using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class CharacterInfoInspector
{
    EnemyInformation enemy;
    SerializedProperty dropItems;

    ReorderableList dropItemList;

    void EnemyInfoEnable()
    {
        dropItems = serializedObject.FindProperty("dropItems");
        HandlingDropItemList();
    }

    void EnemyHeader()
    {
        if (string.IsNullOrEmpty(enemy.name) || string.IsNullOrEmpty(enemy.ID) || enemy.DropItems.Exists(x => !x.Item))
            EditorGUILayout.HelpBox("该敌人信息未补全。", MessageType.Warning);
        else EditorGUILayout.HelpBox("该敌人信息已完整。", MessageType.Info);
    }

    void DrawEnemyInfo()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        SerializedProperty race = serializedObject.FindProperty("race");
        EditorGUILayout.PropertyField(race, new GUIContent("种族"));
        if (serializedObject.FindProperty("race").objectReferenceValue)
            EditorGUILayout.LabelField("种族名称", (race.objectReferenceValue as EnemyRace).name);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(dropItems, new GUIContent("掉落道具\t\t" + (dropItems.arraySize > 0 ? "数量：" + dropItems.arraySize : "无")), false);
        if (dropItems.isExpanded)
        {
            serializedObject.Update();
            dropItemList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
        if (dropItems.arraySize > 0)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            SerializedProperty lootPrefab = serializedObject.FindProperty("lootPrefab");
            EditorGUILayout.PropertyField(lootPrefab, new GUIContent("掉落道具预制件"));
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
                EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent(enemy.DropItems[index].ItemName));
            else
                EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent("(空)"));
            EditorGUI.BeginChangeCheck();
            SerializedProperty item = itemInfo.FindPropertyRelative("item");
            SerializedProperty minAmount = itemInfo.FindPropertyRelative("minAmount");
            SerializedProperty maxAmount = itemInfo.FindPropertyRelative("maxAmount");
            SerializedProperty dropRate = itemInfo.FindPropertyRelative("dropRate");
            SerializedProperty onlyDropForQuest = itemInfo.FindPropertyRelative("onlyDropForQuest");
            SerializedProperty binedQuest = itemInfo.FindPropertyRelative("bindedQuest");
            EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                item, new GUIContent(string.Empty));
            if (itemInfo.isExpanded)
            {
                int lineCount = 1;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
                    dropRate, new GUIContent("掉落概率百分比"));
                if (dropRate.floatValue < 0) dropRate.floatValue = 0.0f;
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    minAmount, new GUIContent("最少掉落"));
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    maxAmount, new GUIContent("最多掉落"));
                if (minAmount.intValue < 1) minAmount.intValue = 1;
                lineCount++;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                    onlyDropForQuest, new GUIContent("只在进行任务时掉落"));
                if (onlyDropForQuest.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                        binedQuest, new GUIContent(string.Empty));
                    if (binedQuest.objectReferenceValue)
                    {
                        lineCount++;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), "任务名称",
                            (binedQuest.objectReferenceValue as Quest).Title);
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
            enemy.DropItems.Add(new DropItemInfo() { MinAmount = 1, DropRate = 100.0f });
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
}