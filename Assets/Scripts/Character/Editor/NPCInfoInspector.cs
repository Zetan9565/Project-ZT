using UnityEditor;
using UnityEngine;

namespace ZetanStudio.CharacterSystem.Editor
{
    public partial class CharacterInfoInspector
    {
        NPCInformation NPC;
        SerializedProperty sex;
        SerializedProperty enable;
        SerializedProperty scene;
        SerializedProperty position;
        SerializedProperty prefab;

        SceneSelectionDrawer sceneSelector;

        void NPCInfoEnable()
        {
            talker = target as TalkerInformation;
            sex = serializedObject.FindProperty("sex");
            enable = serializedObject.FindProperty("enable");
            scene = serializedObject.FindProperty("scene");
            position = serializedObject.FindProperty("position");
            prefab = serializedObject.FindProperty("prefab");
            sceneSelector = new SceneSelectionDrawer(scene);
            if (talker)
            {
                TalkerInfoEnable();
            }
        }

        void NPCInfoHeader()
        {
            if (talker)
            {
                TalkerInfoHeader();
            }
            else
            {
                if (string.IsNullOrEmpty(character.Name) || string.IsNullOrEmpty(character.ID))
                    EditorGUILayout.HelpBox("该角色信息未补全。", MessageType.Warning);
                else EditorGUILayout.HelpBox("该角色信息已完整。", MessageType.Info);
            }
        }

        void DrawNPCInfo()
        {
            if (talker)
            {
                DrawTalkerInfo();
            }
            else
            {
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                bool enableBef = enable.boolValue;
                EditorGUILayout.PropertyField(enable, new GUIContent("启用", "若启用，将在场景中生成实体"));
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
                EditorGUILayout.PropertyField(sex, new GUIContent("性别"));
                if (enable.boolValue)
                {
                    sceneSelector.DoLayoutDraw();
                    EditorGUILayout.PropertyField(position, new GUIContent("位置"));
                    EditorGUILayout.PropertyField(prefab, new GUIContent("预制件"));
                    EditorGUILayout.PropertyField(SMParams, new GUIContent("状态机参数"));
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}