using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZetanStudio.ConditionSystem.Editor
{
    [CustomPropertyDrawer(typeof(ConditionGroup))]
    public class ConditionGroupDrawer : PropertyDrawer
    {
        private ReorderableList list;
        private SerializedProperty conditions;
        private readonly float lineHeight;
        private readonly float lineHeightSpace;

        public ConditionGroupDrawer()
        {
            lineHeight = EditorGUIUtility.singleLineHeight;
            lineHeightSpace = lineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            conditions = property.FindPropertyRelative("conditions");
            list = new ReorderableList(property.serializedObject, conditions, true, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, condition, new GUIContent($"[{index}] {(condition.managedReferenceValue != null ? Condition.GetName(condition.managedReferenceValue.GetType()) : typeof(Condition).Name)}"), true);
                },

                elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(conditions.GetArrayElementAtIndex(index), true),

                onCanRemoveCallback = (list) => list.IsSelected(list.index),

                onAddDropdownCallback = (rect, list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (var type in TypeCache.GetTypesDerivedFrom<Condition>())
                    {
                        if (!type.IsAbstract)
                        {
                            string group = Condition.GetGroup(type);
                            string name = Condition.GetName(type);
                            if (!string.IsNullOrEmpty(group))
                                group = group.EndsWith('/') ? group : group + '/';
                            if (string.IsNullOrEmpty(name)) name = type.Name;
                            menu.AddItem(new GUIContent($"{group}{name}"), false, insert, type);
                        }
                    }
                    menu.ShowAsContext();

                    void insert(object type)
                    {
                        conditions.arraySize++;
                        var prop = conditions.GetArrayElementAtIndex(conditions.arraySize - 1);
                        prop.managedReferenceValue = Activator.CreateInstance(type as Type);
                        conditions.serializedObject.ApplyModifiedProperties();
                        this.list.Select(conditions.arraySize - 1);
                    }
                },

                drawHeaderCallback = (rect) =>
                {
                    int notCmpltCount = 0;
                    for (int i = 0; i < conditions.arraySize; i++)
                    {
                        if (conditions.GetArrayElementAtIndex(i).managedReferenceValue is not Condition condition || !condition.IsValid) notCmpltCount++;
                    }
                    EditorGUI.LabelField(rect, EDL.Tr("条件集\t数量: ") + conditions.arraySize + (notCmpltCount > 0 ? EDL.Tr("\t未补全: ") + notCmpltCount : string.Empty));
                },
            };
            if (property.isExpanded) return lineHeightSpace + list.GetHeight() + (conditions.arraySize < 1 ? 0 : EditorGUIUtility.singleLineHeight);
            else return lineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, lineHeight), property.isExpanded, label, true))
            {
                int lineCount = 1;
                if (conditions.arraySize > 0)
                {
                    EditorGUI.PropertyField(new Rect(position.x, position.y + lineCount * lineHeightSpace, position.width, lineHeight),
                        property.FindPropertyRelative("relational"), new GUIContent(EDL.Tr("关系")));
                    lineCount++;
                }
                list?.DoList(new Rect(position.x, position.y + lineCount * lineHeightSpace, position.width, list.GetHeight()));
            }
        }
    }
}
