using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestUI))]
public class QuestUIInspector : Editor
{
    //QuestUI UI;

    SerializedProperty questsWindow;
    SerializedProperty closeWindow;
    SerializedProperty questPrefab;
    SerializedProperty questGroupPrefab;
    SerializedProperty questList;
    SerializedProperty questListParent;
    SerializedProperty questListToggle;
    SerializedProperty cmpltQuestList;
    SerializedProperty cmpltQuestListParent;
    SerializedProperty cmpltQuestListToggle;
    SerializedProperty descriptionWindow;
    SerializedProperty descriptionText;
    SerializedProperty abandonButton;
    SerializedProperty traceButton;
    SerializedProperty closeDescription;
    SerializedProperty moneyText;
    SerializedProperty EXPText;
    SerializedProperty rewardCellPrefab;
    SerializedProperty rewardCellsParent;
    SerializedProperty questBoard;
    SerializedProperty boardQuestPrefab;
    SerializedProperty questBoardArea;

    private void OnEnable()
    {
        //UI = target as QuestUI;

        questsWindow = serializedObject.FindProperty("questsWindow");
        closeWindow = serializedObject.FindProperty("closeWindow");
        questPrefab = serializedObject.FindProperty("questPrefab");
        questGroupPrefab = serializedObject.FindProperty("questGroupPrefab");
        questList = serializedObject.FindProperty("questList");
        questListParent = serializedObject.FindProperty("questListParent");
        questListToggle = serializedObject.FindProperty("questListToggle");
        cmpltQuestList = serializedObject.FindProperty("cmpltQuestList");
        cmpltQuestListParent = serializedObject.FindProperty("cmpltQuestListParent");
        cmpltQuestListToggle = serializedObject.FindProperty("cmpltQuestListToggle");
        descriptionWindow = serializedObject.FindProperty("descriptionWindow");
        descriptionText = serializedObject.FindProperty("descriptionText");
        abandonButton = serializedObject.FindProperty("abandonButton");
        traceButton = serializedObject.FindProperty("traceButton");
        closeDescription = serializedObject.FindProperty("closeDescription");
        moneyText = serializedObject.FindProperty("moneyText");
        EXPText = serializedObject.FindProperty("EXPText");
        rewardCellPrefab = serializedObject.FindProperty("rewardCellPrefab");
        rewardCellsParent = serializedObject.FindProperty("rewardCellsParent");
        questBoard = serializedObject.FindProperty("questBoard");
        boardQuestPrefab = serializedObject.FindProperty("boardQuestPrefab");
        questBoardArea = serializedObject.FindProperty("questBoardArea");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("任务窗口相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(questsWindow, new GUIContent("任务窗口"));
        EditorGUILayout.PropertyField(closeWindow, new GUIContent("关闭任务窗口"));
        EditorGUILayout.PropertyField(questPrefab, new GUIContent("任务载体预制件"));
        EditorGUILayout.PropertyField(questGroupPrefab, new GUIContent("任务组载体预制件"));
        EditorGUILayout.PropertyField(questListParent, new GUIContent("进行中任务放置根"));
        EditorGUILayout.PropertyField(questList, new GUIContent("进行中任务页面"));
        EditorGUILayout.PropertyField(questListToggle, new GUIContent("进行中任务页面切换器"));
        EditorGUILayout.PropertyField(cmpltQuestListParent, new GUIContent("已完成任务放置根"));
        EditorGUILayout.PropertyField(cmpltQuestList, new GUIContent("已完成任务页面"));
        EditorGUILayout.PropertyField(cmpltQuestListToggle, new GUIContent("已完成任务页面切换器"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("任务详情相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(descriptionWindow, new GUIContent("任务详情窗口"));
        EditorGUILayout.PropertyField(descriptionText, new GUIContent("任务描述文字"));
        EditorGUILayout.PropertyField(abandonButton, new GUIContent("放弃按钮"));
        EditorGUILayout.PropertyField(traceButton, new GUIContent("追踪按钮"));
        EditorGUILayout.PropertyField(closeDescription, new GUIContent("关闭详情窗口"));
        EditorGUILayout.PropertyField(moneyText, new GUIContent("金钱奖励文本"));
        EditorGUILayout.PropertyField(EXPText, new GUIContent("经验奖励文本"));
        EditorGUILayout.PropertyField(rewardCellPrefab, new GUIContent("道具奖励格预制件"), true);
        EditorGUILayout.PropertyField(rewardCellsParent, new GUIContent("道具奖励放置根"), true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("任务栏相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(questBoard, new GUIContent("任务栏"));
        EditorGUILayout.PropertyField(boardQuestPrefab, new GUIContent("栏任务载体预制件"));
        EditorGUILayout.PropertyField(questBoardArea, new GUIContent("任务栏放置根"), true);
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}