using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
                serializedObject = new SerializedObject(nodeEditor.node);
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree && nodeEditor != null && nodeEditor.node)
                    {
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField("结点名称", nodeEditor.node.name);
                        SerializedProperty property = serializedObject.GetIterator();
                        property.NextVisible(true);
                        while (property.NextVisible(false))
                        {
                            if (property.name == "child" || property.name == "children" || property.name == "start" || property.name == "isRuntime") continue;
                            DrawProperty(property);
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

        public void InspectNodes(System.Action<System.Type> insertCallback)
        {
            Clear();
            Dictionary<string, System.Action> action = new Dictionary<string, System.Action>();
            Dictionary<string, System.Action> conditional = new Dictionary<string, System.Action>();
            Dictionary<string, System.Action> composite = new Dictionary<string, System.Action>();
            Dictionary<string, System.Action> decorator = new Dictionary<string, System.Action>();
            var types = TypeCache.GetTypesDerivedFrom<Action>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    action.Add(type.Name, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Conditional>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    conditional.Add(type.Name, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Composite>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    composite.Add(type.Name, () => insertCallback(type));
            }

            types = TypeCache.GetTypesDerivedFrom<Decorator>().OrderBy(x => x.Name);
            foreach (var type in types)
            {
                if (!type.IsAbstract && !type.IsGenericType)
                    decorator.Add(type.Name, () => insertCallback(type));
            }

            searchKey = string.Empty;
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                searchKey = EditorGUILayout.TextField(searchKey);
                bool empty = string.IsNullOrEmpty(searchKey);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("行为结点");
                foreach (var node in action)
                {
                    if (Contains(node.Key, searchKey))
                        if (GUILayout.Button(node.Key))
                            node.Value?.Invoke();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("条件结点");
                foreach (var node in conditional)
                {
                    if (Contains(node.Key, searchKey))
                        if (GUILayout.Button(node.Key))
                            node.Value?.Invoke();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("复合结点");
                foreach (var node in composite)
                {
                    if (Contains(node.Key, searchKey))
                        if (GUILayout.Button(node.Key))
                            node.Value?.Invoke();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("修饰结点");
                foreach (var node in decorator)
                {
                    if (Contains(node.Key, searchKey))
                        if (GUILayout.Button(node.Key))
                            node.Value?.Invoke();
                }
                EditorGUILayout.EndVertical();

                bool Contains(string content, string key)
                {
                    return empty || content.ToLower().Contains(key.ToLower());
                }
            });
            Add(container);
        }

        private void DrawProperty(SerializedProperty property)
        {
            if (!ZetanEditorUtility.TryGetValue(property, out var proValue, out var fieldInfo)) return;
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
                SharedVariable variable = proValue as SharedVariable;
                int typeIndex = variable.isGlobal ? 2 : (variable.isShared ? 1 : 0);
                SerializedProperty name = property.FindPropertyRelative("_name");
                SerializedProperty value = property.FindPropertyRelative("value");
                Rect rect = EditorGUILayout.GetControlRect();
                EditorGUI.BeginDisabledGroup(nodeEditor.node.IsInstance);
                typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width - 32, rect.y, 32, EditorGUIUtility.singleLineHeight), typeIndex, varType);
                switch (typeIndex)
                {
                    case 1:
                        variable.isShared = true;
                        variable.isGlobal = false;
                        break;
                    case 2:
                        variable.isShared = false;
                        variable.isGlobal = true;
                        break;
                    default:
                        variable.isShared = false;
                        variable.isGlobal = false;
                        break;
                }
                EditorGUI.EndDisabledGroup();
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
                        if (type == typeof(SharedString) && fieldInfo.GetCustomAttribute<Tag>() != null)
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
                    int nameIndex = System.Array.IndexOf(varNames, variable.name);
                    if (nameIndex < 0) nameIndex = 0;
                    nameIndex = EditorGUI.Popup(valueRect, new GUIContent(displayName, tooltip), nameIndex, contents);
                    string nameStr = string.Empty;
                    if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
                    if (!nodeEditor.node.IsInstance) name.stringValue = nameStr;
                    else if (nameStr != name.stringValue)
                    {
                        var val = variableHandler.GetVariable(varNames[nameIndex]);
                        if (val == null)
                        {
                            val = System.Activator.CreateInstance(type) as SharedVariable;
                            val.isShared = variableHandler is BehaviourTree;
                            val.isGlobal = variableHandler is GlobalVariables;
                            type.GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(val, nameStr);
                        }
                        ZetanEditorUtility.TrySetValue(property, val);
                    }
                }
            }
            else if (type == typeof(string))
            {
                if (fieldInfo.GetCustomAttribute<Tag>() != null)
                    property.stringValue = EditorGUILayout.TagField(new GUIContent(displayName, tooltip),
                        string.IsNullOrEmpty(property.stringValue) ? UnityEditorInternal.InternalEditorUtility.tags[0] : property.stringValue);
                else
                {
                    VariableName varNameAttr = fieldInfo.GetCustomAttribute<VariableName>();
                    if (varNameAttr != null)
                    {
                        var variables = tree.GetVariables(varNameAttr.type);
                        string[] varNames = variables.Select(x => x.name).Prepend("未选择").ToArray();
                        GUIContent[] contents = new GUIContent[varNames.Length];
                        for (int i = 0; i < varNames.Length; i++)
                        {
                            contents[i] = new GUIContent(varNames[i]);
                        }
                        int nameIndex = System.Array.IndexOf(varNames, property.stringValue);
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
            HideIfAttribute hideAttr = fieldInfo.GetCustomAttribute<HideIfAttribute>();
            ReadOnlyAttribute roAttr = fieldInfo.GetCustomAttribute<ReadOnlyAttribute>();
            should = false;
            readOnly = false;
            if (hideAttr != null)
            {
                readOnly = hideAttr.readOnly;
                if (ZetanEditorUtility.TryGetMemberValue(hideAttr.path, nodeEditor.node, out var fv, out _))
                    if (Equals(fv, hideAttr.value))
                        should = true;
            }
            else if (roAttr != null)
            {
                readOnly = !roAttr.onlyRuntime || roAttr.onlyRuntime && Application.isPlaying;
            }
        }
    }
}