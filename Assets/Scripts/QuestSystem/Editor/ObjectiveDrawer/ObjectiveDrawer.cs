using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Objective), true)]
public class ObjectiveDrawer : PropertyDrawer
{
    public Quest[] questCache;
    public TalkerInformation[] talkerCache;
    public ItemBase[] itemCache;
    public Dialogue[] dialogueCache;
    protected float lineHeight;
    protected float lineHeightSpace;

    public ObjectiveDrawer()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;
        questCache = Resources.LoadAll<Quest>("Configuration");
        talkerCache = Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.Enable).ToArray();
        itemCache = Resources.LoadAll<ItemBase>("Configuration");
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.serializedObject.targetObject is Quest && typeof(IList<Objective>).IsAssignableFrom(fieldInfo.FieldType))
            DrawObjectiveItem(property, position);
        else
        {
            Debug.LogWarning("意外的Objective，它不处于任何任务的目标列表中");
            EditorGUI.PropertyField(position, property, label);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.serializedObject.targetObject is Quest && typeof(IList<Objective>).IsAssignableFrom(fieldInfo.FieldType))
            return GetObejctiveItemDrawHeight(property);
        else return EditorGUI.GetPropertyHeight(property, label);
    }

    public void DrawObjectiveItem(SerializedProperty objective, Rect position)
    {
        int.TryParse(objective.propertyPath.Split('[', ']')[^2], out int index);
        SerializedProperty cmpltObjctvInOrder = objective.serializedObject.FindProperty("cmpltObjctvInOrder");
        SerializedProperty objectives = objective.serializedObject.FindProperty("objectives");
        SerializedProperty display = objective.FindPropertyRelative("display");
        SerializedProperty displayName = objective.FindPropertyRelative("displayName");
        SerializedProperty amount = objective.FindPropertyRelative("amount");
        SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
        SerializedProperty priority = objective.FindPropertyRelative("priority");
        string typePrefix = GetTypePrefix();
        if (display.boolValue)
        {
            if (!string.IsNullOrEmpty(displayName.stringValue))
                EditorGUI.PropertyField(new Rect(position.x + 8, position.y, position.width * 0.8f, lineHeight), objective, new GUIContent(typePrefix + displayName.stringValue));
            else EditorGUI.PropertyField(new Rect(position.x + 8, position.y, position.width * 0.8f, lineHeight), objective, new GUIContent(typePrefix + "(空标题)"));
        }
        else EditorGUI.PropertyField(new Rect(position.x + 8, position.y, position.width * 0.8f, lineHeight), objective, new GUIContent(typePrefix + "(被隐藏的目标)"));
        EditorGUI.LabelField(new Rect(position.x + position.width - 20, position.y, 0, lineHeight), $"优先级：{priority.intValue}", ZetanUtility.Editor.Style.middleRight);
        if (cmpltObjctvInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(position.x + position.width - 15, position.y, 10, lineHeight), display.boolValue);
        int lineCount = 1;
        if (objective.isExpanded)
        {
            if (display.boolValue || !cmpltObjctvInOrder.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), displayName, new GUIContent("标题"));
                lineCount++;
            }
            EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), amount, new GUIContent("目标数量"));
            if (amount.intValue < 1) amount.intValue = 1;
            lineCount++;
            if (cmpltObjctvInOrder.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), inOrder, new GUIContent("按顺序"));
                lineCount++;
            }
            int left = 1;
            if (index > 1 && objectives.GetArrayElementAtIndex(index - 1) is SerializedProperty prev)
                left = prev.FindPropertyRelative("priority").intValue;
            priority.intValue = EditorGUI.IntSlider(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), "优先级", priority.intValue, left, index + 1);
            lineCount++;
            DrawAdditionalProperty(objective, position, ref lineCount);
        }
    }
    protected virtual void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
    {
        SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
        SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
        SerializedProperty auxiliaryPos = objective.FindPropertyRelative("auxiliaryPos");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
        lineCount++;
        if (showMapIcon.boolValue)
        {
            EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * (lineCount - 1), rect.width * 0.5f, lineHeight),
               canNavigate, new GUIContent("可导航"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
               auxiliaryPos, new GUIContent("辅助位置", "用于显示地图图标、导航等"));
            lineCount++;
        }
    }
    protected virtual string GetTypePrefix() => string.Empty;
    public virtual float GetObejctiveItemDrawHeight(SerializedProperty objective)
    {
        Quest quest = objective.serializedObject.targetObject as Quest;
        int lineCount = 1;
        if (objective.isExpanded)
        {
            lineCount++;//目标数量
            if (quest.CmpltObjctvInOrder)
                lineCount++;// 按顺序
            lineCount += 1;//执行顺序
            lineCount += 1;//可导航
            if (objective.FindPropertyRelative("showMapIcon").boolValue)
                lineCount++;//辅助位置
            if (objective.FindPropertyRelative("display").boolValue || !quest.CmpltObjctvInOrder) lineCount++;//标题
        }
        return lineCount * lineHeightSpace;
    }
}