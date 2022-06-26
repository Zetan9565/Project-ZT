using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.BehaviourTree.Nodes;
using Action = ZetanStudio.BehaviourTree.Nodes.Action;

namespace ZetanStudio.BehaviourTree.Editor
{
    public sealed class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        public NodeEditor nodeEditor;
        private SerializedObject serializedObject;
        private string searchKey;
        private BehaviourTreeEditorSettings settings;

        public InspectorView()
        {
            settings = BehaviourTreeEditorSettings.GetOrCreate();
        }

        public void InspectNode(BehaviourTree tree, NodeEditor node)
        {
            Clear();
            if (serializedObject != null) serializedObject.Dispose();
            nodeEditor = node;
            if (tree && nodeEditor != null && nodeEditor.node)
            {
                serializedObject = new SerializedObject(tree);
                SerializedProperty nodes = serializedObject.FindProperty("nodes");
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree && nodeEditor != null && nodeEditor.node)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        string label = $"[{Tr("结点名称")}]  {nodeEditor.node.name}  [{Tr("优先级")} {nodeEditor.node.priority}]";
                        EditorGUILayout.LabelField(new GUIContent(label, Tr("信息: ") + label));
                        var nType = nodeEditor.node.GetType();
                        using (SerializedProperty property = nodes.GetArrayElementAtIndex(tree.Nodes.IndexOf(nodeEditor.node)))
                        {
                            using var copy = property;
                            using var end = property.GetEndProperty();
                            bool enter = true;
                            while (copy.NextVisible(enter) && !SerializedProperty.EqualContents(copy, end))
                            {
                                enter = false;
                                string field = copy.name;
                                if (field == "name" || field == "priority" || field == "child" || field == "children" || field == "start" ||
                                field == "isRuntime" || field == "isShared" || field == "isGlobal") continue;
                                EditorGUILayout.PropertyField(copy, property.hasVisibleChildren);
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
            if (tree)
            {
                serializedObject = new SerializedObject(tree);
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree)
                    {
                        serializedObject.UpdateIfRequiredOrScript();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"), new GUIContent(Tr("行为树名称")));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent(Tr("行为树描述")));
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
                    EditorGUILayout.LabelField(Tr("当前选中"));
                    EditorGUILayout.BeginVertical("Box");
                    foreach (var node in nodes)
                    {
                        EditorGUILayout.LabelField($"[{node.node.name}]", $"({node.node.GetType().Name})");
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
                searchKey = EditorGUILayout.TextField(searchKey, EditorStyles.toolbarSearchField);
                string desc = string.Empty;
                bool empty = string.IsNullOrEmpty(searchKey);
                bool showBox = true;
                string getNodeDesc(Type type)
                {
                    return Tr(Node.GetNodeDesc(type));
                }

                foreach (var node in action)
                {
                    desc = getNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showAction = EditorGUILayout.BeginFoldoutHeaderGroup(showAction, Tr("行为结点"));
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
                    desc = getNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showConditional = EditorGUILayout.BeginFoldoutHeaderGroup(showConditional, Tr("条件结点"));
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
                    desc = getNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showComposite = EditorGUILayout.BeginFoldoutHeaderGroup(showComposite, Tr("复合结点"));
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
                    desc = getNodeDesc(node.Key);
                    if (Contains(node.Key.Name, desc, searchKey))
                    {
                        if (showBox)
                        {
                            EditorGUILayout.BeginVertical("Box");
                            showDecorator = EditorGUILayout.BeginFoldoutHeaderGroup(showDecorator, Tr("修饰结点"));
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
            });
            Add(container);
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
    }
}