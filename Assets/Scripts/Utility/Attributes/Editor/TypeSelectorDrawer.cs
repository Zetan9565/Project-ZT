using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Editor
{
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    public class TypeSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference && typeof(TypeReference).IsAssignableFrom(fieldInfo.FieldType))
            {
                if (property.managedReferenceValue is not TypeReference value)
                {
                    value = new TypeReference();
                    property.managedReferenceValue = value;
                }
                drawDropdown(value.typeName, onTypeSelect);

                void onTypeSelect(Type type)
                {
                    value.typeName = $"{(string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace}.")}{type.Name}";
                }
            }
            else if (property.propertyType == SerializedPropertyType.Generic && Utility.Editor.TryGetValue(property, out var value, out var field)
                && typeof(TypeReference).IsAssignableFrom(field.FieldType))
            {
                if (value == null)
                {
                    value = new TypeReference();
                    Utility.Editor.TrySetValue(property, value);
                }
                drawDropdown((value as TypeReference).typeName, onTypeSelect);

                void onTypeSelect(Type type)
                {
                    (value as TypeReference).typeName = $"{(string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace}.")}{type.Name}";
                }
            }
            else if (property.propertyType == SerializedPropertyType.String)
            {
                drawDropdown(property.stringValue, onTypeSelect);

                void onTypeSelect(Type type)
                {
                    property.stringValue = $"{(string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace}.")}{type.Name}";
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else EditorGUI.PropertyField(position, property, label);

            void drawDropdown(string name, Action<Type> onTypeSelect)
            {
                TypeSelectorAttribute dropDown = (TypeSelectorAttribute)attribute;
                Type baseType = dropDown.baseType;
                Assembly assembly = property.serializedObject.targetObject.GetType().Assembly;
                IEnumerable<Type> types = assembly.GetTypes();
                if (!dropDown.includeAbstract) types = types.Where(x => !x.IsAbstract);
                if (baseType != null) types = types.Where(type => baseType.IsAssignableFrom(type));
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);
                var rect = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, position.height);
                if (GUI.Button(rect, string.IsNullOrEmpty(name) ? "未选择" : name, EditorStyles.popup))
                {
                    var dropdown = new AdvancedDropdown<Type>(types, onTypeSelect, t => t.Name, dropDown.groupByNamespace ? groupGetter : null, title: baseType?.Name ?? "类型");
                    dropdown.Show(rect);
                }
                EditorGUI.EndProperty();

                static string groupGetter(Type type)
                {
                    return string.IsNullOrEmpty(type.Namespace) ? string.Empty : $"{type.Namespace.Replace('.', '/')}";
                }
            }
        }
    }
}