using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Module
{
    using Character;
    using Extension.Editor;

    [CustomPropertyDrawer(typeof(RandomAffix))]
    public class RandomAffixDrawer : PropertyDrawer
    {
        readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        readonly ItemEditorSettings settings;

        public RandomAffixDrawer()
        {
            settings = ItemEditorSettings.GetOrCreate();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.TryGetValue(out var value) && value is RandomAffix attr)
                switch (attr.ValueType)
                {
                    case RoleValueType.Integer:
                        var intRange = property.FindPropertyRelative("intRange");
                        return intRange.vector2IntValue.x == intRange.vector2IntValue.y ? lineHeightSpace : lineHeight + lineHeightSpace;
                    case RoleValueType.Float:
                        var floatRange = property.FindPropertyRelative("floatRange");
                        return floatRange.vector2Value.x == floatRange.vector2Value.y ? lineHeightSpace : lineHeight + lineHeightSpace;
                    case RoleValueType.Boolean:
                        return lineHeightSpace;
                }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.TryGetValue(out var value) && value is RandomAffix attr)
            {
                SerializedProperty type = property.FindPropertyRelative("type");
                Rect left = new Rect(position.x, position.y, position.width / 2 - 1, lineHeight);
                property.TryGetOwnerValue(out var owner);
                if (owner is IEnumerable<RandomAffix> list)
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
                Rect right = new Rect(position.x + position.width / 2 + 2, position.y, position.width / 2 - 1, lineHeight);
                switch (attr.ValueType)
                {
                    case RoleValueType.Integer:
                        var intRange = property.FindPropertyRelative("intRange");
                        var min = intRange.FindPropertyRelative("x");
                        var max = intRange.FindPropertyRelative("y");
                        EditorGUI.BeginProperty(right, GUIContent.none, intRange);
                        float toWidth = GUI.skin.label.CalcSize(new GUIContent(Tr("到"))).x;
                        float halfTo = toWidth / 2;
                        float halfWidth = right.width / 2;
                        float valueWidth = halfWidth - halfTo - 1;
                        Rect minRect = new Rect(right.x, right.y, valueWidth, right.height);
                        min.intValue = EditorGUI.IntField(minRect, min.intValue);
                        Rect toRect = new Rect(right.x + minRect.width + 2, right.y, toWidth, right.height);
                        EditorGUI.LabelField(toRect, Tr("到"));
                        Rect maxRect = new Rect(right.x + minRect.width + toRect.width + 2, right.y, valueWidth, right.height);
                        max.intValue = EditorGUI.IntField(maxRect, max.intValue);
                        EditorGUI.EndProperty();
                        if (min.intValue != max.intValue)
                            EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace, position.width, lineHeight),
                                                property.FindPropertyRelative("valueDistrib"),
                                                new GUIContent(Tr("数值分布图"), Tr("按此分布曲线选择词缀数值\n1.纵轴表示概率的标准化值(0~1)\n2.横轴表示数值区间的标准化值(0~1)")));
                        break;
                    case RoleValueType.Float:
                        var floatRange = property.FindPropertyRelative("floatRange");
                        min = floatRange.FindPropertyRelative("x");
                        max = floatRange.FindPropertyRelative("y");
                        EditorGUI.BeginProperty(right, GUIContent.none, floatRange);
                        toWidth = GUI.skin.label.CalcSize(new GUIContent(Tr("到"))).x;
                        halfTo = toWidth / 2;
                        halfWidth = right.width / 2;
                        valueWidth = halfWidth - halfTo - 1;
                        minRect = new Rect(right.x, right.y, valueWidth, right.height);
                        min.floatValue = EditorGUI.FloatField(minRect, min.floatValue);
                        toRect = new Rect(right.x + minRect.width + 2, right.y, toWidth, right.height);
                        EditorGUI.LabelField(toRect, Tr("到"));
                        maxRect = new Rect(right.x + minRect.width + toRect.width + 2, right.y, valueWidth, right.height);
                        max.floatValue = EditorGUI.FloatField(maxRect, max.floatValue);
                        EditorGUI.EndProperty();
                        if (min.floatValue != max.floatValue)
                            EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace, position.width, lineHeight),
                                                    property.FindPropertyRelative("valueDistrib"),
                                                    new GUIContent(Tr("数值分布图"), Tr("按此分布曲线选择词缀数值\n1.纵轴表示概率的标准化值(0~1)\n2.横轴表示数值区间的标准化值(0~1)")));
                        break;
                    case RoleValueType.Boolean:
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        var pL = new GUIContent(Tr("真值概率"));
                        EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(pL).x;
                        EditorGUI.PropertyField(new Rect(position.x + position.width / 2 + 1, position.y, position.width / 2 - 1, lineHeight),
                                                property.FindPropertyRelative("trueProbility"), pL);
                        EditorGUIUtility.labelWidth = oldLabelWidth;
                        break;
                }
            }
            else EditorGUI.PropertyField(position, property, true);
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
    }
}