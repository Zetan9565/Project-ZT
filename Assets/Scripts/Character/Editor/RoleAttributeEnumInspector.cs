using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZetanStudio.CharacterSystem
{
    //[CustomEditor(typeof(RoleAttributeEnum))]
    public class RoleAttributeEnumInspector : UnityEditor.Editor
    {
        SerializedProperty names;
        SerializedProperty types;

        ReorderableList list;
        Dictionary<string, RoleValueType> dict;

        private void OnEnable()
        {
            if (Utility.TryGetValue("attributeTypes", target, out var value, out _))
                dict = value as Dictionary<string, RoleValueType>;
            names = serializedObject.FindProperty("names");
            types = serializedObject.FindProperty("types");
            list = new ReorderableList(serializedObject, names, false, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty name = names.GetArrayElementAtIndex(index);
                    SerializedProperty type = types.GetArrayElementAtIndex(index);
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 30, EditorGUIUtility.singleLineHeight), $"[{index}]");
                    string nameBef = name.stringValue;
                    EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, (rect.width - 30) / 2 - 2, EditorGUIUtility.singleLineHeight), name, new GUIContent(string.Empty));
                    if (nameBef != name.stringValue && dict.ContainsKey(name.stringValue))
                    {
                        if (EditorUtility.DisplayDialog("错误", $"已存在属性名 [{name.stringValue}]", "确定"))
                            name.stringValue = nameBef;
                    }
                    EditorGUI.PropertyField(new Rect(rect.x + +30 + (rect.width - 30) / 2, rect.y, (rect.width - 30) / 2 - 2, EditorGUIUtility.singleLineHeight), type, new GUIContent(string.Empty));
                },
                onAddCallback = (list) =>
                {
                    string name = "属性0";
                    int i = 1;
                    while (dict.ContainsKey(name))
                    {
                        name = $"属性{i}";
                        i++;
                    }
                    names.arraySize++;
                    names.GetArrayElementAtIndex(names.arraySize - 1).stringValue = name;
                    types.arraySize++;
                    types.GetArrayElementAtIndex(types.arraySize - 1).enumValueIndex = (int)RoleValueType.Integer;
                    list.Select(names.arraySize - 1);
                },
                onRemoveCallback = (list) =>
                {
                    if (list.index >= 0 && list.index < names.arraySize)
                    {
                        if (EditorUtility.DisplayDialog("警告", $"确定删除属性 [{names.GetArrayElementAtIndex(list.index).stringValue}] 吗?", "删除", "取消"))
                        {
                            names.DeleteArrayElementAtIndex(list.index);
                            types.DeleteArrayElementAtIndex(list.index);
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                },
                onCanRemoveCallback = (list) =>
                {
                    return list.selectedIndices.Contains(list.index);
                },
                elementHeightCallback = (index) =>
                {
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                },
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "基础属性类型列表");
                }
            };
        }

        public override void OnInspectorGUI()
        {
            if (dict.ContainsKey(string.Empty)) EditorGUILayout.HelpBox("存在空的属性名!", MessageType.Error);
            else EditorGUILayout.HelpBox("属性类型列表无错误", MessageType.Info);
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            list.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}