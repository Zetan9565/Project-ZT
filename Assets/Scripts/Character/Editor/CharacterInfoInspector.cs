using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(CharacterInformation), true)]
public partial class CharacterInfoInspector : Editor
{
    CharacterInformation character;
    SerializedProperty _ID;
    SerializedProperty _Name;
    SerializedProperty sex;

    PlayerInformation player;
    SerializedProperty backpack;
    SerializedProperty attribute;
    RoleAttributeGroupDrawer attrDrawer;

    float lineHeight;
    float lineHeightSpace;

    CharacterInformation[] characters;
    List<TalkerInformation> allTalkers;


    private void OnEnable()
    {
        character = target as CharacterInformation;
        enemy = target as EnemyInformation;
        talker = target as TalkerInformation;
        player = target as PlayerInformation;
        characters = Resources.LoadAll<CharacterInformation>("");
        if (talker)
        {
            allTalkers = Resources.LoadAll<TalkerInformation>("").ToList();
            allTalkers.Remove(talker);
        }
        _ID = serializedObject.FindProperty("_ID");
        _Name = serializedObject.FindProperty("_Name");
        sex = serializedObject.FindProperty("sex");

        lineHeight = EditorGUIUtility.singleLineHeight;
        lineHeightSpace = lineHeight + 5;

        if (enemy)
        {
            EnemyInfoEnable();
        }
        else if (talker)
        {
            TalkerInfoEnable();
        }
        else if (player)
        {
            backpack = serializedObject.FindProperty("backpack");
            attribute = serializedObject.FindProperty("attribute");
            attrDrawer = new RoleAttributeGroupDrawer(serializedObject, attribute, lineHeight, lineHeightSpace);
        }
    }

    public override void OnInspectorGUI()
    {
        if (enemy)
        {
            EnemyHeader();
        }
        else if (talker)
        {
            TalkerInfoHeader();
        }
        else if (string.IsNullOrEmpty(character.name) || string.IsNullOrEmpty(character.ID))
            EditorGUILayout.HelpBox("该角色信息未补全。", MessageType.Warning);
        else EditorGUILayout.HelpBox("该角色信息已完整。", MessageType.Info);
        if (!talker)
        {
            serializedObject.Update();
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
            EditorGUILayout.PropertyField(_Name, new GUIContent("名称"));
            if (!enemy) EditorGUILayout.PropertyField(sex, new GUIContent("性别"));
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            if (enemy)
            {
                DrawEnemyInfo();
            }
            else if (player)
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("背包信息");
                SerializedProperty size = backpack.FindPropertyRelative("size");
                SerializedProperty weight = backpack.FindPropertyRelative("weight");
                size.FindPropertyRelative("max").intValue = EditorGUILayout.IntSlider("默认容量(格)", size.FindPropertyRelative("max").intValue, 30, 200);
                weight.FindPropertyRelative("max").floatValue = EditorGUILayout.Slider("默认负重(WL)", weight.FindPropertyRelative("max").floatValue, 100, 1000);
                attrDrawer.DoLayoutDraw();
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            DrawTalkerInfo();
        }
    }

    string GetAutoID()
    {
        string newID = string.Empty;
        if (enemy)
        {
            EnemyInformation[] enemies = Resources.LoadAll<EnemyInformation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "ENMY" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(enemies, x => x.ID == newID))
                    break;
            }
        }
        else if (talker)
        {
            TalkerInformation[] talkers = Resources.LoadAll<TalkerInformation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "NPC" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(talkers, x => x.ID == newID))
                    break;
            }
        }
        else if (player)
        {
            PlayerInformation[] players = Resources.LoadAll<PlayerInformation>("");
            for (int i = 1; i < 1000; i++)
            {
                newID = "PLAY" + i.ToString().PadLeft(3, '0');
                if (!Array.Exists(players, x => x.ID == newID))
                    break;
            }
        }
        else
        {
            CharacterInformation[] characters = Resources.LoadAll<CharacterInformation>("").Where(x => !(x is EnemyInformation) && !(x is TalkerInformation)).ToArray();
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