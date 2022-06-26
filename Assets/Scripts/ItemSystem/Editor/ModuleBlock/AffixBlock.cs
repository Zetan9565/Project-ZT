using UnityEditor;
using UnityEngine;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomMuduleDrawer(typeof(AffixModule))]
    public class AffixBlock : ModuleBlock
    {
        private readonly SerializedProperty affixInfo;
        private readonly SerializedProperty affix;
        private readonly SerializedProperty affixes;
        public AffixBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            affixInfo = property.FindPropertyRelative("affixInfo");
            affix = property.FindPropertyRelative("affix");
            affixes = affix.FindPropertyRelative("affixes");
        }

        protected override void OnInspectorGUI()
        {
            if (affixes.arraySize < 1) EditorGUILayout.PropertyField(affixInfo, new GUIContent(Tr("公共词缀")));
            if (!affixInfo.objectReferenceValue) EditorGUILayout.PropertyField(affix, new GUIContent(Tr("词缀")));
        }
    }
}
