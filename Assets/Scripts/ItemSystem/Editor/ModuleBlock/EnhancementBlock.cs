using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomMuduleDrawer(typeof(EnhancementModule))]
    public class EnhancementBlock : ModuleBlock
    {
        private readonly SerializedProperty enhancementInfo;
        private readonly SerializedProperty enhancement;
        private readonly SerializedProperty method;
        private readonly SerializedProperty costs;
        private readonly SerializedProperty materials;
        private readonly SerializedProperty expTypes;
        private readonly SerializedProperty experienceTable;
        private readonly SerializedProperty failure;

        public EnhancementBlock(SerializedProperty property, ItemModule module) : base(property, module)
        {
            enhancementInfo = property.FindPropertyRelative("enhancementInfo");
            enhancement = property.FindPropertyRelative("enhancement");
            method = property.FindAutoPropertyRelative("Method");
            costs = property.FindPropertyRelative("costs");
            materials = property.FindPropertyRelative("materials");
            expTypes = property.FindPropertyRelative("expTypes");
            experienceTable = property.FindPropertyRelative("experienceTable");
            failure = property.FindAutoPropertyRelative("Failure");
        }

        protected override void OnInspectorGUI()
        {
            if (enhancement.FindPropertyRelative("increments").arraySize < 1) EditorGUILayout.PropertyField(enhancementInfo, new GUIContent(Tr("公共强化信息")));
            if (!enhancementInfo.objectReferenceValue) EditorGUILayout.PropertyField(enhancement, new GUIContent(Tr("各级强化信息")));
            EditorGUILayout.PropertyField(method, new GUIContent(Tr("强化方式")));
            if (method.enumValueIndex == (int)EnhanceMethod.SingleItem)
                EditorGUILayout.PropertyField(costs, new GUIContent(Tr("各级强化材料")));
            else if (method.enumValueIndex == (int)EnhanceMethod.Materials)
                EditorGUILayout.PropertyField(materials, new GUIContent(Tr("各级强化材料")));
            else
            {
                EditorGUILayout.PropertyField(expTypes, new GUIContent(Tr("可用经验类型")));
                EditorGUILayout.PropertyField(experienceTable, new GUIContent(Tr("各级强化经验")));
            }
            string[] names;
            if (property.TryGetOwnerValue(out var value) && value is IList<ItemModule> modules && modules.Any(x => x is AffixEnhancementModule ae && ae.IsValid))
            {
                names = new string[3];
                names[0] = Tr(ZetanUtility.GetInspectorName(EnhanceFailure.None));
                names[1] = Tr(ZetanUtility.GetInspectorName(EnhanceFailure.Broken));
                names[2] = Tr(ZetanUtility.GetInspectorName(EnhanceFailure.Dsiappear));
            }
            else names = L.TrM(settings.language, ZetanUtility.GetInspectorNames(typeof(EnhanceFailure)));
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, GUIContent.none, failure);
            failure.enumValueIndex = EditorGUI.IntPopup(rect, Tr("失败方式"), failure.enumValueIndex, names, new int[] { (int)EnhanceFailure.None, (int)EnhanceFailure.Broken, (int)EnhanceFailure.Dsiappear });
            EditorGUI.EndProperty();
        }
    }
}