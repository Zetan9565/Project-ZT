using System.Linq;
using UnityEditor;
using UnityEngine;
using ZetanExtends.Editor;
using ZetanStudio.Item.Module;

namespace ZetanStudio.Item.Editor
{
    [CustomMuduleDrawer(typeof(CommonModule), true)]
    public class CommonBlock : ModuleBlock
    {
        private readonly ItemNew item;
        private readonly ItemTemplate template;
        private readonly SerializedProperty Name;
        private readonly SerializedProperty Parameter;
        private bool errorBef = false;

        public CommonBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            text = module.GetName();
            Name = property.FindAutoPropertyRelative("Name");
            Parameter = property.FindAutoPropertyRelative("Parameter");
            item = property.serializedObject.targetObject as ItemNew;
            template = property.serializedObject.targetObject as ItemTemplate;
            errorBef = !(!module.IsValid || Duplicate());
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(Name, new GUIContent($"名称{(string.IsNullOrEmpty(Name.stringValue) ? "(名称为空)" : (Duplicate() ? "(名称重复)" : string.Empty))}"));
            EditorGUILayout.PropertyField(Parameter);
            CheckError();
        }
        private bool Duplicate()
        {
            if (userData is ItemModule)
                return item && item.Modules.Any(check) || template && template.Modules.Any(check);
            return false;

            bool check(ItemModule other) => CommonModule.Duplicate(userData as CommonModule, other as CommonModule);
        }
        protected override void CheckError()
        {
            if (userData is ItemModule module)
            {
                if (errorBef != (!module.IsValid || Duplicate()))
                {
                    text = module.GetName();
                    if (!errorBef) text += "(存在错误)";
                    errorBef = !errorBef;
                }
            }
        }
    }
}