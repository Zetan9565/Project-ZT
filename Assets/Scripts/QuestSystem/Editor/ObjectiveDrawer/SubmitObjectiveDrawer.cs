using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubmitObjective))]
public sealed class SubmitObjectiveDrawer : ObjectiveDrawer
{
    protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
    {
        SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
        SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
        lineCount++;
        if (showMapIcon.boolValue)
        {
            EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * (lineCount - 1), rect.width * 0.5f, lineHeight),
               canNavigate, new GUIContent("可导航"));
        }
        SerializedProperty _NPCToSubmit = objective.FindPropertyRelative("_NPCToSubmit");
        SerializedProperty itemToSubmit = objective.FindPropertyRelative("itemToSubmit");
        SerializedProperty wordsWhenSubmit = objective.FindPropertyRelative("wordsWhenSubmit");
        SerializedProperty talkerType = objective.FindPropertyRelative("talkerType");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), _NPCToSubmit, new GUIContent("提交给此NPC"));
        lineCount++;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), itemToSubmit, new GUIContent("需提交的道具"));
        lineCount++;
        EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight), new GUIContent("提交时的说的话"));
        talkerType.enumValueIndex = EditorGUI.Popup(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
            talkerType.enumValueIndex, new string[] { "NPC说", "玩家说" });
        lineCount++;
        wordsWhenSubmit.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
            wordsWhenSubmit.stringValue);
        lineCount++;
    }
    public override float GetObejctiveItemDrawHeight(SerializedProperty objective)
    {
        float lineHeightSpace = EditorGUIUtility.singleLineHeight + 2;
        Quest quest = objective.serializedObject.targetObject as Quest;
        int lineCount = 1;
        if (objective.isExpanded)
        {
            lineCount++;//目标数量
            if (quest.InOrder)
                lineCount++;// 按顺序
            lineCount += 1;//执行顺序
            lineCount += 1;//可导航
            if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
            lineCount += 4;//NPC、目标道具、提交对话、对话人
        }
        return lineCount * lineHeightSpace;
    }

    protected override string GetTypePrefix()
    {
        return "[给]";
    }
}