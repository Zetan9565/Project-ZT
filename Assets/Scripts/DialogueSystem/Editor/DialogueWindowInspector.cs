using UnityEngine;
using UnityEditor;

namespace ZetanStudio.DialogueSystem.Editor
{

    //[CustomEditor(typeof(DialogueWindow))]
    public class DialogueWindowInspector : WindowInspector
    {

        SerializedProperty nameText;
        SerializedProperty wordsText;
        SerializedProperty buttonArea;
        SerializedProperty giftButton;
        SerializedProperty warehouseButton;
        SerializedProperty shopButton;
        SerializedProperty backButton;
        SerializedProperty optionList;
        SerializedProperty questButton;
        SerializedProperty descriptionWindow;
        SerializedProperty descriptionText;
        SerializedProperty rewardList;

        protected override void EnableOther()
        {
            nameText = serializedObject.FindProperty("nameText");
            wordsText = serializedObject.FindProperty("nameText");
            buttonArea = serializedObject.FindProperty("buttonArea");
            giftButton = serializedObject.FindProperty("giftButton");
            warehouseButton = serializedObject.FindProperty("warehouseButton");
            shopButton = serializedObject.FindProperty("shopButton");
            backButton = serializedObject.FindProperty("backButton");
            optionList = serializedObject.FindProperty("optionList");
            questButton = serializedObject.FindProperty("questButton");
            descriptionWindow = serializedObject.FindProperty("descriptionWindow");
            descriptionText = serializedObject.FindProperty("descriptionText");
            rewardList = serializedObject.FindProperty("rewardList");
        }

        protected override void InspectOther()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("对话框相关", new GUIStyle() { fontStyle = FontStyle.Bold });
            EditorGUILayout.PropertyField(nameText, new GUIContent("说话者名字"));
            EditorGUILayout.PropertyField(wordsText, new GUIContent("语句文字"));
            EditorGUILayout.PropertyField(buttonArea, new GUIContent("按钮区"));
            EditorGUILayout.PropertyField(giftButton, new GUIContent("送礼按钮"));
            EditorGUILayout.PropertyField(warehouseButton, new GUIContent("仓库按钮"));
            EditorGUILayout.PropertyField(shopButton, new GUIContent("商店按钮"));
            EditorGUILayout.PropertyField(backButton, new GUIContent("返回按钮"));
            EditorGUILayout.PropertyField(optionList, new GUIContent("选项放置根"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("任务相关", new GUIStyle() { fontStyle = FontStyle.Bold });
            EditorGUILayout.PropertyField(questButton, new GUIContent("任务按钮"));
            EditorGUILayout.PropertyField(descriptionWindow, new GUIContent("任务描述窗口"));
            EditorGUILayout.PropertyField(descriptionText, new GUIContent("任务描述文字"));
            EditorGUILayout.PropertyField(rewardList, new GUIContent("道具奖励列表"), true);
            EditorGUILayout.EndVertical();
        }
    }
}