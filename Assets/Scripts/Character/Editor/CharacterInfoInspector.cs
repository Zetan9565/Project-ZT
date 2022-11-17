using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.CharacterSystem.Editor
{
    [CustomEditor(typeof(CharacterInformation), true)]
    public partial class CharacterInfoInspector : UnityEditor.Editor
    {
        CharacterInformation character;
        SerializedProperty _ID;
        SerializedProperty _name;

        PlayerInformation player;
        SerializedProperty backpack;
        SerializedProperty SMParams;
        //SerializedProperty attribute;

        float lineHeight;
        float lineHeightSpace;

        CharacterInformation[] characters;

        private void OnEnable()
        {
            character = target as CharacterInformation;
            enemy = target as EnemyInformation;
            NPC = target as NPCInformation;
            player = target as PlayerInformation;
            characters = Resources.LoadAll<CharacterInformation>("Configuration");
            _ID = serializedObject.FindProperty("_ID");
            _name = serializedObject.FindProperty("_name");
            SMParams = serializedObject.FindProperty("_SMParams");
            //attribute = serializedObject.FindProperty("attribute");

            lineHeight = EditorGUIUtility.singleLineHeight;
            lineHeightSpace = lineHeight + 2;

            if (enemy)
            {
                EnemyInfoEnable();
            }
            else if (NPC)
            {
                NPCInfoEnable();
            }
            else if (player)
            {
                backpack = serializedObject.FindProperty("backpack");
            }
        }

        public override void OnInspectorGUI()
        {
            if (enemy)
            {
                EnemyHeader();
            }
            else if (NPC)
            {
                NPCInfoHeader();
            }
            else if (string.IsNullOrEmpty(character.Name) || string.IsNullOrEmpty(character.ID))
                EditorGUILayout.HelpBox("该角色信息未补全。", MessageType.Warning);
            else EditorGUILayout.HelpBox("该角色信息已完整。", MessageType.Info);
            if (!NPC)
            {
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_ID, new GUIContent("识别码"));
                if (string.IsNullOrEmpty(_ID.stringValue) || ExistsID())
                {
                    if (!string.IsNullOrEmpty(_ID.stringValue) && ExistsID()) EditorGUILayout.HelpBox("此识别码已存在！", MessageType.Error);
                    else EditorGUILayout.HelpBox("识别码为空！", MessageType.Error);
                    if (GUILayout.Button("自动生成识别码"))
                    {
                        _ID.stringValue = GetAutoID();
                        EditorGUI.FocusTextInControl(null);
                    }
                }
                EditorGUILayout.PropertyField(_name, new GUIContent("名称"));
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                if (enemy)
                {
                    DrawEnemyInfo();
                }
                else if (player)
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(SMParams, new GUIContent("状态机参数"));
                    //EditorGUILayout.PropertyField(attribute, new GUIContent("属性"));
                    if (EditorGUI.EndChangeCheck())
                        serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                DrawNPCInfo();
            }
        }

        string GetAutoID()
        {
            string newID = string.Empty;
            if (enemy)
            {
                var enemies = characters.Where(x => x is EnemyInformation);
                for (int i = 1; i < 1000; i++)
                {
                    newID = "ENMY" + i.ToString().PadLeft(3, '0');
                    if (!enemies.Any(x => x.ID == newID))
                        break;
                }
            }
            else if (NPC)
            {
                var npcs = characters.Where(x => x is NPCInformation);
                for (int i = 1; i < 1000; i++)
                {
                    newID = "NPC" + i.ToString().PadLeft(3, '0');
                    if (!npcs.Any(x => x.ID == newID))
                        break;
                }
            }
            else if (player)
            {
                var players = characters.Where(x => x is PlayerInformation);
                for (int i = 1; i < 1000; i++)
                {
                    newID = "PLAY" + i.ToString().PadLeft(3, '0');
                    if (!players.Any(x => x.ID == newID))
                        break;
                }
            }
            else
            {
                for (int i = 1; i < 1000; i++)
                {
                    newID = "CHAR" + i.ToString().PadLeft(3, '0');
                    if (!Array.Exists(characters, x => x.ID == newID))
                        break;
                }
            }
            return newID;
        }

        bool ExistsID()
        {
            CharacterInformation find = Array.Find(characters, x => x.ID == _ID.stringValue);
            if (!find) return false;//若没有找到，则ID可用
                                    //找到的对象不是原对象 或者 找到的对象是原对象且同ID超过一个 时为true
            return find != character || (find == character && Array.FindAll(characters, x => x.ID == _ID.stringValue).Length > 1);
        }
    }
}