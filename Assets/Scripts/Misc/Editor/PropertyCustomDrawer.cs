using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RoleAttributeGroupDrawer
{
    private readonly SerializedObject owner;

    public ReorderableList List { get; }

    public RoleAttributeGroupDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "属性列表")
    {
        this.owner = owner;
        SerializedProperty attrs = property.FindPropertyRelative("attributes");
        if (attrs != null)
        {
            List = new ReorderableList(owner, attrs, true, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    owner.Update();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty attr = attrs.GetArrayElementAtIndex(index);
                    SerializedProperty type = attr.FindPropertyRelative("type");
                    SerializedProperty value = attr.FindPropertyRelative(TypeToPropertyName(type.enumValueIndex));
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width / 2, lineHeight), type, new GUIContent(string.Empty));
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight), value, new GUIContent(string.Empty));
                    if (EditorGUI.EndChangeCheck())
                        owner.ApplyModifiedProperties();
                },

                elementHeightCallback = (index) =>
                {
                    return lineHeightSpace;
                },

                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, listTitle);
                },

                drawNoneElementCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "空列表");
                }
            };
        }
    }

    public void DoLayoutDraw()
    {
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        List?.DoList(rect);
        owner?.ApplyModifiedProperties();
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }

    private string TypeToPropertyName(int type)
    {
        switch ((RoleAttributeType)type)
        {
            case RoleAttributeType.HP:
            case RoleAttributeType.MP:
            case RoleAttributeType.SP:
            case RoleAttributeType.CutATK:
            case RoleAttributeType.PunATK:
            case RoleAttributeType.BluATK:
            case RoleAttributeType.DEF:
                return "intValue";
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpeed:
            case RoleAttributeType.MoveSpeed:
                return "floatValue";
            case RoleAttributeType.TestBool:
                return "boolValue";
            default:
                return "intValue";
        }
    }
}

public class ItemAmountListDrawer
{
    private readonly SerializedObject owner;

    public ReorderableList List { get; }

