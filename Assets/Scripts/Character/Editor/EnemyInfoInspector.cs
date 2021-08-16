using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class CharacterInfoInspector
{
    EnemyInformation enemy;
    SerializedProperty race;
    SerializedProperty dropItems;
    SerializedProperty lootPrefab;

    void EnemyInfoEnable()
    {
        race = serializedObject.FindProperty("race");
        dropItems = serializedObject.FindProperty("dropItems");
        lootPrefab = serializedObject.FindProperty("lootPrefab");
    }

    void EnemyHeader()
    {
        if (string.IsNullOrEmpty(enemy.name) || string.IsNullOrEmpty(enemy.ID) || enemy.DropItems && enemy.DropItems.Products.Exists(x => !x.Item))
            EditorGUILayout.HelpBox("该敌人信息未补全。", MessageType.Warning);
        else EditorGUILayout.HelpBox("该敌人信息已完整。", MessageType.Info);
    }

    void DrawEnemyInfo()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(race, new GUIContent("种族"));
        if (race.objectReferenceValue)
            EditorGUILayout.LabelField("种族名称", (race.objectReferenceValue as EnemyRace).name);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(dropItems, new GUIContent("掉落道具"));
        if (dropItems.objectReferenceValue)
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(lootPrefab, new GUIContent("掉落道具预制件"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}