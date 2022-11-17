using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZetanStudio.ItemSystem.Editor;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.QuestSystem.Editor
{
    [CustomPropertyDrawer(typeof(MoveObjective))]
    public sealed class MoveObjectiveDrawer : ObjectiveDrawer
    {
        protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
        {
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty auxiliaryPos = objective.FindPropertyRelative("auxiliaryPos");
            SerializedProperty itemToUseHere = objective.FindPropertyRelative("itemToUseHere");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            lineCount++;
            if (showMapIcon.boolValue)
            {
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * (lineCount - 1), rect.width * 0.5f, lineHeight),
                   canNavigate, new GUIContent("可导航"));
            }
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), auxiliaryPos, new GUIContent("检查点"));
            lineCount++;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), itemToUseHere, new GUIContent("需在此处使用的道具"));
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
                lineCount++;//辅助位置
                if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
                lineCount++;//道具
            }
            return lineCount * lineHeightSpace;
        }
        protected override string GetTypePrefix()
        {
            return "[移]";
        }
    }
}