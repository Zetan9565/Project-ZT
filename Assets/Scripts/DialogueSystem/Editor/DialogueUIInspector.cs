using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueUI))]
public class DialogueUIInspector : Editor
{
    //DialogueUI dialogueUI;

    SerializedProperty dialogueWindow;
    SerializedProperty nameText;
    SerializedProperty wordsText;
    SerializedProperty warehouseButton;
    SerializedProperty shopButton;
    SerializedProperty backButton;
    SerializedProperty finishButton;
    SerializedProperty optionsParent;
    SerializedProperty optionPrefab;
    SerializedProperty pageUpButton;
    SerializedProperty pageDownButton;
    SerializedProperty pageText;
    SerializedProperty textLineHeight;
    SerializedProperty lineAmount;
    SerializedProperty questButton;
    SerializedProperty descriptionWindow;
    SerializedProperty descriptionText;
    SerializedProperty moneyText;
    SerializedProperty EXPText;
    SerializedProperty rewardCellPrefab;
    SerializedProperty rewardCellsParent;

    private void OnEnable()
    {
        //dialogueUI = target as DialogueUI;

        dialogueWindow = serializedObject.FindProperty("dialogueWindow");
        nameText = serializedObject.FindProperty("nameText");
        wordsText = serializedObject.FindProperty("nameText");
        warehouseButton = serializedObject.FindProperty("warehouseButton");
        shopButton = serializedObject.FindProperty("shopButton");
        backButton = serializedObject.FindProperty("backButton");
        finishButton = serializedObject.FindProperty("finishButton");
        optionsParent = serializedObject.FindProperty("optionsParent");
        optionPrefab = serializedObject.FindProperty("optionPrefab");
        pageUpButton = serializedObject.FindProperty("pageUpButton");
        pageDownButton = serializedObject.FindProperty("pageDownButton");
        pageText = serializedObject.FindProperty("pageText");
        textLineHeight = serializedObject.FindProperty("textLineHeight");
        lineAmount = serializedObject.FindProperty("lineAmount");
        questButton = serializedObject.FindProperty("questButton");
        descriptionWindow = serializedObject.FindProperty("descriptionWindow");
        descriptionText = serializedObject.FindProperty("descriptionText");
        moneyText = serializedObject.FindProperty("moneyText");
        EXPText = serializedObject.FindProperty("EXPText");
        rewardCellPrefab = serializedObject.FindProperty("rewardCellPrefab");
        rewardCellsParent = serializedObject.FindProperty("rewardCellsParent");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("对话框相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(dialogueWindow, new GUIContent("对话框"));
        EditorGUILayout.PropertyField(nameText, new GUIContent("说话者名字"));
        EditorGUILayout.PropertyField(wordsText, new GUIContent("语句文字"));
        EditorGUILayout.PropertyField(warehouseButton, new GUIContent("仓库按钮"));
        EditorGUILayout.PropertyField(shopButton, new GUIContent("商店按钮"));
        EditorGUILayout.PropertyField(backButton, new GUIContent("返回按钮"));
        EditorGUILayout.PropertyField(finishButton, new GUIContent("结束按钮"));
        EditorGUILayout.PropertyField(textLineHeight, new GUIContent("每行高度"));
        EditorGUILayout.PropertyField(lineAmount, new GUIContent("总行数"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("选项相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(optionPrefab, new GUIContent("选项预制体"));
        EditorGUILayout.PropertyField(optionsParent, new GUIContent("选项放置根"));
        EditorGUILayout.PropertyField(pageUpButton, new GUIContent("上翻页按钮"));
        EditorGUILayout.PropertyField(pageDownButton, new GUIContent("下翻页按钮"));
        EditorGUILayout.PropertyField(pageText, new GUIContent("页数文字"));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("任务相关", new GUIStyle() { fontStyle = FontStyle.Bold });
        EditorGUILayout.PropertyField(questButton, new GUIContent("任务按钮"));
        EditorGUILayout.PropertyField(descriptionWindow, new GUIContent("任务描述窗口"));
        EditorGUILayout.PropertyField(descriptionText, new GUIContent("任务描述文字"));
        EditorGUILayout.PropertyField(moneyText, new GUIContent("金钱奖励文字"));
        EditorGUILayout.PropertyField(EXPText, new GUIContent("经验奖励文字"));
        EditorGUILayout.PropertyField(rewardCellPrefab, new GUIContent("道具奖励格预制体"), true);
        EditorGUILayout.PropertyField(rewardCellsParent, new GUIContent("道具奖励放置根"), true);
        EditorGUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}