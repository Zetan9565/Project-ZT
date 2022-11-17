using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Editor
{
    using Extension;
    using System;

    [CustomEditor(typeof(ScriptableObjectEnum), true)]
    public class ScriptableObjectEnumInspector : UnityEditor.Editor
    {
        SerializedProperty _enum;
        ScriptableObjectEnum obj;

        private Type type;

        private void OnEnable()
        {
            _enum = serializedObject.FindProperty("_enum");
            obj = target as ScriptableObjectEnum;
            type = target.GetType();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("单例", type.BaseType.GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as ScriptableObjectEnum, type, false);
            EditorGUI.EndDisabledGroup();
            var list = Utility.Editor.LoadAssets(type);
            if (list.Count > 1)
            {
                string paths = "存在多个实例：";
                for (int i = 0; i < list.Count; i++)
                {
                    paths += "路径" + (i + 1) + AssetDatabase.GetAssetPath(list[i]);
                }
                EditorGUILayout.HelpBox(paths, MessageType.Error);
            }
            else
            {
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUI.BeginChangeCheck();
                if (obj.GetEnum().ExistsDuplicate(x => x.Name))
                    EditorGUILayout.HelpBox("存在同名枚举值!", MessageType.Error);
                else if (obj.GetEnum().Any(x => string.IsNullOrEmpty(x.Name)))
                    EditorGUILayout.HelpBox("存在无名枚举值!", MessageType.Error);
                else EditorGUILayout.HelpBox("无错误", MessageType.Info);
                EditorGUILayout.PropertyField(_enum, new GUIContent("枚举值"));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
        }
    }
}