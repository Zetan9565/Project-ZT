using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.CharacterSystem.Editor
{
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
            if (string.IsNullOrEmpty(enemy.Name) || string.IsNullOrEmpty(enemy.ID) || enemy.DropItems && enemy.DropItems.Products.Any(x => !x.Item))
                EditorGUILayout.HelpBox("该敌人信息未补全。", MessageType.Warning);
            else EditorGUILayout.HelpBox("该敌人信息已完整。", MessageType.Info);
        }

        void DrawEnemyInfo()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(race, new GUIContent("种族"));
            EditorGUILayout.PropertyField(SMParams, new GUIContent("状态机参数"));
            EditorGUILayout.PropertyField(dropItems, new GUIContent("掉落道具"));
            if (dropItems.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(lootPrefab, new GUIContent("掉落道具预制件"));
            }
            //EditorGUILayout.PropertyField(attribute, new GUIContent("属性"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}