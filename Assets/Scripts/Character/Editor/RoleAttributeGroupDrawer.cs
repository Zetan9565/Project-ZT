using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

//[CustomPropertyDrawer(typeof(RoleAttributeGroup))]
//public class RoleAttributeGroupDrawer : PropertyDrawer
//{
//    public ReorderableList List;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        float lineHeight = EditorGUIUtility.singleLineHeight;
//        float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//        SerializedProperty attrs = property.FindPropertyRelative("attributes");
//        if (attrs != null)
//        {
//            if (property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(new Rect(position.x, position.y, position.width, lineHeight), property.isExpanded, label.text))
//            {
//                List = new ReorderableList(property.serializedObject, attrs, true, true, true, true)
//                {
//                    drawElementCallback = (rect, index, isActive, isFocused) =>
//                    {
//                        SerializedProperty attr = attrs.GetArrayElementAtIndex(index);
//                        SerializedProperty type = attr.FindPropertyRelative("type");
//                        SerializedProperty value = attr.FindPropertyRelative(TypeToPropertyName(type.enumValueIndex));
//                        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width / 2, lineHeight), type, new GUIContent(string.Empty));
//                        EditorGUI.PropertyField(new Rect(rect.x + rect.width * 2 / 3, rect.y, rect.width / 3, lineHeight), value, new GUIContent(string.Empty));
//                    },

//                    elementHeightCallback = (index) =>
//                    {
//                        return lineHeightSpace;
//                    },

//                    drawHeaderCallback = (rect) =>
//                    {
//                        EditorGUI.LabelField(rect, "属性列表");
//                    },

//                    drawNoneElementCallback = (rect) =>
//                    {
//                        EditorGUI.LabelField(rect, "空列表");
//                    }
//                };
//                List.DoList(new Rect(position.x, position.y + lineHeightSpace, position.width, position.height));
//            }
//            EditorGUI.EndFoldoutHeaderGroup();
//        }
//        else EditorGUI.PropertyField(position, property, label);
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        SerializedProperty attrs = property.FindPropertyRelative("attributes");
//        if (attrs != null)
//            if (property.isExpanded)
//                return (List?.GetHeight() ?? 0) + base.GetPropertyHeight(property, label);
//            else return base.GetPropertyHeight(property, label);
//        else return EditorGUI.GetPropertyHeight(property, label);
//    }

//    private string TypeToPropertyName(int type)
//    {
//        switch ((RoleAttributeType)type)
//        {
//            case RoleAttributeType.HP:
//            case RoleAttributeType.MP:
//            case RoleAttributeType.SP:
//            case RoleAttributeType.CutATK:
//            case RoleAttributeType.PunATK:
//            case RoleAttributeType.BluATK:
//            case RoleAttributeType.DEF:
//                return "intValue";
//            case RoleAttributeType.Hit:
//            case RoleAttributeType.Crit:
//            case RoleAttributeType.ATKSpeed:
//            case RoleAttributeType.MoveSpeed:
//                return "floatValue";
//            case RoleAttributeType.TestBool:
//                return "boolValue";
//            default:
//                return "intValue";
//        }
//    }
//}