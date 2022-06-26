using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(ItemIncrement))]
    public class ItemIncrementDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("attributes"), label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("attributes"), new GUIContent(L.Tr(settings.language, "[{0}级] 提升属性", property.GetArrayIndex() + 1)));
        }
    }
}