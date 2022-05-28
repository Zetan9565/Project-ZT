using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TriggerObjective))]
public sealed class TriggerObjectiveDrawer : ObjectiveDrawer
{
    protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
    {
        SerializedProperty showAmount = objective.FindPropertyRelative("showAmount");
        SerializedProperty triggerName = objective.FindPropertyRelative("triggerName");
        SerializedProperty stateToCheck = objective.FindPropertyRelative("stateToCheck");
        SerializedProperty checkStateAtAcpt = objective.FindPropertyRelative("checkStateAtAcpt");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
            showAmount, new GUIContent("显示数量进度"));
        lineCount++;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight),
            triggerName, new GUIContent("触发器名称"));
        lineCount++;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
            stateToCheck, new GUIContent("触发器置位状态"));
        EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
            checkStateAtAcpt, new GUIContent("接取时检查状态"));
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
            if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
            lineCount += 3;//触发器、状态、检查
        }
        return lineCount * lineHeightSpace;
    }

    protected override string GetTypePrefix()
    {
        return "[触]";
    }
}