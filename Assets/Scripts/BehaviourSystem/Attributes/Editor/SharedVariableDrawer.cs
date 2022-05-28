using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZetanStudio.BehaviourTree.Nodes;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.BehaviourTree
{
    [CustomPropertyDrawer(typeof(SharedVariable), true)]
    public class SharedVariableDrawer : PropertyDrawer
    {
        private BehaviourTree tree;
        private GlobalVariables global;

        private static readonly string[] varType = { "普通", "共享", "全局" };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (typeof(Node).IsAssignableFrom(fieldInfo.DeclaringType))
            {
                if (!tree) tree = property.serializedObject.targetObject as BehaviourTree;
                if (tree)
                {
                    if (!global)
                        if (!tree.IsInstance) global = ZetanUtility.Editor.LoadAsset<GlobalVariables>();
                        else global = BehaviourManager.Instance.GlobalVariables;
                }
                SerializedProperty isShared = property.FindPropertyRelative("isShared");
                SerializedProperty isGlobal = property.FindPropertyRelative("isGlobal");
                SerializedProperty value = property.FindPropertyRelative("value");
                int typeIndex = isGlobal.boolValue ? 2 : (isShared.boolValue ? 1 : 0);
                switch (typeIndex)
                {
                    case 1:
                    case 2:
                        return EditorGUIUtility.singleLineHeight;
                    default:
                        return EditorGUI.GetPropertyHeight(value, label, value.hasVisibleChildren);
                }
            }
            else return EditorGUI.GetPropertyHeight(property, label, true) - (property.isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("_name");
            SerializedProperty value = property.FindPropertyRelative("value");
            if (tree && typeof(Node).IsAssignableFrom(fieldInfo.DeclaringType) && property.TryGetValue(out var v) && v is SharedVariable shared)
            {
                var type = fieldInfo.FieldType;
                SerializedProperty isShared = property.FindPropertyRelative("isShared");
                SerializedProperty isGlobal = property.FindPropertyRelative("isGlobal");
                int typeIndex = isGlobal.boolValue ? 2 : (isShared.boolValue ? 1 : 0);
                var newTypeIndex = EditorGUI.Popup(new Rect(position.x + position.width - 32, position.y, 32, EditorGUIUtility.singleLineHeight), typeIndex, varType);
                if (newTypeIndex != typeIndex)
                {
                    isShared.boolValue = newTypeIndex == 1;
                    isGlobal.boolValue = newTypeIndex == 2;
                }
                switch (newTypeIndex)
                {
                    case 1:
                        Rect valueRect = new Rect(position.x, position.y, position.width - 34, EditorGUIUtility.singleLineHeight);
                        drawSharedField(valueRect, tree);
                        break;
                    case 2:
                        valueRect = new Rect(position.x, position.y, position.width - 34, EditorGUIUtility.singleLineHeight);
                        drawSharedField(valueRect, global);
                        break;
                    default:
                        valueRect = new Rect(position.x, position.y, position.width - 34, position.height);
                        if (value.propertyType == SerializedPropertyType.String)
                            if (fieldInfo.GetCustomAttribute<TagAttribute>() != null)
                            {
                                TagDrawer.Draw(valueRect, value, label);
                                break;
                            }
                            else if (fieldInfo.GetCustomAttribute<NameOfVariableAttribute>() is NameOfVariableAttribute attribute)
                            {
                                NameOfVariableDrawer.Draw(valueRect, value, label, attribute, global);
                                break;
                            }
                        EditorGUI.PropertyField(valueRect, value, label, value.hasVisibleChildren);
                        break;
                }
                void drawSharedField(Rect valueRect, ISharedVariableHandler variableHandler)
                {
                    var variables = variableHandler.GetVariables(type);
                    string[] varNames = variables.Select(x => x.name).Prepend(L10n.Tr("None")).ToArray();
                    GUIContent[] contents = new GUIContent[varNames.Length];
                    for (int i = 0; i < varNames.Length; i++)
                    {
                        contents[i] = new GUIContent(varNames[i]);
                    }
                    SharedVariable linked = type.GetField("linkedVariable", ZetanUtility.CommonBindingFlags).GetValue(shared) as SharedVariable;
                    SerializedProperty linkedVariable = property.FindPropertyRelative("linkedVariable");
                    SerializedProperty linkedSVName = property.FindPropertyRelative("linkedSVName");
                    SerializedProperty linkedGVName = property.FindPropertyRelative("linkedGVName");
                    if (linkedVariable.managedReferenceValue != null)
                    {
                        var linkedName = linkedVariable.FindPropertyRelative("_name").stringValue;
                        if (linkedVariable.FindPropertyRelative("isGlobal").boolValue) linkedGVName.stringValue = linkedName;
                        else linkedSVName.stringValue = linkedName;
                    }
                    name.stringValue = newTypeIndex switch
                    {
                        1 => linkedSVName.stringValue,
                        2 => linkedGVName.stringValue,
                        _ => null,
                    };
                    int nameIndex = Array.IndexOf(varNames, name.stringValue);
                    if (nameIndex < 0) nameIndex = 0;
                    nameIndex = EditorGUI.Popup(valueRect, new GUIContent(label) { tooltip = $"isG: {isGlobal.boolValue}, {shared.isGlobal}\nisS: {isShared.boolValue}, {shared.isShared}\nname: {name.stringValue}, {shared.name}\nlinkSV: {linkedSVName.stringValue}, {shared.linkedSVName}\nlinkGV: {linkedGVName.stringValue}, {shared.linkedGVName}" }, nameIndex, contents);
                    string nameStr = string.Empty;
                    if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
                    name.stringValue = nameStr;
                    if (!string.IsNullOrEmpty(nameStr))
                    {
                        var toLink = variableHandler.GetVariable(nameStr);
                        if (toLink != null && toLink != linked)
                        {
                            switch (newTypeIndex)
                            {
                                case 1:
                                    linkedSVName.stringValue = toLink.name;
                                    break;
                                case 2:
                                    linkedGVName.stringValue = toLink.name;
                                    break;
                            }
                            linkedVariable.managedReferenceValue = toLink;
                        }
                    }
                    else
                    {
                        switch (newTypeIndex)
                        {
                            case 1:
                                linkedSVName.stringValue = null;
                                break;
                            case 2:
                                linkedGVName.stringValue = null;
                                break;
                        }
                        linkedVariable.managedReferenceValue = null;
                    }
                }
            }
            else
            {
                if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true))
                {
                    EditorGUI.indentLevel++;
                    float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace, position.width, EditorGUIUtility.singleLineHeight), name);
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * 2, position.width, EditorGUIUtility.singleLineHeight), value);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}