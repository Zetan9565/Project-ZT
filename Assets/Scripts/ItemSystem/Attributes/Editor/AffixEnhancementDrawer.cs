using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using ItemSystem.Module;

    [CustomPropertyDrawer(typeof(AffixEnhancement))]
    public class AffixEnhancementDrawer : PropertyDrawer
    {
        private readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private readonly float lineHeightSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        private readonly ItemEditorSettings settings;

        public AffixEnhancementDrawer()
        {
            settings = ItemEditorSettings.GetOrCreate();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(new Rect(position.x, position.y, position.width, lineHeight), GUIContent.none, property);
            EditorGUI.EndProperty();
            if (property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, lineHeight), property.isExpanded, label, true))
            {
                EditorGUI.indentLevel++;
                int lineCount = 1;
                SerializedProperty affixCountRange = property.FindPropertyRelative("affixCountRange");
                SerializedProperty affixCountDistrib = property.FindPropertyRelative("affixCountDistrib");
                SerializedProperty affixIndexDistrib = property.FindPropertyRelative("affixIndexDistrib");
                SerializedProperty affixes = property.FindPropertyRelative("affixes");
                float minValue = Mathf.Max(0, affixCountRange.vector2IntValue.x);
                float maxValue = Mathf.Min(affixCountRange.vector2IntValue.y, affixes.arraySize);
                EditorGUI.BeginProperty(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), GUIContent.none, affixCountRange);
                Utility.Editor.MinMaxSlider(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight), new GUIContent(Tr("词缀数量范围")),
                                                 ref minValue, ref maxValue, 0, affixes.arraySize);
                EditorGUI.EndProperty();
                lineCount++;
                affixCountRange.vector2IntValue = new Vector2Int(Mathf.FloorToInt(minValue), Mathf.FloorToInt(maxValue));
                if (affixCountRange.vector2IntValue.x != affixCountRange.vector2IntValue.y)
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight),
                    affixCountDistrib, new GUIContent(Tr("词缀数量分布"), Tr("按此分布曲线选择词缀数量\n1.纵轴表示概率的标准化值(0~1)\n2.横轴表示数量区间的标准化值(0~1)")));
                    lineCount++;
                }
                if (affixes.arraySize > 1)
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight),
                    affixIndexDistrib, new GUIContent(Tr("词缀分布"), Tr("按此分布曲线选择词缀\n1.纵轴表示概率的标准化值(0~1)\n2.横轴表示下标区间的标准化值(0~1)")));
                    lineCount++;
                }
                EditorGUI.PropertyField(new Rect(position.x, position.y + lineHeightSpace * lineCount, position.width, lineHeight),
                    affixes, new GUIContent(Tr("词缀提升列表")));
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;
            float listHeight = 0;
            if (property.isExpanded)
            {
                lineCount += 1;
                SerializedProperty affixCountRange = property.FindPropertyRelative("affixCountRange");
                if (affixCountRange.vector2IntValue.x != affixCountRange.vector2IntValue.y)
                    lineCount++;
                SerializedProperty affixes = property.FindPropertyRelative("affixes");
                if (affixes.arraySize > 1)
                    lineCount++;
                listHeight = EditorGUI.GetPropertyHeight(affixes, true);
            }
            return lineHeightSpace * lineCount + listHeight;
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
    }
}