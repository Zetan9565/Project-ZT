using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubStateAttribute))]
public class SubStateAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SubStateAttribute attr = attribute as SubStateAttribute;
        SerializedProperty main = property.serializedObject.FindProperty(attr.mainField);
        if (main != null)
        {
            List<GUIContent> names = new List<GUIContent>();
            switch ((CharacterStates)main.enumValueIndex)
            {
                case CharacterStates.Normal:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterNormalStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Abnormal:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterAbnormalStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Gather:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterGatherStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Attack:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterAttackStates)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterStates.Busy:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterBusyStates)))
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
    }
}
