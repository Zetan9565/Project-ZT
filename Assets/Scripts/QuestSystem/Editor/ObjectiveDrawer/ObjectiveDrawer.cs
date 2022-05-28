using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Item;
using ZetanStudio.Extension.Editor;

[CustomPropertyDrawer(typeof(Objective), true)]
public class ObjectiveDrawer : PropertyDrawer
{
    public Quest[] questCache;
    public TalkerInformation[] talkerCache;
    public Item[] itemCache;
    public Dialogue[] dialogueCache;
    protected float lineHeight;
    protected float lineHeightSpace;

    public ObjectiveDrawer()
    {
        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 2;
        questCache = Resources.LoadAll<Quest>("Configuration");
        talkerCache = Resources.LoadAll<TalkerInformation>("Configuration").Where(x => x.Enable).ToArray();
        itemCache = Resources.LoadAll<Item>("Configuration");
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
        SerializedProperty questInOrder = objective.serializedObject.FindProperty("inOrder");
        SerializedProperty objectives = objective.serializedObject.FindProperty("objectives");
        SerializedProperty display = objective.FindPropertyRelative("display");
        SerializedProperty displayName = objective.FindPropertyRelative("displayName");
        SerializedProperty amount = objective.FindPropertyRelative("amount");
        SerializedProperty showAmount = objective.FindPropertyRelative("showAmount");
        SerializedProperty inOrder = objective.FindPropertyRelative("inOrder");
        SerializedProperty priority = objective.FindPropertyRelative("priority");
        int index = objective.GetArrayIndex();
        int left = 0;
        if (index > 0 && objectives.GetArrayElementAtIndex(index - 1) is SerializedProperty prev)
            left = prev.FindPropertyRelative("priority").intValue;
        priority.intValue = Mathf.Clamp(priority.intValue, left, index);
        string typePrefix = GetTypePrefix();
        string title = typePrefix;
        if (display.boolValue)
            if (string.IsNullOrEmpty(displayName.stringValue)) title += "(空标题)";
            else title += displayName.stringValue;
        else title += "(被隐藏的目标)";
        EditorGUI.BeginProperty(new Rect(position.x + 8, position.y, position.width * 0.8f, lineHeight), GUIContent.none, objective);
        objective.isExpanded = EditorGUI.Foldout(new Rect(position.x + 8, position.y, position.width * 0.8f, lineHeight), objective.isExpanded, title, true);
        EditorGUI.EndProperty();
        EditorGUI.LabelField(new Rect(position.x + position.width - 20, position.y, 0, lineHeight), $"优先级：{priority.intValue}", ZetanUtility.Editor.Style.middleRight);
        if (questInOrder.boolValue) display.boolValue = EditorGUI.Toggle(new Rect(position.x + position.width - 15, position.y, 10, lineHeight), display.boolValue);
        int lineCount = 1;
        if (objective.isExpanded)
        {
            if (display.boolValue || !questInOrder.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), displayName, new GUIContent("标题"));
                lineCount++;
            }
            EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width - 17, lineHeight), amount, new GUIContent("目标数量"));
            showAmount.boolValue = EditorGUI.Toggle(new Rect(position.x + position.width - 15, position.y + lineHeightSpace * lineCount, 10, lineHeight), showAmount.boolValue);
            if (amount.intValue < 1) amount.intValue = 1;
            lineCount++;
            if (questInOrder.boolValue)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), inOrder, new GUIContent("按顺序"));
                lineCount++;
            }
            priority.intValue = EditorGUI.IntSlider(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), "优先级", priority.intValue, left, index);
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
            if (quest.InOrder)
                lineCount++;// 按顺序
            lineCount += 1;//执行顺序
            lineCount += 1;//可导航
            if (objective.FindPropertyRelative("showMapIcon").boolValue)
                lineCount++;//辅助位置
            if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
        }
        return lineCount * lineHeightSpace;
    }
}