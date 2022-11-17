using UnityEditor;
using UnityEngine;

namespace ZetanStudio.QuestSystem.Editor
{
    using UI;

    [CustomEditor(typeof(QuestWindow))]
    public class QuestWindowInspector : WindowInspector
    {
        SerializedProperty questList;
        SerializedProperty tabBar;
        SerializedProperty title;
        SerializedProperty giver;
        SerializedProperty descriptionWindow;
        SerializedProperty descriptionText;
        SerializedProperty objectiveText;
        SerializedProperty abandonButton;
        SerializedProperty traceButton;
        SerializedProperty traceBtnText;
        SerializedProperty desCloseButton;
        SerializedProperty rewardList;
        SerializedProperty maxRewardCount;

        protected override void EnableOther()
        {
            questList = serializedObject.FindProperty("questList");
            tabBar = serializedObject.FindProperty("tabBar");
            descriptionWindow = serializedObject.FindProperty("descriptionWindow");
            title = serializedObject.FindProperty("title");
            giver = serializedObject.FindProperty("giver");
            descriptionText = serializedObject.FindProperty("descriptionText");
            objectiveText = serializedObject.FindProperty("objectiveText");
            abandonButton = serializedObject.FindProperty("abandonButton");
            traceButton = serializedObject.FindProperty("traceButton");
            traceBtnText = serializedObject.FindProperty("traceBtnText");
            desCloseButton = serializedObject.FindProperty("desCloseButton");
            rewardList = serializedObject.FindProperty("rewardList");
            maxRewardCount = serializedObject.FindProperty("maxRewardCount");
        }

        protected override void InspectOther()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("任务窗口相关", Utility.Editor.Style.bold);
            EditorGUILayout.PropertyField(content, new GUIContent("任务窗口"));
            EditorGUILayout.PropertyField(questList, new GUIContent("任务列表"));
            EditorGUILayout.PropertyField(tabBar, new GUIContent("页面切换栏"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("任务详情相关", Utility.Editor.Style.bold);
            EditorGUILayout.PropertyField(descriptionWindow, new GUIContent("任务详情窗口"));
            EditorGUILayout.PropertyField(title, new GUIContent("任务标题文字"));
            EditorGUILayout.PropertyField(giver, new GUIContent("任务委托人文字"));
            EditorGUILayout.PropertyField(descriptionText, new GUIContent("任务描述文字"));
            EditorGUILayout.PropertyField(objectiveText, new GUIContent("任务目标文字"));
            EditorGUILayout.PropertyField(abandonButton, new GUIContent("放弃按钮"));
            EditorGUILayout.PropertyField(traceButton, new GUIContent("追踪按钮"));
            EditorGUILayout.PropertyField(traceBtnText, new GUIContent("追踪按钮文字"));
            EditorGUILayout.PropertyField(desCloseButton, new GUIContent("详情关闭按钮"));
            EditorGUILayout.PropertyField(rewardList, new GUIContent("道具奖励列表"));
            EditorGUILayout.PropertyField(maxRewardCount, new GUIContent("奖励格子数量"));
            EditorGUILayout.EndVertical();
        }
    }
}