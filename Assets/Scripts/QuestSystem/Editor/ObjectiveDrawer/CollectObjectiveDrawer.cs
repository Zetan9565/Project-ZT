using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CollectObjective))]
public sealed class CollectObjectiveDrawer : ObjectiveDrawer
{
    protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
    {
        base.DrawAdditionalProperty(objective, rect, ref lineCount);
        SerializedProperty itemToCollect = objective.FindPropertyRelative("itemToCollect");
        SerializedProperty checkBagAtStart = objective.FindPropertyRelative("checkBagAtStart");
        SerializedProperty loseItemAtSbmt = objective.FindPropertyRelative("loseItemAtSbmt");
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), itemToCollect, new GUIContent("目标道具"));
        lineCount++;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
            checkBagAtStart, new GUIContent("开始进行时检查数量"));
        EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2, rect.y + lineHeightSpace * lineCount, rect.width / 2, lineHeight),
            loseItemAtSbmt, new GUIContent("提交时失去相应道具"));
        lineCount++;
    }
    protected override string GetTypePrefix()
    {
        return "[集]";
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
            lineCount += 2;//目标道具、接取时检查、提交时失去
        }
        return lineCount * lineHeightSpace;
    }
}