    public ItemAmountListDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "道具数量列表")
    {
        this.owner = owner;
        List = new ReorderableList(owner, property, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                SerializedProperty itemInfo = property.GetArrayElementAtIndex(index);
                SerializedProperty item = itemInfo.FindPropertyRelative("item");
                SerializedProperty amount = itemInfo.FindPropertyRelative("amount");
                if (item.objectReferenceValue != null)
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), $"[{index}] {(item.objectReferenceValue as ItemBase).name}");
                else
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), $"[{index}] (空)");
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                    item, new GUIContent(string.Empty));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                    amount, new GUIContent("数量"));
                if (amount.intValue < 1) amount.intValue = 1;
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            elementHeightCallback = (int index) =>
            {
                return 2 * lineHeightSpace;
            },

            onRemoveCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
                {
                    property.DeleteArrayElementAtIndex(list.index);
                }
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                SerializedProperty item;
                for (int i = 0; i < property.arraySize; i++)
                {
                    item = property.GetArrayElementAtIndex(i);
                    if (!item.FindPropertyRelative("item").objectReferenceValue || item.FindPropertyRelative("amount").intValue < 1)
                        notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, "数量：" + property.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        List?.DoList(rect);
        owner?.ApplyModifiedProperties();
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class MaterialListDrawer
{
    private readonly SerializedObject owner;

    public ReorderableList List { get; }

    public MaterialListDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "材料列表")
    {
        this.owner = owner;
        List = new ReorderableList(owner, property, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                owner.Update();
                SerializedProperty material = property.GetArrayElementAtIndex(index);
                SerializedProperty materialType = material.FindPropertyRelative("materialType");
                SerializedProperty makingType = material.FindPropertyRelative("makingType");
                SerializedProperty item = material.FindPropertyRelative("item");
                SerializedProperty amount = material.FindPropertyRelative("amount");

                string headLabel = $"[{index}] (空)";
                if (item.objectReferenceValue && (MakingType)makingType.enumValueIndex == MakingType.SingleItem)
                    headLabel = $"[{index}] {(item.objectReferenceValue as ItemBase).name}";
                else if ((MakingType)makingType.enumValueIndex == MakingType.SameType)
                {
                    headLabel = $"[{index}] {ZetanUtility.GetInspectorName((MaterialType)materialType.enumValueIndex)}";
                }
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), headLabel);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight), makingType, new GUIContent(string.Empty));
                if (makingType.enumValueIndex == (int)MakingType.SameType)
                {
                    var typeBef = materialType.enumValueIndex;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), materialType, new GUIContent("所需种类"));
                    if (typeBef != materialType.enumValueIndex)
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (i != index)
                            {
                                SerializedProperty element = property.GetArrayElementAtIndex(i);
                                SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                SerializedProperty eMakingType = element.FindPropertyRelative("makingType");
                                SerializedProperty eItem = element.FindPropertyRelative("item");
                                if (eMakingType.enumValueIndex == (int)MakingType.SingleItem)
                                {
                                    if (eItem.objectReferenceValue is ItemBase ei)
                                        if (ei.MaterialType == (MaterialType)materialType.enumValueIndex)
                                        {
                                            EditorUtility.DisplayDialog("错误", $"与第 {i + 1} 个材料的道具类型冲突", "确定");
                                            materialType.enumValueIndex = typeBef;
                                        }
                                }
                                else
                                {
                                    if ((MaterialType)materialType.enumValueIndex != MaterialType.None && eMaterialType.enumValueIndex == materialType.enumValueIndex)
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料已使用该类型", "确定");
                                        materialType.enumValueIndex = typeBef;
                                    }
                                }
                            }
                        }
                }
                else
                {
                    var itemBef = item.objectReferenceValue;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), item, new GUIContent("所需材料"));
                    if (itemBef != item.objectReferenceValue && item.objectReferenceValue is ItemBase itemNow)
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (i != index)
                            {
                                SerializedProperty element = property.GetArrayElementAtIndex(i);
                                SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                SerializedProperty eMakingType = element.FindPropertyRelative("makingType");
                                if (eMakingType.enumValueIndex == (int)MakingType.SameType)
                                    if (itemNow.MaterialType == (MaterialType)eMaterialType.enumValueIndex)
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料的类型 [{ZetanUtility.GetInspectorName((MaterialType)eMaterialType.enumValueIndex)}] 已包括这个道具", "确定");
                                        item.objectReferenceValue = itemBef;
                                    }
                            }
                        }
                }
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight),
                    amount, new GUIContent("所需数量"));
                if (amount.intValue < 1) amount.intValue = 1;
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            elementHeightCallback = (int index) =>
            {
                return 3 * lineHeightSpace;
            },

            onAddCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                property.InsertArrayElementAtIndex(property.arraySize);
                SerializedProperty material = property.GetArrayElementAtIndex(property.arraySize - 1);
                list.Select(property.arraySize - 1);
                SerializedProperty materialType = material.FindPropertyRelative("materialType");
                materialType.enumValueIndex = (int)MaterialType.None;
                SerializedProperty makingType = material.FindPropertyRelative("makingType");
                makingType.enumValueIndex = (int)MakingType.SingleItem;
                SerializedProperty item = material.FindPropertyRelative("item");
                item.objectReferenceValue = null;
                SerializedProperty amount = material.FindPropertyRelative("amount");
                amount.intValue = 1;
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            onRemoveCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个道具吗？", "确定", "取消"))
                {
                    property.DeleteArrayElementAtIndex(list.index);
                }
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                SerializedProperty material;
                for (int i = 0; i < property.arraySize; i++)
                {
                    material = property.GetArrayElementAtIndex(i);
                    if (material.FindPropertyRelative("makingType").enumValueIndex == (int)MakingType.SingleItem && !material.FindPropertyRelative("item").objectReferenceValue)
                        notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        List?.DoList(rect);
        owner?.ApplyModifiedProperties();
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class DropItemListDrawer
{
    private readonly SerializedObject owner;

    public ReorderableList List { get; }

    public DropItemListDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "产出列表")
    {
        this.owner = owner;
        List = new ReorderableList(owner, property, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                owner.Update();
                SerializedProperty itemInfo = property.GetArrayElementAtIndex(index);
                EditorGUI.BeginChangeCheck();
                SerializedProperty item = itemInfo.FindPropertyRelative("item");
                if (item.objectReferenceValue != null)
                    EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent($"[{index}] {(item.objectReferenceValue as ItemBase).name}"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x + 8f, rect.y, rect.width / 2f, lineHeight), itemInfo, new GUIContent($"[{index}] (空)"));
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
                        minAmount, new GUIContent("最少产出"));
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
                        maxAmount, new GUIContent("最多产出"));
                    if (minAmount.intValue < 1) minAmount.intValue = 1;
                    if (minAmount.intValue > maxAmount.intValue)
                    {
                        minAmount.intValue = maxAmount.intValue + minAmount.intValue;
                        maxAmount.intValue = minAmount.intValue - maxAmount.intValue;
                        minAmount.intValue = minAmount.intValue - maxAmount.intValue;
                    }
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
                    owner.ApplyModifiedProperties();
            },

            elementHeightCallback = (int index) =>
            {
                int lineCount = 1;
                SerializedProperty dropItem = property.GetArrayElementAtIndex(index);
                if (dropItem.isExpanded)
                {
                    lineCount += 3;//数量、百分比、只在
                    if (dropItem.FindPropertyRelative("onlyDropForQuest").boolValue)
                    {
                        if (dropItem.FindPropertyRelative("bindedQuest").objectReferenceValue)
                            lineCount++;//任务标题
                    }
                }
                return lineCount * lineHeightSpace;
            },

            onRemoveCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个掉落道具吗？", "确定", "取消"))
                {
                    property.DeleteArrayElementAtIndex(list.index);
                }
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                SerializedProperty dropItem;
                for (int i = 0; i < property.arraySize; i++)
                {
                    dropItem = property.GetArrayElementAtIndex(i);
                    if (!dropItem.FindPropertyRelative("item").objectReferenceValue)
                        notCmpltCount++;
                }
                EditorGUI.LabelField(rect, listTitle, notCmpltCount > 0 ? "未补全：" + notCmpltCount : string.Empty);
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        List?.DoList(rect);
        owner?.ApplyModifiedProperties();
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        return List.GetHeight();
    }
}

