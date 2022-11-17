using System;
using UnityEditor;
using UnityEngine;
using ZetanStudio.DialogueSystem;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.QuestSystem.Editor
{
    [CustomPropertyDrawer(typeof(TalkObjective))]
    public sealed class TalkObjectiveDrawer : ObjectiveDrawer
    {
        protected override void DrawAdditionalProperty(SerializedProperty objective, Rect rect, ref int lineCount)
        {
            SerializedProperty canNavigate = objective.FindPropertyRelative("canNavigate");
            SerializedProperty showMapIcon = objective.FindPropertyRelative("showMapIcon");
            SerializedProperty auxiliaryPos = objective.FindPropertyRelative("auxiliaryPos");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width * 0.5f, lineHeight), showMapIcon, new GUIContent("显示地图图标"));
            lineCount++;
            if (showMapIcon.boolValue)
                EditorGUI.PropertyField(new Rect(rect.x + rect.width * 0.5f, rect.y + lineHeightSpace * (lineCount - 1), rect.width * 0.5f, lineHeight),
                    canNavigate, new GUIContent("可导航"));

            Quest quest = objective.serializedObject.targetObject as Quest;
            SerializedProperty _NPCToTalk = objective.FindPropertyRelative("_NPCToTalk");
            SerializedProperty dialogue = objective.FindAutoProperty("Dialogue");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), _NPCToTalk, new GUIContent("与此NPC交谈"));
            lineCount++;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight), dialogue, new GUIContent("交谈时的对话"));
            lineCount++;
            if (dialogue.objectReferenceValue is Dialogue dialog)
            {
                Quest find = Array.Find(questCache, x => x != quest && x.Objectives.Exists(y => y is TalkObjective to && to.Dialogue == dialog));
                if (find)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y + lineHeightSpace * lineCount, rect.width, lineHeight * 2.4f),
                        "已有任务目标使用该对话，游戏中可能会产生逻辑错误。\n任务名称：" + find.Title, MessageType.Warning);
                    lineCount += 2;
                }
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
                if (objective.FindPropertyRelative("display").boolValue || !quest.InOrder) lineCount++;//标题
                lineCount++;//目标NPC
                lineCount++; //交谈时对话
                SerializedProperty dialogue = objective.FindAutoProperty("Dialogue");
                if (dialogue.objectReferenceValue is Dialogue dialog)
                {
                    if (Array.Find(questCache, x => x != quest && x.Objectives.Exists(y => y is TalkObjective to && to.Dialogue == dialog)))
                        lineCount += 2;//逻辑错误
                }
            }
            return lineCount * lineHeightSpace;
        }
        protected override string GetTypePrefix()
        {
            return "[谈]";
        }
    }
}