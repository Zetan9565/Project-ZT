using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public sealed class SharedVariableListDrawer
    {
        private readonly ReorderableList variableList;

        public SharedVariableListDrawer(SerializedObject serializedObject, SerializedProperty serializedVariables, bool isShared)
        {
            variableList = new ReorderableList(serializedObject, serializedVariables, true, true, true, true);
            variableList.drawElementCallback = (rect, index, isFocused, isActive) =>
            {
                if (serializedObject.targetObject)
                {
                    serializedObject.Update();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variable = serializedVariables.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable, true);
                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                }
            };
            variableList.elementHeightCallback = (index) =>
            {
                return EditorGUI.GetPropertyHeight(serializedVariables.GetArrayElementAtIndex(index), true);
            };
            variableList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("新建类型"), false, CreateVariableScript);
                menu.AddSeparator("");
                foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                {
                    if (!type.IsGenericType) menu.AddItem(new GUIContent(type.Name), false, () => { InsertNewVariable(type); });
                }
                menu.DropDown(rect);
            };
            variableList.onRemoveCallback = (list) =>
            {
                serializedObject.Update();
                SerializedProperty _name = serializedVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                if (EditorUtility.DisplayDialog("删除变量", $"确定要删除变量 {_name.stringValue} 吗？", "确定", "取消"))
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                serializedObject.ApplyModifiedProperties();
            };
            variableList.onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            };
            variableList.drawHeaderCallback = (rect) =>
            {
                string typeMsg = EditorUtility.IsPersistent(serializedObject.targetObject) ? "(不可引用场景对象)" : "(可引用场景对象)";
                EditorGUI.LabelField(rect, $"{(isShared ? "共享变量列表" : "全局变量列表")}{typeMsg}");
            };
            variableList.serializedProperty = serializedVariables;

            void InsertNewVariable(Type type)
            {
                serializedObject.Update();
                int index = serializedVariables.arraySize;
                serializedVariables.InsertArrayElementAtIndex(index);
                SerializedProperty element = serializedVariables.GetArrayElementAtIndex(index);
                SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                string newName = $"{char.ToLower(type.Name[0])}{type.Name.Substring(1)}_{serializedVariables.arraySize}";
                variable.GetType().GetField("_name", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(variable, newName);
                variable.isShared = isShared;
                variable.isGlobal = !isShared;
                element.managedReferenceValue = variable;
                serializedObject.ApplyModifiedProperties();
                variableList.Select(serializedVariables.arraySize - 1);
            }
        }

        private void CreateVariableScript()
        {
            var settings = BehaviourTreeSettings.GetOrCreate();
            string path = $"{settings.newVarScriptFolder}/{ScriptTemplate.Variable.folder}";
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            UnityEngine.Object script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);

            string templatePath = AssetDatabase.GetAssetPath(ScriptTemplate.Variable.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, ScriptTemplate.Variable.fileName);
        }

        public void DoLayoutList()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < variableList.serializedProperty.arraySize; i++)
            {
                SerializedProperty variable = variableList.serializedProperty.GetArrayElementAtIndex(i);
                string varName = variable.FindPropertyRelative("_name").stringValue;
                names.Add(varName);
            }
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    EditorGUILayout.HelpBox("有未命名的变量", MessageType.Error);
                    break;
                }
                else if (names.FindAll(x => x == name).Count > 1)
                {
                    EditorGUILayout.HelpBox("有名字重复的变量", MessageType.Error);
                    break;
                }
            }
            variableList.DoLayoutList();
        }
    }

    public sealed class SharedVariablePresetListDrawer
    {
        private readonly ReorderableList presetVariableList;

        private readonly string[] varType = { "填写", "选择" };

        public SharedVariablePresetListDrawer(SerializedObject serializedObject, SerializedProperty presetVariables, ISharedVariableHandler variableHandler, Func<int, Type> elementTypeGetter)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineHeightSpace = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            presetVariableList = new ReorderableList(serializedObject, presetVariables, true, true, true, true);
            presetVariableList.drawElementCallback = (rect, index, isFocused, isActive) =>
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                Type type = elementTypeGetter(index);
                SerializedProperty name = variable.FindPropertyRelative("_name");
                List<SharedVariable> variables = variableHandler.GetVariables(type);
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable,
                    new GUIContent(string.IsNullOrEmpty(name.stringValue) ? $"({type.Name})" : name.stringValue));
                Rect valueRect = new Rect(rect.x, rect.y + lineHeightSpace, rect.width * 0.84f, lineHeight);
                if (variable.isExpanded)
                {
                    SerializedProperty isShared = variable.FindPropertyRelative("isShared");
                    int typeIndex = isShared.boolValue ? 1 : 0;
                    if (typeIndex == 1)
                    {
                        string[] varNames = variables.Select(x => x.name).ToArray();
                        bool noNames = varNames.Length < 1;
                        int nameIndex = Array.IndexOf(varNames, name.stringValue);
                        if (noNames) varNames = varNames.Prepend("未选择").ToArray();
                        if (nameIndex < 0) nameIndex = 0;
                        nameIndex = EditorGUI.Popup(valueRect, "关联变量名称", nameIndex, varNames);
                        string nameStr = name.stringValue;
                        if (!noNames && nameIndex >= 0 && nameIndex < variables.Count) nameStr = varNames[nameIndex];
                        name.stringValue = nameStr;
                    }
                    else EditorGUI.PropertyField(valueRect, name, new GUIContent("关联变量名称"));
                    typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width * 0.84f + 2, rect.y + lineHeightSpace, rect.width * 0.16f - 2, lineHeight), typeIndex, varType);
                    switch (typeIndex)
                    {
                        case 0:
                            isShared.boolValue = false;
                            break;
                        case 1:
                            isShared.boolValue = true;
                            break;
                    }
                    SerializedProperty value = variable.FindPropertyRelative("value");
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, EditorGUI.GetPropertyHeight(value, true) - lineHeight), value, new GUIContent("预设值"), true);
                }
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            };
            presetVariableList.elementHeightCallback = (index) =>
            {
                SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                if (variable.isExpanded)
                {
                    return EditorGUI.GetPropertyHeight(presetVariables.GetArrayElementAtIndex(index), true);
                }
                else return lineHeightSpace;
            };
            presetVariableList.onAddDropdownCallback = (rect, list) =>
            {
                GenericMenu menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                {
                    if (!type.IsGenericType)
                    {
                        Type g = type.BaseType.GetGenericArguments()[0];
                        if (g.IsSubclassOf(typeof(UnityEngine.Object)) || g.IsGenericType && typeof(IList).IsAssignableFrom(g) && g.GetGenericArguments()[0].IsSubclassOf(typeof(UnityEngine.Object)))
                            menu.AddItem(new GUIContent($"自定义/{type.Name}"), false, () => { InsertNewVariable(type, false); });
                    }
                }
                List<SharedVariable> variables = ZetanEditorUtility.GetValue(presetVariables) as List<SharedVariable>;
                foreach (var variable in variableHandler.Variables)
                {
                    Type type = variable.GetType();
                    if (!variables.Exists(x => x.name == variable.name && x.GetType() == type) && type.BaseType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityEngine.Object)))
                        menu.AddItem(new GUIContent(variable.name), false, () => { InsertNewVariable(type, true, variable.name); });
                }
                menu.DropDown(rect);
            };
            presetVariableList.onRemoveCallback = (list) =>
            {
                serializedObject.Update();
                SerializedProperty _name = presetVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                if (EditorUtility.DisplayDialog("删除变量", $"确定要删除预设 {_name.stringValue} 吗？", "确定", "取消"))
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                serializedObject.ApplyModifiedProperties();
            };
            presetVariableList.onCanRemoveCallback = (list) =>
            {
                return list.IsSelected(list.index);
            };
            presetVariableList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, $"变量预设列表");
            };
            presetVariableList.serializedProperty = presetVariables;

            void InsertNewVariable(Type type, bool select, string name = "")
            {
                serializedObject.Update();
                int index = presetVariables.arraySize;
                presetVariables.InsertArrayElementAtIndex(index);
                SerializedProperty element = presetVariables.GetArrayElementAtIndex(index);
                SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                variable.GetType().GetField("_name", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(variable, name);
                variable.isShared = select;
                element.managedReferenceValue = variable;
                serializedObject.ApplyModifiedProperties();
                presetVariableList.Select(presetVariables.arraySize - 1);
            }
        }

        public void DoLayoutList()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < presetVariableList.serializedProperty.arraySize; i++)
            {
                SerializedProperty variable = presetVariableList.serializedProperty.GetArrayElementAtIndex(i);
                string varName = variable.FindPropertyRelative("_name").stringValue;
                names.Add(varName);
            }
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    EditorGUILayout.HelpBox("有未命名的变量", MessageType.Error);
                    break;
                }
                else if (names.FindAll(x => x == name).Count > 1)
                {
                    EditorGUILayout.HelpBox("有名字重复的变量", MessageType.Error);
                    break;
                }
            }
            presetVariableList.DoLayoutList();
        }
    }
}