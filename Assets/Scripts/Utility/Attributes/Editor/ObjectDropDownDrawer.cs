using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Editor
{
    [CustomPropertyDrawer(typeof(ObjectDropDownAttribute))]
    public class ObjectDropDownDrawer : PropertyDrawer
    {
        private List<UnityEngine.Object> objects;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectDropDownAttribute attribute = this.attribute as ObjectDropDownAttribute;
            var type = attribute.type ?? fieldInfo.FieldType;
            if ((attribute.type == null || fieldInfo.FieldType.IsAssignableFrom(attribute.type)) && typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                Handling(type, attribute.memberAsName, attribute.resPath, attribute.nameNull, out var objectNames);
                int index = objects.IndexOf(property.objectReferenceValue) + 1;
                index = index < 0 ? 0 : index;
                label = EditorGUI.BeginProperty(position, label, property);
                index = EditorGUI.Popup(new Rect(position.x, position.y, position.width - 25, position.height), label, index, objectNames);
                if (index < 1 || index > objects.Count) property.objectReferenceValue = null;
                else property.objectReferenceValue = objects[index - 1];
                EditorGUI.PropertyField(new Rect(position.x + position.width - 23, position.y, 23, position.height), property, new GUIContent(string.Empty));
                EditorGUI.EndProperty();
            }
            else EditorGUI.PropertyField(position, property, label);
        }

        private void Handling(Type type, string memberAsName, string resPath, string nameNull, out GUIContent[] objectNamesArray)
        {
            objects ??= Utility.Editor.LoadAssets(type, resPath);
            List<GUIContent> objectNames = new List<GUIContent>() { new GUIContent(nameNull) };
            HashSet<string> existNames = new HashSet<string>();
            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                string name = obj.ToString();
                if (!string.IsNullOrEmpty(memberAsName))
                {
                    if (obj.GetType().GetField(memberAsName, Utility.CommonBindingFlags) is FieldInfo field) name = field.GetValue(obj).ToString();
                    else if (obj.GetType().GetProperty(memberAsName, Utility.CommonBindingFlags) is PropertyInfo property) name = property.GetValue(obj).ToString();
                    else if (obj.GetType().GetMethod(memberAsName, Utility.CommonBindingFlags) is MethodInfo method && method.ReturnType == typeof(string) && method.GetParameters().Length < 1)
                        name = method.Invoke(obj, null).ToString();
                }
                int num = 1;
                name = string.IsNullOrEmpty(name) ? obj.name : name;
                string uniueName = name;
                while (existNames.Contains(uniueName))
                {
                    uniueName = name + $"({L10n.Tr("Repeat")} {num})";
                    num++;
                }
                existNames.Add(uniueName);
                objectNames.Add(new GUIContent(uniueName));
            }
            objectNamesArray = objectNames.ToArray();
        }
    }
}