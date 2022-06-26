using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.ItemSystem.Editor
{
    using Extension.Editor;
    using Module;

    [CustomPropertyDrawer(typeof(MaterialInfo))]
    public class MaterialInfoDrawer : PropertyDrawer
    {
        private readonly float lineHeight = EditorGUIUtility.singleLineHeight;

        private readonly ItemEditorSettings settings = ItemEditorSettings.GetOrCreate();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType)) return lineHeight;
            else return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private IEnumerable<Item> GetItems()
        {
            if (fieldInfo.GetCustomAttribute<ItemFilterAttribute>() is ItemFilterAttribute itemFilter) return Item.Editor.GetItemsWhere(itemFilter.DoFilter);
            else return Item.Editor.GetItemsWhere(x => x.Modules.Any(x => typeof(MaterialModule).IsAssignableFrom(x.GetType())));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
            {
                int index = property.GetArrayIndex();
                SerializedProperty matList = property.serializedObject.FindProperty(property.propertyPath[..^$".Array.data[{index}]".Length]);
                SerializedProperty materialType = property.FindPropertyRelative("materialType");
                SerializedProperty costType = property.FindPropertyRelative("costType");
                SerializedProperty item = property.FindPropertyRelative("item");
                SerializedProperty amount = property.FindPropertyRelative("amount");

                var headLabel = new GUIContent($"[{index + 1}]");
                float labelWidth = GUI.skin.label.CalcSize(headLabel).x;
                EditorGUI.LabelField(new Rect(position.x, position.y, labelWidth, lineHeight), headLabel);
                EditorGUI.PropertyField(new Rect(position.x + labelWidth + 1, position.y, position.width / 3 - labelWidth - 1, lineHeight), costType, GUIContent.none);
                Rect matRect = new Rect(position.x + position.width / 3 + 1, position.y, position.width / 3 - 1, lineHeight);
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                if (costType.enumValueIndex == (int)MaterialCostType.SameType)
                {
                    var typeBef = materialType.intValue;
                    var mL = new GUIContent(Tr("类型"));
                    EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(mL).x;
                    EditorGUI.PropertyField(matRect, materialType, mL);
                    if (typeBef != materialType.intValue)
                        for (int i = 0; i < matList.arraySize; i++)
                        {
                            if (i != index)
                            {
                                SerializedProperty element = matList.GetArrayElementAtIndex(i);
                                SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                SerializedProperty eCostType = element.FindPropertyRelative("costType");
                                SerializedProperty eItem = element.FindPropertyRelative("item");
                                if (eCostType.enumValueIndex == (int)MaterialCostType.SingleItem)
                                {
                                    if (eItem.objectReferenceValue is Item ei)
                                        if (MaterialModule.SameType(MaterialTypeEnum.Instance[materialType.intValue], ei))
                                        {
                                            EditorUtility.DisplayDialog("错误", $"与第 {i + 1} 个材料的道具类型冲突", "确定");
                                            materialType.intValue = typeBef;
                                            for (int j = 0; MaterialModule.SameType(MaterialTypeEnum.Instance[materialType.intValue], ei) && j < MaterialTypeEnum.Instance.Enum.Count; j++)
                                            {
                                                materialType.intValue = j;
                                            }
                                        }
                                }
                                else
                                {
                                    if (eMaterialType.intValue == materialType.intValue)
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料已使用该类型", "确定");
                                        materialType.intValue = typeBef;
                                        for (int j = 0; eMaterialType.intValue == materialType.intValue && j < MaterialTypeEnum.Instance.Enum.Count; j++)
                                        {
                                            materialType.intValue = j;
                                        }
                                    }
                                }
                            }
                        }
                }
                else
                {
                    var iL = new GUIContent(Tr("道具"));
                    EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(iL).x;
                    var itemBef = item.objectReferenceValue;
                    ItemSelectorDrawer.Draw(matRect, item, iL, GetItems, selected, fieldInfo.GetCustomAttribute<ItemFilterAttribute>() ?? new ItemFilterAttribute(typeof(MaterialModule)));

                    void selected(Item s)
                    {
                        item.objectReferenceValue = s;
                        if (item.objectReferenceValue is Item itemNow)
                        {
                            for (int i = 0; i < matList.arraySize; i++)
                            {
                                if (i != index)
                                {
                                    SerializedProperty element = matList.GetArrayElementAtIndex(i);
                                    SerializedProperty eMaterialType = element.FindPropertyRelative("materialType");
                                    SerializedProperty eCostType = element.FindPropertyRelative("costType");
                                    SerializedProperty eItem = element.FindPropertyRelative("item");
                                    if (eCostType.enumValueIndex == (int)MaterialCostType.SameType)
                                    {
                                        if (MaterialModule.SameType(MaterialTypeEnum.Instance[eMaterialType.intValue], itemNow))
                                        {
                                            EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料的类型 [{MaterialTypeEnum.IndexToName(eMaterialType.intValue)}] 已包括这个道具", "确定");
                                            if (MaterialModule.SameType(MaterialTypeEnum.Instance[eMaterialType.intValue], itemBef as Item))
                                                item.objectReferenceValue = null;
                                            else item.objectReferenceValue = itemBef;
                                            item.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                            return;
                                        }
                                    }
                                    else if (Equals(eItem.objectReferenceValue, itemNow))
                                    {
                                        EditorUtility.DisplayDialog("错误", $"第 {i + 1} 个材料已使用这个道具", "确定");
                                        item.objectReferenceValue = itemBef;
                                        if (Equals(eItem.objectReferenceValue, item.objectReferenceValue))
                                            item.objectReferenceValue = null;
                                        item.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                                        return;
                                    }
                                }
                            }
                        }
                        item.serializedObject.ApplyModifiedProperties();
                    }
                }
                var aL = new GUIContent(Tr("数量"));
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(aL).x;
                EditorGUI.PropertyField(new Rect(position.x + position.width * 2 / 3 + 2, position.y, position.width / 3 - 2, lineHeight),
                    amount, aL);
                if (amount.intValue < 1) amount.intValue = 1;
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
            else EditorGUI.PropertyField(position, property, label, true);
        }

        private string Tr(string text)
        {
            return L.Tr(settings.language, text);
        }
    }
}