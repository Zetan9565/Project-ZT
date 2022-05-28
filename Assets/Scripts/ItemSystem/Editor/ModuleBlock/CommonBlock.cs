using System.Linq;
using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Editor
{
    [CustomMuduleDrawer(typeof(CommonModule), true)]
    public class CommonBlock : ModuleBlock
    {
        private readonly Item item;
        private readonly ItemTemplate template;
        private readonly SerializedProperty Name;
        private readonly SerializedProperty Parameter;

        public CommonBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            Name = property.FindAutoPropertyRelative("Name");
            Parameter = property.FindAutoPropertyRelative("Parameter");
            item = property.serializedObject.targetObject as Item;
            template = property.serializedObject.targetObject as ItemTemplate;
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(Name, new GUIContent($"名称{(string.IsNullOrEmpty(Name.stringValue) ? "(名称为空)" : (Duplicate() ? "(名称重复)" : string.Empty))}"));
            EditorGUILayout.PropertyField(Parameter);
            errorBef = !HasError();
        }
        private bool Duplicate()
        {
            if (userData is ItemModule)
                return item && item.Modules.Any(check) || template && template.Modules.Any(check);
            return false;

            bool check(ItemModule other) => CommonModule.Duplicate(userData as CommonModule, other as CommonModule);
        }
        protected override bool HasError()
        {
            return userData is ItemModule module && !module.IsValid || Duplicate();
        }
    }
}