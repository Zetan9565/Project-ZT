using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.BehaviourTree
{
    public sealed class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        public BehaviourTree tree;
        public GlobalVariables global;
        public NodeEditor nodeEditor;

        public string[] varType = { "普通", "共享", "全局" };

        public InspectorView() { }

        public void InspectNode(BehaviourTree tree, NodeEditor e)
        {
            Clear();
            this.tree = tree;
            if (!tree.IsInstance) global = ZetanEditorUtility.LoadAssets<GlobalVariables>().Find(g => g);
            else global = BehaviourManager.Instance.GlobalVariables;
            nodeEditor = e;
            if (tree && e != null && e.node)
            {
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (tree && e != null && e.node)
                    {
                        using SerializedObject serializedObject = new SerializedObject(e.node);
                        serializedObject.Update();
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField("结点名称", e.node.name);
                        SerializedProperty property = serializedObject.GetIterator();
                        property.NextVisible(true);
                        while (property.NextVisible(false))
                        {
                            DrawProperty(property);
                        }
                        if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    }
                });
                Add(container);
            }
        }

        private void DrawProperty(SerializedProperty property)
        {
            var proValue = ZetanEditorUtility.GetValue(property, out var fieldInfo);
            var type = fieldInfo.FieldType;
            if (type.IsSubclassOf(typeof(SharedVariable)))
            {
                var variable = proValue as SharedVariable;
                int typeIndex = variable.isGlobal ? 2 : (variable.isShared ? 1 : 0);
                string displayName = property.displayName;
                var attr = fieldInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(DisplayNameAttribute));
                if (attr != null) displayName = attr.ConstructorArguments[0].Value.ToString();
                SerializedProperty name = property.FindPropertyRelative("_name");
                SerializedProperty value = property.FindPropertyRelative("value");
                Rect rect = EditorGUILayout.GetControlRect();
                Rect valueRect = new Rect(rect.x, rect.y, rect.width * 0.8f, EditorGUIUtility.singleLineHeight);
                switch (typeIndex)
                {
                    case 1:
                        DrawVariableField(tree);
                        break;
                    case 2:
                        DrawVariableField(global);
                        break;
                    default:
                        EditorGUI.PropertyField(valueRect, value, new GUIContent(displayName));
                        break;
                }
                EditorGUI.BeginDisabledGroup(nodeEditor.node.IsInstance);
                typeIndex = EditorGUI.Popup(new Rect(rect.x + rect.width * 0.8f + 2, rect.y, rect.width * 0.2f - 2, EditorGUIUtility.singleLineHeight), typeIndex, varType);
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

                void DrawVariableField(ISharedVariableHandler variableHandler)
                {
                    var variables = variableHandler.GetVariables(type);
                    string[] varNames = variables.Select(x => x.name).Prepend("未选择").ToArray();
                    int nameIndex = System.Array.IndexOf(varNames, variable.name);
                    if (nameIndex < 0) nameIndex = 0;
                    nameIndex = EditorGUI.Popup(valueRect, displayName, nameIndex, varNames);
                    string nameStr = string.Empty;
                    if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
                    if (!nodeEditor.node.IsInstance)
                    {
                        name.stringValue = nameStr;
                    }
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
                        ZetanEditorUtility.SetValue(property, val);
                    }
                }
            }
            else EditorGUILayout.PropertyField(property, true);
        }
    }
}