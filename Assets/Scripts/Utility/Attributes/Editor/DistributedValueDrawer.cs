using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;
using ZetanStudio.Math;

namespace ZetanStudio
{
    [CustomPropertyDrawer(typeof(DistributedValue), true)]
    public class DistributedValueDrawer : PropertyDrawer
    {
        private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var range = property.FindAutoPropertyRelative("Range");
            if (range.propertyType == SerializedPropertyType.Vector2Int)
                if (range.vector2IntValue.x != range.vector2IntValue.y) return lineHeightSpace * 2;
                else return lineHeightSpace;
            else if (range.propertyType == SerializedPropertyType.Vector2)
                if (range.vector2Value.x != range.vector2Value.y) return lineHeightSpace * 2;
                else return lineHeightSpace;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DistributedValueRangeAttribute attr = fieldInfo.GetCustomAttribute<DistributedValueRangeAttribute>();
            var range = property.FindAutoPropertyRelative("Range");
            if (range.propertyType == SerializedPropertyType.Vector2 || range.propertyType == SerializedPropertyType.Vector2Int)
            {
                var distrib = property.FindAutoPropertyRelative("Distribution");
                EditorGUI.LabelField(new Rect(position.x, position.y, position.width, lineHeight), new GUIContent(EDL.Tr("{0}范围", label.text)));
                if (range.propertyType == SerializedPropertyType.Vector2Int)
                {
                    int indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    int[] minMax = { range.vector2IntValue.x, range.vector2IntValue.y };
                    EditorGUI.MultiIntField(new Rect(position.x + EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing,
                                            position.y, position.width - EditorGUIUtility.labelWidth - 2f, lineHeight),
                                            new GUIContent[] { new GUIContent(EDL.Tr("最小")), new GUIContent(EDL.Tr("最大")) }, minMax);
                    int max = Mathf.Max(((int?)attr?.min) ?? 1, minMax[1]);
                    int min = Mathf.Min(max, Mathf.Max(((int?)attr?.max) ?? 0, minMax[0]));
                    range.vector2IntValue = new Vector2Int(min, max);
                    EditorGUI.indentLevel = indentLevel;
                    if (min != max)
                        EditorGUI.PropertyField(
                            new Rect(position.x, position.y + lineHeightSpace, position.width, lineHeight),
                            distrib, new GUIContent(EDL.Tr("{0}分布", label.text), EDL.Tr("按此分布图分配{0}。至少要有2个关键帧，且其中至少有一个关键帧的值大于0", label.text)));
                }
                else if (range.propertyType == SerializedPropertyType.Vector2)
                {
                    int indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    float[] minMax = { range.vector2Value.x, range.vector2Value.y };
                    EditorGUI.MultiFloatField(new Rect(position.x + EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing,
                                              position.y, position.width - EditorGUIUtility.labelWidth - 2f, lineHeight),
                                              new GUIContent[] { new GUIContent(EDL.Tr("最小")), new GUIContent(EDL.Tr("最大")) }, minMax);
                    float max = Mathf.Max(attr?.min ?? 1, minMax[1]);
                    float min = Mathf.Min(max, Mathf.Max(attr?.max ?? 0, minMax[0]));
                    range.vector2Value = new Vector2(min, max);
                    EditorGUI.indentLevel = indentLevel;
                    if (min != max)
                        EditorGUI.PropertyField(
                            new Rect(position.x, position.y + lineHeightSpace, position.width, lineHeight),
                            distrib, new GUIContent(EDL.Tr("{0}分布", label.text), EDL.Tr("按此分布图分配{0}。至少要有2个关键帧，且其中至少有一个关键帧的值大于0", label.text)));
                }
            }
            else EditorGUILayout.PropertyField(property, label, true);
        }
    }
}
