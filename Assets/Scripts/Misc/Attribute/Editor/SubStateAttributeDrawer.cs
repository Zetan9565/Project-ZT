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
            switch ((CharacterState)main.enumValueIndex)
            {
                case CharacterState.Normal:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterNormalState)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterState.Abnormal:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterAbnormalState)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterState.Gather:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterGatherState)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterState.Attack:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterAttackState)))
                    {
                        names.Add(new GUIContent(name));
                    }
                    break;
                case CharacterState.Busy:
                    foreach (var name in ZetanUtility.GetEnumNames(typeof(CharacterBusyState)))
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
