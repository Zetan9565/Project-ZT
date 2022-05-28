using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(KillObjective))]
public sealed class KillObjectiveDrawer : ObjectiveDrawer
{
    protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
    {
        base.DrawAdditionalProperty(objective, rect, ref lineCount);
        SerializedProperty killType = objective.FindPropertyRelative("killType");
        SerializedProperty enemy = objective.FindPropertyRelative("enemy");
        SerializedProperty race = objective.FindPropertyRelative("race");
        SerializedProperty group = objective.FindPropertyRelative("group");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), killType, new GUIContent("目标类型"));
        lineCount++;
        switch (killType.enumValueIndex)
        {
            case (int)KillObjectiveType.Specific:
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), enemy, new GUIContent("目标敌人"));
                lineCount++;
                break;
            case (int)KillObjectiveType.Race:
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), race, new GUIContent("目标种族"));
                lineCount++;
                break;
            case (int)KillObjectiveType.Group:
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), group, new GUIContent("目标组合"));
                lineCount++;
                break;
            default: break;
        }
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
            if (objective.FindPropertyRelative("showMapIcon").boolValue)
                lineCount++;//辅助位置
            if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
            lineCount += 1;//目标类型
            switch (objective.FindPropertyRelative("killType").enumValueIndex)
            {
                case (int)KillObjectiveType.Specific:
                case (int)KillObjectiveType.Race:
                case (int)KillObjectiveType.Group:
                    lineCount++;
                    break;
                default: break;
            }
        }
        return lineCount * lineHeightSpace;
    }
    protected override string GetTypePrefix()
    {
        return "[杀]";
    }
}