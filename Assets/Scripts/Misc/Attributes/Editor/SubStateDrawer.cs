using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubStateAttribute))]
public class SubStateDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SubStateAttribute attr = attribute as SubStateAttribute;
        SerializedProperty main = property.serializedObject.FindProperty(attr.mainField);
        string[] paths = property.propertyPath.Split('.');
        object value = property.serializedObject.targetObject;
        for (int i = 0; i < paths.Length - 1; i++)
        {
            if (i + 1 < paths.Length - 1 && i + 2 < paths.Length)
            {
                if (paths[i + 1] == "Array" && paths[i + 2].StartsWith("data"))
                {
                    if (int.TryParse(paths[i + 2].Replace("data[", "").Replace("]", ""), out var index))
                    {
                        if (GetValue(paths[i], value) is IList list)
                        {
                            value = list[index];
                            i += 2;
                        }
                    }
                }
            }
            else
            {
                value = GetValue(paths[i], value);
            }

            static object GetValue(string path, object target)
            {
                return target.GetType().GetField(path, ZetanUtility.CommonBindingFlags).GetValue(target);
            }
        }
        value = value.GetType().GetField(attr.mainField, ZetanUtility.CommonBindingFlags).GetValue(value);
        if (value != null)
        {
            List<GUIContent> names = new List<GUIContent>();
            switch ((CharacterStates)value)
            {
                case CharacterStates.Normal:
                    foreach (var name in ZetanUtility.GetInspectorNames(typeof(CharacterNormalStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Abnormal:
                    foreach (var name in ZetanUtility.GetInspectorNames(typeof(CharacterAbnormalStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Gather:
                    foreach (var name in ZetanUtility.GetInspectorNames(typeof(CharacterGatherStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Attack:
                    foreach (var name in ZetanUtility.GetInspectorNames(typeof(CharacterAttackStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Busy:
                    foreach (var name in ZetanUtility.GetInspectorNames(typeof(CharacterBusyStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                default:
                    EditorGUI.PropertyField(position, property, label);
                    return;
            }
            property.intValue = EditorGUI.Popup(position, label, property.intValue, names.ToArray());
        }
        else EditorGUI.PropertyField(position, property, label);
    }
}
