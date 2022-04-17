using System;
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
            variableList = new ReorderableList(serializedObject, serializedVariables, true, true, true, true)
            {
                drawElementCallback = (rect, index, isFocused, isActive) =>
                {
                    if (serializedObject.targetObject)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        SerializedProperty variable = serializedVariables.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable, true);
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                        if (ZetanEditorUtility.TryGetValue(variable, out var value) && value is SharedVariable sv
                        && sv.GetType().GetField("linkedVariables", ZetanUtility.CommonBindingFlags).GetValue(sv) is HashSet<SharedVariable> lvs)
                            foreach (var lv in lvs)
                            {
                                lv.GetType().GetField("_name", ZetanUtility.CommonBindingFlags).SetValue(lv, sv.name);
                            }
                    }
                },
                elementHeightCallback = (index) =>
                {
                    if (index >= serializedVariables.arraySize) return 0;
                    return EditorGUI.GetPropertyHeight(serializedVariables.GetArrayElementAtIndex(index), true);
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("新建类型"), false, CreateVariableScript);
                    menu.AddSeparator("");
                    foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                    {
                        if (!type.IsGenericType) menu.AddItem(new GUIContent(type.Name), false, () => { InsertNewVariable(type); });
                    }
                    menu.DropDown(rect);
                },
                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    SerializedProperty _name = serializedVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                    if (EditorUtility.DisplayDialog("删除变量", $"确定要删除变量 {_name.stringValue} 吗？", "确定", "取消"))
                    {
                        if (ZetanEditorUtility.TryGetValue(serializedVariables.GetArrayElementAtIndex(list.index), out var value) && value is SharedVariable sv
                            && sv.GetType().GetField("linkedVariables", ZetanUtility.CommonBindingFlags).GetValue(sv) is HashSet<SharedVariable> lvs)
                            foreach (var lv in lvs)
                            {
                                lv.GetType().GetField("_name", ZetanUtility.CommonBindingFlags).SetValue(lv, string.Empty);
                                lv.GetType().GetField("linkedVariable", ZetanUtility.CommonBindingFlags).SetValue(lv, null);
                            }
                        list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                    }
                    serializedObject.ApplyModifiedProperties();
                },
                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },
                drawHeaderCallback = (rect) =>
                {
                    string typeMsg = EditorUtility.IsPersistent(serializedObject.targetObject) ? "(不可引用场景对象)" : "(可引用场景对象)";
                    EditorGUI.LabelField(rect, $"{(isShared ? "共享变量列表" : "全局变量列表")}{typeMsg}");
                },
                serializedProperty = serializedVariables
            };

            void InsertNewVariable(Type type)
            {
                if (ZetanEditorUtility.TryGetValue(serializedVariables, out var value) && value is List<SharedVariable> list)
                {
                    SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                    string newName = $"{char.ToLower(type.Name[0])}{type.Name.Substring(1)}_{serializedVariables.arraySize}";
                    variable.GetType().GetField("_name", ZetanUtility.CommonBindingFlags).SetValue(variable, newName);
                    variable.isShared = isShared;
                    variable.isGlobal = !isShared;
                    list.Add(variable);
                    variableList.Select(serializedVariables.arraySize);
                }
                else Debug.LogError("添加失败，请检查变量列表归属");
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
            presetVariableList = new ReorderableList(serializedObject, presetVariables, true, true, true, true)
            {
                drawElementCallback = (rect, index, isFocused, isActive) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                    Type type = elementTypeGetter(index);
                    SerializedProperty name = variable.FindPropertyRelative("_name");
                    List<SharedVariable> variables = variableHandler.GetVariables(type);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), variable,
                        new GUIContent(string.IsNullOrEmpty(name.stringValue) ? $"({type.Name})" : name.stringValue));
                    Rect valueRect = new Rect(rect.x, rect.y + lineHeightSpace, rect.width - 46, lineHeight);
                    if (variable.isExpanded)
                    {
                        //这里的isShared用来标识变量名的填写方式，不作正常用途
                        SerializedProperty isShared = variable.FindPropertyRelative("isShared");
                        int typeIndex = isShared.boolValue ? 1 : 0;
                        typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width - 44, rect.y + lineHeightSpace, 44, lineHeight), typeIndex, varType);
                        switch (typeIndex)
                        {
                            case 0:
                                isShared.boolValue = false;
                                break;
                            case 1:
                                isShared.boolValue = true;
                                break;
                        }
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
                        SerializedProperty value = variable.FindPropertyRelative("value");
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + lineHeightSpace * 2, rect.width, EditorGUI.GetPropertyHeight(value, true) - lineHeight), value, new GUIContent("预设值"), true);
                    }
                    if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = (index) =>
                {
                    if (index >= presetVariables.arraySize) return 0;
                    SerializedProperty variable = presetVariables.GetArrayElementAtIndex(index);
                    if (variable.isExpanded)
                    {
                        return EditorGUI.GetPropertyHeight(presetVariables.GetArrayElementAtIndex(index), true);
                    }
                    else return lineHeightSpace;
                },
                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in TypeCache.GetTypesDerivedFrom<SharedVariable>())
                    {
                        if (!type.IsGenericType)
                        {
                            menu.AddItem(new GUIContent($"自定义/{type.Name}"), false, () => { InsertNewVariable(type, false); });
                        }
                    }
                    if (ZetanEditorUtility.TryGetValue(presetVariables, out var value))
                    {
                        List<SharedVariable> variables = value as List<SharedVariable>;
                        foreach (var variable in variableHandler.Variables)
                        {
                            Type type = variable.GetType();
                            if (!variables.Exists(x => x.name == variable.name && x.GetType() == type))
                                menu.AddItem(new GUIContent(variable.name), false, () => { InsertNewVariable(type, true, variable.name); });
                        }
                    }
                    menu.DropDown(rect);
                },
                onRemoveCallback = (list) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    SerializedProperty _name = presetVariables.GetArrayElementAtIndex(list.index).FindPropertyRelative("_name");
                    if (EditorUtility.DisplayDialog("删除变量", $"确定要删除预设 {_name.stringValue} 吗？", "确定", "取消"))
                    {
                        list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                        GUIUtility.ExitGUI();
                    }
                    serializedObject.ApplyModifiedProperties();
                },
                onCanRemoveCallback = (list) =>
                {
                    return list.IsSelected(list.index);
                },
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, $"变量预设列表");
                },
                serializedProperty = presetVariables
            };

            void InsertNewVariable(Type type, bool select, string name = "")
            {
                if (ZetanEditorUtility.TryGetValue(presetVariables, out var value) && value is List<SharedVariable> list)
                {
                    SharedVariable variable = (SharedVariable)Activator.CreateInstance(type);
                    variable.GetType().GetField("_name", ZetanUtility.CommonBindingFlags).SetValue(variable, name);
                    variable.isShared = select;
                    list.Add(variable);
                    presetVariableList.Select(presetVariables.arraySize);
                }
                else Debug.LogError("添加失败，请检查变量列表归属");
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