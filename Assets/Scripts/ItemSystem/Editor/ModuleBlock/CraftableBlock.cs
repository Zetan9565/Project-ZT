using UnityEditor;
using UnityEngine;
using ZetanStudio.Extension.Editor;
using ZetanStudio.ItemSystem.Module;

namespace ZetanStudio.ItemSystem.Editor
{
    [CustomMuduleDrawer(typeof(CraftableModule))]
    public class CraftableBlock : ModuleBlock
    {
        private readonly SerializedProperty craftMethod;
        private readonly SerializedProperty canMakeByTry;
        private readonly SerializedProperty formulation;
        private readonly SerializedProperty yield;
        private readonly SerializedProperty materials;

        public CraftableBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            craftMethod = property.FindPropertyRelative("craftMethod");
            canMakeByTry = property.FindPropertyRelative("canMakeByTry");
            formulation = property.FindPropertyRelative("formulation");
            materials = property.FindPropertyRelative("materials");
            yield = property.FindAutoProperty("Yield");
        }

        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(craftMethod, new GUIContent(Tr("制作方法")));
            EditorGUILayout.PropertyField(canMakeByTry, new GUIContent(Tr("可自学")));
            if (materials.arraySize < 1) EditorGUILayout.PropertyField(formulation, new GUIContent(Tr("公共配方")));
            if (!formulation.objectReferenceValue) EditorGUILayout.PropertyField(materials, new GUIContent(Tr("材料表")));
            EditorGUILayout.PropertyField(yield, new GUIContent(Tr("产量")));
        }
    }
}