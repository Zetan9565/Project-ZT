using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Module;

    [CustomPropertyDrawer(typeof(ItemEnhancement))]
    public class ItemEnhancementDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("increments"), label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("increments"), label);
        }
    }
}