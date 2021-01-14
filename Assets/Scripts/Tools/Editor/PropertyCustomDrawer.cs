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
            case RoleAttributeType.ATK:
            case RoleAttributeType.DEF:
                return "intValue";
            case RoleAttributeType.Hit:
            case RoleAttributeType.Crit:
            case RoleAttributeType.ATKSpd:
            case RoleAttributeType.MoveSpd:
                return "floatValue";
            case RoleAttributeType.TestBool:
                return "boolValue";
            default:
                return "intValue";
        }
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

                if (item.objectReferenceValue && (MakingType)makingType.enumValueIndex == MakingType.SingleItem)
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), (item.objectReferenceValue as ItemBase).name);
                else if ((MakingType)makingType.enumValueIndex == MakingType.SameType)
                {
                    switch ((MaterialType)materialType.enumValueIndex)
                    {
                        case MaterialType.Cloth: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "布料"); break;
                        case MaterialType.Fruit: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "水果"); break;
                        case MaterialType.Fur: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "皮毛"); break;
                        case MaterialType.Meat: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "肉类"); break;
                        case MaterialType.Metal: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "金属"); break;
                        case MaterialType.Ore: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "矿石"); break;
                        case MaterialType.Plant: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "植物"); break;
                        default: EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "未定义"); break;
                    }
                }
                else
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight), makingType, new GUIContent(string.Empty));
                if (makingType.enumValueIndex == (int)MakingType.SameType)
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), materialType, new GUIContent("所需种类"));
                else
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), item, new GUIContent("所需材料"));
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
                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty material = property.GetArrayElementAtIndex(i);
                    if (!material.FindPropertyRelative("item").objectReferenceValue)
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

    public ReorderableList List { get; }

    public ConditionGroupDrawer(SerializedObject owner, SerializedProperty property, float lineHeight, float lineHeightSpace, string listTitle = "条件列表")
    {
        this.owner = owner;
        this.property = property;
        this.lineHeightSpace = lineHeightSpace;
        SerializedProperty conditions = property.FindPropertyRelative("conditions");
        List = new ReorderableList(property.serializedObject, conditions, true, true, true, true)
        {
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                owner.Update();
                SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
                SerializedProperty type = condition.FindPropertyRelative("type");
                ConditionType conditionType = (ConditionType)type.enumValueIndex;
                if (condition != null)
                {
                    switch (conditionType)
                    {
                        case ConditionType.CompleteQuest:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "完成任务");
                            break;
                        case ConditionType.AcceptQuest:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "接取任务");
                            break;
                        case ConditionType.HasItem:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "拥有道具");
                            break;
                        case ConditionType.LevelEquals:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级等于");
                            break;
                        case ConditionType.LevelLargeThen:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级大于");
                            break;
                        case ConditionType.LevelLessThen:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "等级小于");
                            break;
                        case ConditionType.TriggerSet:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器开启");
                            break;
                        case ConditionType.TriggerReset:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "触发器关闭");
                            break;
                        default:
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "[" + index + "]" + "未定义条件");
                            break;
                    }
                }
                else EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), "(空)");
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, lineHeight),
                    type, new GUIContent(string.Empty), true);

                switch (conditionType)
                {
                    case ConditionType.CompleteQuest:
                    case ConditionType.AcceptQuest:
                        SerializedProperty relatedQuest = condition.FindPropertyRelative("relatedQuest");
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), relatedQuest, new GUIContent("需完成的任务"));
                        if (relatedQuest.objectReferenceValue == owner.targetObject as Quest) relatedQuest.objectReferenceValue = null;
                        if (relatedQuest.objectReferenceValue)
                        {
                            Quest quest = relatedQuest.objectReferenceValue as Quest;
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "任务标题", quest.Title);
                        }
                        break;
                    case ConditionType.HasItem:
                        SerializedProperty relatedItem = condition.FindPropertyRelative("relatedItem");
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 1, rect.width, lineHeight), relatedItem, new GUIContent("需拥有的道具"));
                        if (relatedItem.objectReferenceValue)
                        {
                            ItemBase item = relatedItem.objectReferenceValue as ItemBase;
                            EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, lineHeight), "道具名称", item.name);
                        }
                        break;
                    case ConditionType.LevelEquals:
                    case ConditionType.LevelLargeThen:
                    case ConditionType.LevelLessThen:
                        SerializedProperty level = condition.FindPropertyRelative("level");
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight), level, new GUIContent("限制的等级"));
                        if (level.intValue < 1) level.intValue = 1;
                        break;
                    case ConditionType.TriggerSet:
                    case ConditionType.TriggerReset:
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace, rect.width, lineHeight),
                            condition.FindPropertyRelative("triggerName"), new GUIContent("触发器名称"));
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
                switch (conditionType)
                {
                    case ConditionType.CompleteQuest:
                    case ConditionType.AcceptQuest:
                        if (condition.FindPropertyRelative("relatedQuest").objectReferenceValue)
                            return 3 * lineHeightSpace;
                        else return 2 * lineHeightSpace;
                    case ConditionType.HasItem:
                        if (condition.FindPropertyRelative("relatedItem").objectReferenceValue)
                            return 3 * lineHeightSpace;
                        else return 2 * lineHeightSpace;
                    case ConditionType.LevelEquals:
                    case ConditionType.LevelLargeThen:
                    case ConditionType.LevelLessThen:
                    case ConditionType.TriggerSet:
                    case ConditionType.TriggerReset:
                        return 2 * lineHeightSpace;
                    default: return lineHeightSpace;
                }
            },

            onRemoveCallback = (list) =>
            {
                owner.Update();
                EditorGUI.BeginChangeCheck();
                if (EditorUtility.DisplayDialog("删除", "确定删除这个条件吗？", "确定", "取消"))
                {
                    conditions.DeleteArrayElementAtIndex(list.index);
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
                        case ConditionType.LevelEquals:
                        case ConditionType.LevelLargeThen:
                        case ConditionType.LevelLessThen:
                            if (condition.FindPropertyRelative("level").intValue < 1) notCmpltCount++;
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
        owner?.Update();
        List?.DoLayoutList();
        owner?.ApplyModifiedProperties();
        if (List != null && List.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
    }

    public void DoDraw(Rect rect)
    {
        owner?.Update();
        List?.DoList(rect);
        owner?.ApplyModifiedProperties();
        if (List != null && List.count > 0)
        {
            owner?.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + List.GetHeight(), rect.width, lineHeightSpace), property.FindPropertyRelative("relational"), new GUIContent("(?)条件关系表达式"));
            if (EditorGUI.EndChangeCheck())
                owner?.ApplyModifiedProperties();
        }
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