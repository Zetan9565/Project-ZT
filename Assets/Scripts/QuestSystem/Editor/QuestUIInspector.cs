using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuestUI))]
public class QuestUIInspector : Editor
{
    //QuestUI UI;

    SerializedProperty questsWindow;
    SerializedProperty openWindow;
    SerializedProperty closeWindow;
    SerializedProperty questPrefab;
    SerializedProperty questGroupPrefab;
    SerializedProperty questListParent;
    SerializedProperty cmpltQuestListParent;
    SerializedProperty descriptionWindow;
    SerializedProperty descriptionText;
    SerializedProperty abandonButton;
    SerializedProperty closeDescription;
    SerializedProperty money_EXPText;
    SerializedProperty rewardCells;
    SerializedProperty boardQuestPrefab;
    SerializedProperty questBoardArea;

    private void OnEnable()
    {
        //UI = target as QuestUI;

        questsWindow = serializedObject.FindProperty("questsWindow");
        openWindow = serializedObject.FindProperty("openWindow");
        closeWindow = serializedObject.FindProperty("closeWindow");
        questPrefab = serializedObject.FindProperty("questPrefab");
        questGroupPrefab = serializedObject.FindProperty("questGroupPrefab");
        questListParent = serializedObject.FindProperty("questListParent");
        cmpltQuestListParent = serializedObject.FindProperty("cmpltQuestListParent");
        descriptionWindow = serializedObject.FindProperty("descriptionWindow");
        descriptionText = serializedObject.FindProperty("descriptionText");
        abandonButton = serializedObject.FindProperty("abandonButton");
        closeDescription = serializedObject.FindProperty("closeDescription");
        money_EXPText = serializedObject.FindProperty("money_EXPText");
        rewardCells = serializedObject.FindProperty("rewardCells");
        boardQuestPrefab = serializedObject.FindProperty("boardQuestPrefab");
        questBoardArea = serializedObject.FindProperty("questBoardArea");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(questsWindow, new GUIContent("任务窗口"));
        EditorGUILayout.PropertyField(openWindow, new GUIContent("打开任务窗口"));
        EditorGUILayout.PropertyField(closeWindow, new GUIContent("关闭任务窗口"));
        EditorGUILayout.PropertyField(questPrefab, new GUIContent("任务载体预制体"));
        EditorGUILayout.PropertyField(questGroupPrefab, new GUIContent("任务组载体预制体"));
        EditorGUILayout.PropertyField(questListParent, new GUIContent("进行中任务放置根"));
        EditorGUILayout.PropertyField(cmpltQuestListParent, new GUIContent("已完成任务放置根"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(descriptionWindow, new GUIContent("任务详情窗口"));
        EditorGUILayout.PropertyField(descriptionText, new GUIContent("任务描述文字"));
        EditorGUILayout.PropertyField(abandonButton, new GUIContent("放弃按钮"));
        EditorGUILayout.PropertyField(closeDescription, new GUIContent("关闭详情窗口"));
        EditorGUILayout.PropertyField(money_EXPText, new GUIContent("金钱和经验奖励文本"));
        EditorGUILayout.PropertyField(rewardCells, new GUIContent("道具奖励展示槽"), true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.PropertyField(boardQuestPrefab, new GUIContent("栏任务载体预制体"));
        EditorGUILayout.PropertyField(questBoardArea, new GUIContent("任务栏放置根"), true);
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}