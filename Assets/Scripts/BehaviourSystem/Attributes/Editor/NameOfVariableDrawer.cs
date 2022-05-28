using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [CustomPropertyDrawer(typeof(NameOfVariableAttribute))]
    public class NameOfVariableDrawer : EnhancedAttributeDrawer
    {
        GlobalVariables global;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if ((attribute as NameOfVariableAttribute).global && !global)
            {
                if (!(property.serializedObject.targetObject as BehaviourTree).IsInstance) global = ZetanUtility.Editor.LoadAsset<GlobalVariables>();
                else global = BehaviourManager.Instance.GlobalVariables;
            }
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String && property.serializedObject.targetObject is BehaviourTree)
                Draw(position, property, label, attribute as NameOfVariableAttribute, global);
            else base.OnGUI(position, property, label);
        }

        public static void Draw(Rect valueRect, SerializedProperty property, GUIContent label, NameOfVariableAttribute attribute, GlobalVariables global = null)
        {
            var tree = property.serializedObject.targetObject as BehaviourTree;
            if (attribute.global && !global)
                if (!tree.IsInstance) global = ZetanUtility.Editor.LoadAsset<GlobalVariables>();
                else global = BehaviourManager.Instance.GlobalVariables;
            var variables = attribute.global ? global.GetVariables(attribute.type) : tree.GetVariables(attribute.type);
            string[] varNames = variables.Select(x => x.name).Prepend(L10n.Tr("None")).ToArray();
            GUIContent[] contents = new GUIContent[varNames.Length];
            for (int i = 0; i < varNames.Length; i++)
            {
                contents[i] = new GUIContent(varNames[i]);
            }
            int nameIndex = Array.IndexOf(varNames, property.stringValue);
            if (nameIndex < 0) nameIndex = 0;
            nameIndex = EditorGUI.Popup(valueRect, label, nameIndex, contents);
            string nameStr = string.Empty;
            if (nameIndex > 0 && nameIndex <= variables.Count) nameStr = varNames[nameIndex];
            property.stringValue = nameStr;
        }
    }
}