using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Character;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomPropertyDrawer(typeof(ItemAttribute))]
    public class ItemAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.TryGetValue(out var value) && value is ItemAttribute attr)
            {
                SerializedProperty type = property.FindPropertyRelative("type");
                Rect left = new Rect(position.x, position.y, position.width / 2 - 1, EditorGUIUtility.singleLineHeight);
                property.TryGetOwnerValue(out var owner);
                if (owner is IEnumerable<ItemAttribute> list)
                {
                    List<int> indices = new List<int>();
                    List<string> names = new List<string>();
                    var _enum = ItemAttributeEnum.Instance.Enum;
                    for (int i = 0; i < _enum.Count; i++)
                    {
                        if (i != type.intValue && list.Any(x => x.Type == _enum[i]))
                            continue;
                        indices.Add(i);
                        names.Add(_enum[i].Name);
                    }
                    EditorGUI.BeginProperty(left, GUIContent.none, type);
                    type.intValue = EditorGUI.IntPopup(left, type.intValue, names.ToArray(), indices.ToArray());
                    EditorGUI.EndProperty();
                }
                else EditorGUI.PropertyField(left, type, GUIContent.none);
                Rect right = new Rect(position.x + position.width / 2 + 2, position.y, position.width / 2 - 1, EditorGUIUtility.singleLineHeight);
                switch (attr.ValueType)
                {
                    case RoleValueType.Integer:
                        var intValue = property.FindPropertyRelative("intValue");
                        EditorGUI.BeginProperty(right, GUIContent.none, intValue);
                        intValue.intValue = EditorGUI.IntField(right, intValue.intValue);
                        EditorGUI.EndProperty();
                        break;
                    case RoleValueType.Float:
                        var floatValue = property.FindPropertyRelative("floatValue");
                        EditorGUI.BeginProperty(right, GUIContent.none, floatValue);
                        floatValue.floatValue = EditorGUI.FloatField(right, floatValue.floatValue);
                        EditorGUI.EndProperty();
                        break;
                    case RoleValueType.Boolean:
                        var boolValue = property.FindPropertyRelative("boolValue");
                        EditorGUI.BeginProperty(right, GUIContent.none, boolValue);
                        boolValue.boolValue = EditorGUI.Toggle(right, boolValue.boolValue);
                        EditorGUI.EndProperty();
                        break;
                }
            }
            else EditorGUI.PropertyField(position, property, true);
        }
    }
}
