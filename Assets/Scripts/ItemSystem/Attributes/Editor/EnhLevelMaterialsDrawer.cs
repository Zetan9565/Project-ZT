using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(EnhLevelMaterials))]
    public class EnhLevelMaterialsDrawer : PropertyDrawer
    {
        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("materials"), GUIContent.none);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("materials"), new GUIContent(Tr("[{0}级] 可选材料组", property.GetArrayIndex() + 1), Tr("强化时可从这些材料组中任选一组")));
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
        private string Tr(string text, params object[] args)
        {
            return L.Tr(settings.language, text, args);
        }
    }
}