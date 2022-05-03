using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZetanStudio.Item
{
    [CustomPropertyDrawer(typeof(ItemSelectorAttribute))]
    public class ItemSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typeof(ItemBase).IsAssignableFrom(fieldInfo.FieldType))
            {
                bool emptyLable = string.IsNullOrEmpty(label.text);
                float labelWidth = emptyLable ? 0 : EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(new Rect(position.x, position.y, labelWidth, EditorGUIUtility.singleLineHeight), label);
                var buttonRect = new Rect(position.x + (emptyLable ? 0 : labelWidth + 2), position.y, position.width - labelWidth - (emptyLable ? 25 : 27), EditorGUIUtility.singleLineHeight);
                var item = property.objectReferenceValue as ItemBase;
                if (GUI.Button(buttonRect, new GUIContent(item ? item.Name : "未选择") { image = item ? (item.Icon ? item.Icon.texture : null) : null }, EditorStyles.popup))
                    ItemSearchProvider.OpenWindow(new SearchWindowContext(GUIUtility.GUIToScreenRect(buttonRect).position),
                                                  ZetanUtility.Editor.LoadAssets<ItemBase>(),
                                                  i => { property.objectReferenceValue = i; property.serializedObject.ApplyModifiedProperties(); });
                EditorGUI.PropertyField(new Rect(position.x + position.width - 23f, position.y, 23f, EditorGUIUtility.singleLineHeight), property, new GUIContent(string.Empty));
            }
            else EditorGUI.PropertyField(position, property, label);
        }
    }
}