using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ZetanStudio.BehaviourTree
{
    public sealed class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        private BehaviourTree tree;
        private GlobalVariables global;
        public NodeEditor nodeEditor;
        private SerializedObject serializedObject;
        private string searchKey;

        private readonly string[] varType = { "普通", "共享", "全局" };

        public InspectorView() { }

        public void InspectNode(BehaviourTree tree, NodeEditor node)
        {
            Clear();
            if (serializedObject != null) serializedObject.Dispose();
            this.tree = tree;
            if (!tree.IsInstance) global = ZetanEditorUtility.LoadAsset<GlobalVariables>();
            else global = BehaviourManager.Instance.GlobalVariables;
            nodeEditor = node;
            if (tree && nodeEditor != null && nodeEditor.node)
            {
                serializedObject = new SerializedObject(tree);
                SerializedProperty nodes = serializedObject.FindProperty("nodes");
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree && nodeEditor != null && nodeEditor.node)
                    {
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField("结点名称", nodeEditor.node.name);
                        var nType = nodeEditor.node.GetType();
                        var fields = new HashSet<string>(nType.GetFields(ZetanUtility.CommonBindingFlags).Select(f => f.Name));
                        var fieldsMap = new HashSet<string>();
                        using (SerializedProperty property = nodes.GetArrayElementAtIndex(tree.Nodes.IndexOf(nodeEditor.node)))
                        {
                            using var end = property.GetEndProperty();
                            property.Next(true);
                            while (property.NextVisible(false) && !SerializedProperty.EqualContents(property, end))
                            {
                                string field = property.name;
                                if (field == "name" || field == "child" || field == "children" || field == "start" ||
                                field == "isRuntime" || field == "isShared" || field == "isGlobal") continue;
                                if (fields.Contains(field))
                                {
                                    if (fieldsMap.Contains(field)) break;
                                    fieldsMap.Add(field);
                                    var fInfo = nType.GetField(field, ZetanUtility.CommonBindingFlags);
                                    DrawProperty(property, fInfo);
                                }
                            }
                        }
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    }
                });
                Add(container);
            }
        }

        public void InspectTree(BehaviourTree tree)
        {
            Clear();
            if (serializedObject != null) serializedObject.Dispose();
            this.tree = tree;
            if (tree)
            {
                serializedObject = new SerializedObject(tree);
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree)
                    {
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"), new GUIContent("行为树名称"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("行为树描述"));
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    }
                });
                Add(container);
            }
        }

        public void InspectMultSelect(List<NodeEditor> nodes)
        {
            Clear();
            if (serializedObject != null) serializedObject.Dispose();
            if (nodes.Count() > 0)
            {
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    EditorGUILayout.LabelField("当前选中");
                    EditorGUILayout.BeginVertical("Box");
                    foreach (var node in nodes)
                    {
                        EditorGUILayout.LabelField($"[{node.node.name}]", $"({ node.node.GetType().Name})");
                    }
                    EditorGUILayout.EndVertical();
                });
                Add(container);
            }
        }
        bool showAction = true;
        bool showConditional = true;
        bool showComposite = true;
        bool showDecorator = true;
        public void InspectNodes(Action<Type> insertCallback)
        {
            Clear();
            Dictionary<Type, System.Action> action = new Dictionary<Type, System.Action>();
            Dictionary<Type, System.Action> conditional = new Dictionary<Type, System.Action>();
            Dictionary<Type, System.Action> composite = new Dictionary<Type, System.Action>();
            Dictionary<Type, System.Action> decorator = new Dictionary<Type, System.Action>();
            var types = TypeCache.GetTypesDerivedFrom<Action>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    action.Add(type, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Conditional>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    conditional.Add(type, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Composite>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    composite.Add(type, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Decorator>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    decorator.Add(type, () => insertCallback(type));
            }

            searchKey = string.Empty;
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                searchKey = EditorGUILayout.TextField(searchKey);
                string desc = string.Empty;
                bool empty = string.IsNullOrEmpty(searchKey);
                bool showBox = true;
                foreach (var node in action)
                {
                    desc = GetNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showAction = EditorGUILayout.BeginFoldoutHeaderGroup(showAction, "行为结点");
                            showBox = false;
                        }
                        if (showAction)
                            if (GUILayout.Button(new GUIContent(node.Key.Name, desc)))
                                node.Value?.Invoke();
                    }
                }
                if (!showBox)
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.EndVertical();
                }

                showBox = true;
                foreach (var node in conditional)
                {
                    desc = GetNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showConditional = EditorGUILayout.BeginFoldoutHeaderGroup(showConditional, "条件结点");
                            showBox = false;
                        }
                        if (showConditional)
                            if (GUILayout.Button(new GUIContent(node.Key.Name, desc)))
                                node.Value?.Invoke();
                    }
                }
                if (!showBox)
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.EndVertical();
                }

                showBox = true;
                foreach (var node in composite)
                {
                    desc = GetNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showComposite = EditorGUILayout.BeginFoldoutHeaderGroup(showComposite, "复合结点");
                            showBox = false;
                        }
                        if (showComposite)
                            if (GUILayout.Button(new GUIContent(node.Key.Name, desc)))
                                node.Value?.Invoke();
                    }
                }
                if (!showBox)
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.EndVertical();
                }

                showBox = true;
                foreach (var node in decorator)
                {
                    desc = GetNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showDecorator = EditorGUILayout.BeginFoldoutHeaderGroup(showDecorator, "修饰结点");
                            showBox = false;
                        }
                        if (showDecorator)
                            if (GUILayout.Button(new GUIContent(node.Key.Name, desc)))
                                node.Value?.Invoke();
                    }
                }
                if (!showBox)
                {
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.EndVertical();
                }

                bool Contains(string name, string desc, string key)
                {
                    return empty || name.ToLower().Contains(key.ToLower()) || desc.ToLower().Contains(key.ToLower());
                }

                string GetNodeDesc(Type type)
                {
                    if (type.IsSubclassOf(typeof(Node)))
                    {
                        var descAttr = type.GetCustomAttribute<NodeDescriptionAttribute>();
                        if (descAttr != null)
                            return descAttr.description;
                        else return string.Empty;
                    }
                    else return string.Empty;
                }
            });
            Add(container);
        }

        private void DrawProperty(SerializedProperty property, FieldInfo fieldInfo)
        {
            if (fieldInfo == null) return;
            ShouldHide(fieldInfo, out var shouldHide, out var readOnly);
            if (shouldHide && !readOnly) return;
            string displayName = property.displayName;
            DisplayNameAttribute nameAttr = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
            if (nameAttr != null) displayName = nameAttr.name;
            string tooltip = property.tooltip;
            TooltipAttribute tipAttr = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            if (tipAttr != null) tooltip = tipAttr.tooltip;
            EditorGUI.BeginDisabledGroup(readOnly);
            var type = fieldInfo.FieldType;
            if (type.IsSubclassOf(typeof(SharedVariable)))
            {
                SerializedProperty name = property.FindPropertyRelative("_name");
                SerializedProperty value = property.FindPropertyRelative("value");
                //使用HideInInspector标签后仍可Find出来
                SerializedProperty isShared = property.FindPropertyRelative("isShared");
                SerializedProperty isGlobal = property.FindPropertyRelative("isGlobal");
                int typeIndex = isGlobal.boolValue ? 2 : (isShared.boolValue ? 1 : 0);
                int oldTypeIndex = typeIndex;
                Rect rect = EditorGUILayout.GetControlRect();
                //EditorGUI.BeginDisabledGroup(nodeEditor.node.IsInstance);
                typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width - 32, rect.y, 32, EditorGUIUtility.singleLineHeight), typeIndex, varType);
                if (oldTypeIndex != typeIndex)
                    switch (typeIndex)
                    {
                        case 1:
                            isShared.boolValue = true;
                            isGlobal.boolValue = false;
                            break;
                        case 2:
                            isShared.boolValue = false;
                            isGlobal.boolValue = true;
                            break;
                        default:
                            isShared.boolValue = false;
                            isGlobal.boolValue = false;
                            break;
                    }
                //EditorGUI.EndDisabledGroup();
                switch (typeIndex)
                {
                    case 1:
                        Rect valueRect = new Rect(rect.x, rect.y, rect.width - 34, EditorGUIUtility.singleLineHeight);
                        DrawSharedField(valueRect, tree);
                        break;
                    case 2:
                        valueRect = new Rect(rect.x, rect.y, rect.width - 34, EditorGUIUtility.singleLineHeight);
                        DrawSharedField(valueRect, global);
                        break;
                    default:
                        valueRect = new Rect(rect.x, rect.y, rect.width - 34, EditorGUI.GetPropertyHeight(value, true));
                        if (type == typeof(SharedString) && fieldInfo.GetCustomAttribute<Tag_BTAttribute>() != null)
                            value.stringValue = EditorGUI.TagField(valueRect, new GUIContent(displayName, tooltip),
                                string.IsNullOrEmpty(value.stringValue) ? UnityEditorInternal.InternalEditorUtility.tags[0] : value.stringValue);
                        else EditorGUI.PropertyField(valueRect, value, new GUIContent(displayName, tooltip), true);
                        EditorGUILayout.Space(EditorGUI.GetPropertyHeight(value, true) - EditorGUIUtility.singleLineHeight);
                        break;
                }

                void DrawSharedField(Rect valueRect, ISharedVariableHandler variableHandler)
                {
                    var variables = variableHandler.GetVariables(type);
                    string[] varNames = variables.Select(x => x.name).Prepend("未选择").ToArray();
                    GUIContent[] contents = new GUIContent[varNames.Length];
                    for (int i = 0; i < varNames.Length; i++)
                    {
                        contents[i] = new GUIContent(varNames[i]);
                    }
                    if (ZetanEditorUtility.TryGetValue(property, out var value))
                    {
                        var link = type.GetField("linkedVariable", ZetanUtility.CommonBindingFlags).GetValue(value);
                        if (link != null) name.stringValue = (link as SharedVariable).name;
                    }
                    int nameIndex = Array.IndexOf(varNames, name.stringValue);
                    if (nameIndex < 0) nameIndex = 0;
                    nameIndex = EditorGUI.Popup(valueRect, new GUIContent(displayName, tooltip), nameIndex, contents);
                    string nameStr = string.Empty;
                    if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
                    //if (!nodeEditor.node.IsInstance) name.stringValue = nameStr;
                    //else if (nameStr != name.stringValue)
                    //{
                    //    var val = variableHandler.GetVariable(varNames[nameIndex]);
                    //    if (val == null)
                    //    {
                    //        val = Activator.CreateInstance(type) as SharedVariable;
                    //        val.isShared = variableHandler is BehaviourTree;
                    //        val.isGlobal = variableHandler is GlobalVariables;
                    //        type.GetField("_name", ZetanUtility.CommonBindingFlags).SetValue(val, nameStr);
                    //    }
                    //    ZetanEditorUtility.TrySetValue(property, val);
                    //}
                    if (!nodeEditor.node.IsInstance) name.stringValue = nameStr;
                    if (!string.IsNullOrEmpty(nameStr))
                    {
                        var val = variableHandler.GetVariable(nameStr);
                        if (val != null && value != null) (value as SharedVariable).Link(val);
                    }
                    else (value as SharedVariable).Unlink();
                }
            }
            else if (type == typeof(string))
            {
                if (fieldInfo.GetCustomAttribute<Tag_BTAttribute>() != null)
                    property.stringValue = EditorGUILayout.TagField(new GUIContent(displayName, tooltip),
                        string.IsNullOrEmpty(property.stringValue) ? UnityEditorInternal.InternalEditorUtility.tags[0] : property.stringValue);
                else
                {
                    NameOfVariableAttribute varNameAttr = fieldInfo.GetCustomAttribute<NameOfVariableAttribute>();
                    if (varNameAttr != null)
                    {
                        var variables = tree.GetVariables(varNameAttr.type);
                        string[] varNames = variables.Select(x => x.name).Prepend("未选择").ToArray();
                        GUIContent[] contents = new GUIContent[varNames.Length];
                        for (int i = 0; i < varNames.Length; i++)
                        {
                            contents[i] = new GUIContent(varNames[i]);
                        }
                        int nameIndex = Array.IndexOf(varNames, property.stringValue);
                        if (nameIndex < 0) nameIndex = 0;
                        nameIndex = EditorGUILayout.Popup(new GUIContent(displayName, tooltip), nameIndex, contents);
                        string nameStr = string.Empty;
                        if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
                        property.stringValue = nameStr;
                    }
                    else EditorGUILayout.PropertyField(property, true);
                }
            }
            else EditorGUILayout.PropertyField(property, true);
            EditorGUI.EndDisabledGroup();
        }

        private void ShouldHide(FieldInfo fieldInfo, out bool should, out bool readOnly)
        {
            HideIf_BTAttribute hideAttr = fieldInfo.GetCustomAttribute<HideIf_BTAttribute>();
            ReadOnlyAttribute roAttr = fieldInfo.GetCustomAttribute<ReadOnlyAttribute>();
            should = false;
            readOnly = false;
            if (hideAttr != null)
            {
                readOnly = hideAttr.readOnly;
                if (ZetanUtility.TryGetMemberValue(hideAttr.path, nodeEditor.node, out var fv, out var mi) && mi != null)
                    if (typeof(SharedVariable).IsAssignableFrom(fv.GetType()))
                    {
                        if (fv is SharedVariable sv)
                        {
                            if ((sv.isGlobal || sv.isShared) && !string.IsNullOrEmpty(sv.name))
                            {
                                if (sv.isGlobal) sv = global.GetVariable(sv.name);
                                else if (sv.isShared) sv = tree.GetVariable(sv.name);
                                if (sv != null && Equals(sv.GetValue(), hideAttr.value)) should = true;
                            }
                            else if (!sv.isGlobal && !sv.isShared && Equals(sv.GetValue(), hideAttr.value)) should = true;
                        }
                    }
                    else if (Equals(fv, hideAttr.value)) should = true;
            }
            else if (roAttr != null)
            {
                readOnly = !roAttr.onlyRuntime || roAttr.onlyRuntime && Application.isPlaying;
            }
        }
    }
}