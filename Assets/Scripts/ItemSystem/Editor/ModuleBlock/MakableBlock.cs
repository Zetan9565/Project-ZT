using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using ZetanExtends.Editor;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Editor
{
    [CustomMuduleDrawer(typeof(MakableModule))]
    public class MakableBlock : ModuleBlock
    {
        SerializedProperty makingMethod;
        SerializedProperty canMakeByTry;
        SerializedProperty formulation;
        SerializedProperty yields;
        ReorderableList list;

        public MakableBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            makingMethod = property.FindPropertyRelative("makingMethod");
            canMakeByTry = property.FindPropertyRelative("canMakeByTry");
            formulation = property.FindAutoPropertyRelative("Formulation");
            yields = property.FindAutoPropertyRelative("Yields");
            list = new ReorderableList(property.serializedObject, yields)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = yields.GetArrayElementAtIndex(index);
                    SerializedProperty yield = element.FindAutoPropertyRelative("Yield");
                    SerializedProperty rate = element.FindAutoPropertyRelative("Rate");
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "产量");
                    yield.intValue = EditorGUI.IntField(new Rect(rect.x + 52, rect.y, 100, EditorGUIUtility.singleLineHeight), yield.intValue);
                    if (yield.intValue < 1) yield.intValue = 1;
                    EditorGUI.LabelField(new Rect(rect.x + 154, rect.y, 50, EditorGUIUtility.singleLineHeight), "概率");
                    rate.floatValue = EditorGUI.Slider(new Rect(rect.x + 206, rect.y, rect.width - 206, EditorGUIUtility.singleLineHeight), rate.floatValue, 0, 1);
                },
                elementHeightCallback = (index) =>
                {
                    return EditorGUIUtility.singleLineHeight;
                },
                onCanRemoveCallback = (list) =>
                {
                    return yields.arraySize > 1 && list.IsSelected(list.index);
                },
                headerHeight = 0,
            };
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(makingMethod);
            EditorGUILayout.PropertyField(canMakeByTry);
            EditorGUILayout.PropertyField(formulation);
            if (yields.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(yields.isExpanded, "产量表"))
                if (yields.isExpanded) list?.DoLayoutList();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}