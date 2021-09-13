using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectDropDownAttribute))]
public class ObjectDropDownAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ObjectDropDownAttribute attribute = this.attribute as ObjectDropDownAttribute;
        Handling(attribute.type, attribute.fieldAsName, attribute.path, attribute.nameNull, out var objects, out var objectNames);
        int index = Array.FindIndex(objects, x => x == property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        index = EditorGUI.Popup(new Rect(position.x, position.y, position.width - 21, position.height), label.text, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
        property.objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x + position.width - 21, position.y, 20, position.height),
                                                              property.objectReferenceValue, attribute.type, false);
    }

    private void Handling(Type type, string fieldAsName, string path, string nameNull,
                          out UnityEngine.Object[] objects, out string[] objectNamesArray)
    {
        objects = Resources.LoadAll(string.IsNullOrEmpty(path) ? string.Empty : path, type);
        List<string> objectNames = new List<string>() { nameNull };
        foreach (var obj in objects)
        {
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                var field = obj.GetType().GetField(fieldAsName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) objectNames.Add(field.GetValue(obj).ToString());
                else objectNames.Add(obj.name);
            }
            else objectNames.Add(obj.name);
        }
        objectNamesArray = objectNames.ToArray();
    }
}