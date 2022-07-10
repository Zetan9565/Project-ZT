using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(IDAttribute))]
public class IDDrawer : PropertyDrawer
{
    private bool existID;
    private IEnumerable<Object> items;
    private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
    private readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        IDAttribute attr = attribute as IDAttribute;
        if (property.propertyType == SerializedPropertyType.String)
        {
            float offset = 0;
            if (string.IsNullOrEmpty(property.stringValue))
            {
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 2 * lineHeight), $"{L10n.Tr("Empty")} ID!", MessageType.Error);
                offset = 2 * lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else if (existID)
            {
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 2 * lineHeight), $"ID {L10n.Tr("Repeat")}!", MessageType.Error);
                offset = 2 * lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            if (string.IsNullOrEmpty(property.stringValue) || existID)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y + offset, position.width - 52, lineHeight), property, label);
                if (GUI.Button(new Rect(position.x + position.width - 50, position.y + offset, 50, lineHeight), L10n.Tr("Create")))
                {
                    property.stringValue = GetAutoID(property.serializedObject.targetObject, attr);
                    property.serializedObject.ApplyModifiedProperties();
                }
                return;
            }
        }
        EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            IDAttribute attr = attribute as IDAttribute;
            items ??= ZetanUtility.Editor.LoadAssets(fieldInfo.DeclaringType, attr.path);
            if (existID = ExistID(property.serializedObject.targetObject, property.stringValue) || string.IsNullOrEmpty(property.stringValue))
                return 2 * lineHeight + lineHeightSpace;
        }
        return base.GetPropertyHeight(property, label);
    }

    private string GetAutoID(Object item, IDAttribute attribute)
    {
        int i = 1;
        string prefix = !string.IsNullOrEmpty(attribute.prefix) ? attribute.prefix : string.Join(string.Empty, fieldInfo.DeclaringType.Name.Where(char.IsUpper));
        string id = prefix + i.ToString().PadLeft(attribute.length, '0');
        while (ExistID(item, id))
        {
            id = prefix + i.ToString().PadLeft(attribute.length, '0');
            i++;
        }
        return id;
    }
    private bool ExistID(Object item, string id)
    {
        return items.Any(x => fieldInfo?.GetValue(x)?.ToString() == id && x != item);
    }
}