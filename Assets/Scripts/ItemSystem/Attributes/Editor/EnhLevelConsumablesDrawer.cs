using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(EnhLevelConsumables))]
    public class EnhLevelConsumablesDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("materials"), label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("materials"), new GUIContent(L.Tr(settings.language, "[{0}级] 可选材料", property.GetArrayIndex() + 1)));
        }
    }
}