public class ConditionGroupDrawer
{
    private readonly SerializedObject owner;
    private readonly SerializedProperty property;

    private readonly float lineHeightSpace;

    private readonly Dictionary<int, CharacterSelectionDrawer<TalkerInformation>> npcSelectors;

    public ReorderableList List { get; }

    public ConditionGroupDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "条件列表")
    {
        this.owner = owner;
        this.property = property;
        this.lineHeightSpace = lineHeightSpace;
        SerializedProperty conditions = property.FindPropertyRelative("conditions");
        npcSelectors = new Dictionary<int, CharacterSelectionDrawer<TalkerInformation>>();
        List = new ReorderableList(property.serializedObject, conditions, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                owner.Update();
                SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
                SerializedProperty type = condition.FindPropertyRelative("type");
                ConditionType conditionType = (ConditionType)type.enumValueIndex;
                EditorGUI.PropertyField(new Rect(rect.x + 8, rect.y, rect.width * 0.2f, lineHeight), condition, new GUIContent("条件[" + index + "]"), false);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.2f + 8, rect.y, rect.width * 0.8f - 8, lineHeight),
                    type, new GUIContent(string.Empty), true);
                if (condition.isExpanded)
                    switch (conditionType)
                    {
                        case ConditionType.CompleteQuest:
                        case ConditionType.AcceptQuest:
                            SerializedProperty day = condition.FindPropertyRelative("intValue");
                            SerializedProperty relatedQuest = condition.FindPropertyRelative("relatedQuest");
                            day.intValue = EditorGUI.IntSlider(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), "后第几天", day.intValue, 0, 30);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), relatedQuest, new GUIContent("对应任务"));
                            if (relatedQuest.objectReferenceValue == owner.targetObject as Quest) relatedQuest.objectReferenceValue = null;
                            if (relatedQuest.objectReferenceValue)
                            {
                                Quest quest = relatedQuest.objectReferenceValue as Quest;
                                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 3, rect.width, lineHeight), "任务标题", quest.Title);
                            }
                            break;
                        case ConditionType.HasItem:
                            SerializedProperty relatedItem = condition.FindPropertyRelative("relatedItem");
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), relatedItem, new GUIContent("对应道具"));
                            if (relatedItem.objectReferenceValue)
                            {
                                ItemBase item = relatedItem.objectReferenceValue as ItemBase;
                                EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "道具名称", item.name);
                            }
                            break;
                        case ConditionType.Level:
                            SerializedProperty level = condition.FindPropertyRelative("intValue");
                            SerializedProperty compareType = condition.FindPropertyRelative("compareType");
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), level, new GUIContent("对应等级"));
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), compareType, new GUIContent("比较方式"));
                            if (level.intValue < 1) level.intValue = 1;
                            break;
                        case ConditionType.TriggerSet:
                        case ConditionType.TriggerReset:
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                                condition.FindPropertyRelative("triggerName"), new GUIContent("触发器名称"));
                            break;
                        case ConditionType.NPCIntimacy:
                            int lineCount = 1;
                            SerializedProperty npc = condition.FindPropertyRelative("relatedCharInfo");
                            SerializedProperty intimacy = condition.FindPropertyRelative("intValue");
                            compareType = condition.FindPropertyRelative("compareType");
                            if (!npcSelectors.TryGetValue(index, out var selector))
                            {
                                selector = new CharacterSelectionDrawer<TalkerInformation>(npc, "条件对象");
                                npcSelectors.Add(index, selector);
                            }
                            selector.DoDraw(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight));
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), npc, new GUIContent("对象引用"), false);
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), intimacy, new GUIContent("亲密度"));
                            lineCount++;
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), compareType, new GUIContent("比较方式"));
                            lineCount++;
                            break;
                        default: break;
                    }
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            elementHeightCallback = (int index) =>
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
                SerializedProperty type = condition.FindPropertyRelative("type");
                ConditionType conditionType = (ConditionType)type.enumValueIndex;
                if (condition.isExpanded)
                    switch (conditionType)
                    {
                        case ConditionType.CompleteQuest:
                        case ConditionType.AcceptQuest:
                            if (condition.FindPropertyRelative("relatedQuest").objectReferenceValue)
                                return 4 * lineHeightSpace;
                            else return 3 * lineHeightSpace;
                        case ConditionType.HasItem:
                            if (condition.FindPropertyRelative("relatedItem").objectReferenceValue)
                                return 3 * lineHeightSpace;
                            else return 2 * lineHeightSpace;
                        case ConditionType.Level:
                            return 3 * lineHeightSpace;
                        case ConditionType.NPCIntimacy:
                            return 5 * lineHeightSpace;
                        case ConditionType.TriggerSet:
                        case ConditionType.TriggerReset:
                            return 2 * lineHeightSpace;
                        default: return lineHeightSpace;
                    }
                else return lineHeightSpace;
            },

            onRemoveCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个条件吗？", "确定", "取消"))
                {
                    conditions.DeleteArrayElementAtIndex(list.index);
                    npcSelectors.Clear();
                }
                if (EditorGUI.EndChangeCheck())
                    owner.ApplyModifiedProperties();
            },

            drawHeaderCallback = (rect) =>
            {
                int notCmpltCount = 0;
                for (int i = 0; i < conditions.arraySize; i++)
                {
                    SerializedProperty condition = conditions.GetArrayElementAtIndex(i);
                    SerializedProperty type = condition.FindPropertyRelative("type");
                    ConditionType conditionType = (ConditionType)type.enumValueIndex;
                    switch (conditionType)
                    {
                        case ConditionType.CompleteQuest:
                        case ConditionType.AcceptQuest:
                            if (condition.FindPropertyRelative("relatedQuest").objectReferenceValue == null) notCmpltCount++;
                            break;
                        case ConditionType.HasItem:
                            if (condition.FindPropertyRelative("relatedItem").objectReferenceValue == null) notCmpltCount++;
                            break;
                        case ConditionType.Level:
                            if (condition.FindPropertyRelative("intValue").intValue < 1) notCmpltCount++;
                            break;
                        case ConditionType.TriggerSet:
                        case ConditionType.TriggerReset:
                            if (!string.IsNullOrEmpty(condition.FindPropertyRelative("triggerName").stringValue)) notCmpltCount++;
                            break;
                        default: break;
                    }
                }
                EditorGUI.LabelField(rect, listTitle, "数量：" + conditions.arraySize + (notCmpltCount > 0 ? "\t未补全：" + notCmpltCount : string.Empty));
            },

            drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "空列表");
            }
        };
    }

    public void DoLayoutDraw()
    {
        if (List != null && List.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
    }

    public void DoDraw(Rect rect)
    {
        if (List != null && List.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
        owner?.Update();
        List?.DoList(List != null && List.count > 0 ? new Rect(rect.x, rect.y + lineHeightSpace, rect.width, rect.height) : rect);
        owner?.ApplyModifiedProperties();
    }

    public float GetDrawHeight()
    {
        if (List == null) return 0;
        float height = List.GetHeight();
        if (List.count > 0)
            height += lineHeightSpace;
        return height;
    }
}

public class CharacterSelectionDrawer<T> where T : CharacterInformation
{
    private readonly T[] characters;
    private readonly string[] characterNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public CharacterSelectionDrawer(SerializedProperty property, string label = "角色选择", string nameNull = "未选择")
    {
        characters = Resources.LoadAll<T>("Configuration");
        List<string> characterNames = new List<string>() { nameNull };
        foreach (var character in characters)
        {
            if (character is EnemyInformation e && e.Race)
            {
                characterNames.Add($"{e.Race.name}/{e.name}");
            }
            else characterNames.Add(character.name);
        }
        this.property = property;
        this.label = label;
        this.characterNames = characterNames.ToArray();
    }

    public void DoLayoutDraw()
    {
        int index = Array.FindIndex(characters, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUILayout.Popup(label, index, characterNames);
        if (index < 1 || index > characters.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = characters[index - 1];
    }

    public void DoDraw(Rect rect)
    {
        int index = Array.FindIndex(characters, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(rect, label, index, characterNames);
        if (index < 1 || index > characters.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = characters[index - 1];
    }
}

public class ItemSelectionDrawer<T> where T : ItemBase
{
    private readonly T[] items;
    private readonly string[] itemNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public ItemSelectionDrawer(SerializedProperty property, string label = "道具", string nameNull = "未选择")
    {
        items = Resources.LoadAll<T>("Configuration");
        List<string> itemNames = new List<string>() { nameNull };
        foreach (var item in items)
        {
            itemNames.Add($"{ZetanUtility.GetInspectorName(item.ItemType)}/{item.name}");
        }
        this.property = property;
        this.label = label;
        this.itemNames = itemNames.ToArray();
    }

    public void DoLayoutDraw()
    {
        int index = Array.FindIndex(items, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUILayout.Popup(label, index, itemNames);
        if (index < 1 || index > items.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = items[index - 1];
    }

    public void DoDraw(Rect rect)
    {
        int index = Array.FindIndex(items, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(rect, label, index, itemNames);
        if (index < 1 || index > items.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = items[index - 1];
    }
}

public class SceneSelectionDrawer
{
    private readonly string[] sceneNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public SceneSelectionDrawer(SerializedProperty property, string label = "场景", string nameNull = "未选择")
    {
        List<string> sceneNames = new List<string>() { nameNull };
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var find = (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
            if (find) sceneNames.Add(find.name);
        }
        this.sceneNames = sceneNames.ToArray();
        this.property = property;
        this.label = label;
    }

    public void DoLayoutDraw()
    {
        int index = Array.FindIndex(sceneNames, x => x == property.stringValue);
        index = index < 0 ? 0 : index;
        index = EditorGUILayout.Popup(label, index, sceneNames);
        if (index < 1 || index > sceneNames.Length) property.stringValue = string.Empty;
        else property.stringValue = sceneNames[index];
    }

    public void DoDraw(Rect rect)
    {
        int index = Array.FindIndex(sceneNames, x => x == property.stringValue);
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(rect, label, index, sceneNames);
        if (index < 1 || index > sceneNames.Length) property.stringValue = string.Empty;
        else property.stringValue = sceneNames[index];
    }

}

public class ScriptableObjectSelectionDrawer<T> where T : ScriptableObject
{
    private readonly T[] objects;
    private readonly string[] objectNames;

    private readonly SerializedProperty property;
    private readonly string label;

    public ScriptableObjectSelectionDrawer(SerializedProperty property, string fieldAsName, string path, string label = "资源", string nameNull = "未选择")
    {
        objects = Resources.LoadAll<T>(string.IsNullOrEmpty(path) ? string.Empty : path);
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null) objectNames.Add(field.GetValue(obj).ToString());
                else objectNames.Add(obj.name);
            }
            else objectNames.Add(obj.name);
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();
    }
    public ScriptableObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, string> groupPicker, string path, string label = "资源", string nameNull = "未选择")
    {
        objects = Resources.LoadAll<T>(string.IsNullOrEmpty(path) ? string.Empty : path);
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null) objectNames.Add(GetGroupedName(obj, field.GetValue(obj).ToString()));
                else objectNames.Add(GetGroupedName(obj, obj.name));
            }
            else objectNames.Add(GetGroupedName(obj, obj.name));
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();

        string GetGroupedName(T obj, string name)
        {
            if (groupPicker == null) return name;
            string group = groupPicker(obj);
            if (string.IsNullOrEmpty(group)) return name;
            else return $"{group}/{name}";
        }
    }

    public ScriptableObjectSelectionDrawer(SerializedProperty property, Func<T, bool> filter, string fieldAsName, string path, string label = "资源", string nameNull = "未选择")
    {
        objects = Resources.LoadAll<T>(string.IsNullOrEmpty(path) ? string.Empty : path);
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (filter == null || filter != null && filter.Invoke(obj))
            {
                if (!string.IsNullOrEmpty(fieldAsName))
                {
                    var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (field != null) objectNames.Add(field.GetValue(obj).ToString());
                    else objectNames.Add(obj.name);
                }
                else objectNames.Add(obj.name);
            }
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();
    }
    public ScriptableObjectSelectionDrawer(SerializedProperty property, Func<T, bool> filter, string fieldAsName, Func<T, string> groupPicker, string path, string label = "资源", string nameNull = "未选择")
    {
        objects = Resources.LoadAll<T>(string.IsNullOrEmpty(path) ? string.Empty : path);
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (filter == null || filter != null && filter.Invoke(obj))
            {

                if (!string.IsNullOrEmpty(fieldAsName))
                {
                    var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (field != null) objectNames.Add(GetGroupedName(obj, field.GetValue(obj).ToString()));
                    else objectNames.Add(GetGroupedName(obj, obj.name));
                }
                else objectNames.Add(GetGroupedName(obj, obj.name));
            }
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();

        string GetGroupedName(T obj, string name)
        {
            if (groupPicker == null) return name;
            string group = groupPicker(obj);
            if (string.IsNullOrEmpty(group)) return name;
            else return $"{group}/{name}";
        }
    }

    public ScriptableObjectSelectionDrawer(SerializedProperty property, string fieldAsName, T[] resources, string label = "资源", string nameNull = "未选择")
    {
        objects = resources;
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null) objectNames.Add(field.GetValue(obj).ToString());
                else objectNames.Add(obj.name);
            }
            else objectNames.Add(obj.name);
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();
    }
    public ScriptableObjectSelectionDrawer(SerializedProperty property, string fieldAsName, Func<T, string> groupPicker, T[] resources, string label = "资源", string nameNull = "未选择")
    {
        objects = resources;
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null) objectNames.Add(GetGroupedName(obj, field.GetValue(obj).ToString()));
                else objectNames.Add(GetGroupedName(obj, obj.name));
            }
            else objectNames.Add(GetGroupedName(obj, obj.name));
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();

        string GetGroupedName(T obj, string name)
        {
            if (groupPicker == null) return name;
            string group = groupPicker(obj);
            if (string.IsNullOrEmpty(group)) return name;
            else return $"{group}/{name}";
        }
    }

    public ScriptableObjectSelectionDrawer(SerializedProperty property, Func<T, bool> filter, string fieldAsName, T[] resources, string label = "资源", string nameNull = "未选择")
    {
        objects = resources;
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (filter == null || filter != null && filter.Invoke(obj))
            {
                if (!string.IsNullOrEmpty(fieldAsName))
                {
                    var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (field != null) objectNames.Add(field.GetValue(obj).ToString());
                    else objectNames.Add(obj.name);
                }
                else objectNames.Add(obj.name);
            }
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();
    }
    public ScriptableObjectSelectionDrawer(SerializedProperty property, Func<T, bool> filter, string fieldAsName, Func<T, string> groupPicker, T[] resources, string label = "资源", string nameNull = "未选择")
    {
        objects = resources;
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (filter == null || filter != null && filter.Invoke(obj))
            {

                if (!string.IsNullOrEmpty(fieldAsName))
                {
                    var field = obj.GetType().GetField(fieldAsName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (field != null) objectNames.Add(GetGroupedName(obj, field.GetValue(obj).ToString()));
                    else objectNames.Add(GetGroupedName(obj, obj.name));
                }
                else objectNames.Add(GetGroupedName(obj, obj.name));
            }
        }
        this.property = property;
        this.label = label;
        this.objectNames = objectNames.ToArray();

        string GetGroupedName(T obj, string name)
        {
            if (groupPicker == null) return name;
            string group = groupPicker(obj);
            if (string.IsNullOrEmpty(group)) return name;
            else return $"{group}/{name}";
        }
    }

    public void DoLayoutDraw()
    {
        int index = Array.FindIndex(objects, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUILayout.Popup(label, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
    }

    public void DoDraw(Rect rect)
    {
        int index = Array.FindIndex(objects, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(rect, label, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
    }
}