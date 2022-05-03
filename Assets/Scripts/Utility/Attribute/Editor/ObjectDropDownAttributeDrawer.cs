using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectDropDownAttribute))]
public class ObjectDropDownAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ObjectDropDownAttribute attribute = this.attribute as ObjectDropDownAttribute;
        Handling(attribute.type, attribute.fieldAsName, attribute.resPath, attribute.nameNull, out var objects, out var objectNames);
        int index = Array.IndexOf(objects, property.objectReferenceValue) + 1;
        index = index < 0 ? 0 : index;
        label = EditorGUI.BeginProperty(position, label, property);
        index = EditorGUI.Popup(new Rect(position.x, position.y, position.width - 25, position.height), label, index, objectNames);
        if (index < 1 || index > objects.Length) property.objectReferenceValue = null;
        else property.objectReferenceValue = objects[index - 1];
        EditorGUI.PropertyField(new Rect(position.x + position.width - 23, position.y, 23, position.height), property, new GUIContent(string.Empty));
        EditorGUI.EndProperty();
    }

    private void Handling(Type type, string fieldAsName, string resPath, string nameNull,
                          out UnityEngine.Object[] objects, out GUIContent[] objectNamesArray)
    {
        objects = Resources.LoadAll(string.IsNullOrEmpty(resPath) ? string.Empty : resPath, type);
        List<GUIContent> objectNames = new List<GUIContent>() { new GUIContent(nameNull) };
        HashSet<string> existNames = new HashSet<string>();
        for (int i = 0; i < objects.Length; i++)
        {
            var obj = objects[i];
            string name = obj.name;
            if (!string.IsNullOrEmpty(fieldAsName))
            {
                if (obj.GetType().GetField(fieldAsName, ZetanUtility.CommonBindingFlags) is FieldInfo field) name = field.GetValue(obj).ToString();
                else if (obj.GetType().GetProperty(fieldAsName, ZetanUtility.CommonBindingFlags) is PropertyInfo property) name = property.GetValue(obj).ToString();
            }
            int num = 1;
            name = string.IsNullOrEmpty(name) ? obj.name : name;
            string uniueName = name;
            while (existNames.Contains(uniueName))
            {
                uniueName = name + $"(ÖØ¸´ {num})";
                num++;
            }
            existNames.Add(uniueName);
            objectNames.Add(new GUIContent(uniueName));
        }
        objectNamesArray = objectNames.ToArray();
    }
}