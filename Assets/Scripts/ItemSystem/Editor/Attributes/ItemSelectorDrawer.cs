using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Item
{
    [CustomPropertyDrawer(typeof(Item))]
    public class ItemSelectorDrawer : PropertyDrawer
    {
        private IEnumerable<Item> items;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.FieldType == typeof(Item))
            {
                items ??= Item.Editor.GetItems();
                if (fieldInfo.GetCustomAttribute<ItemFilterAttribute>() is ItemFilterAttribute filter)
                    items = items.Where(x => filter.DoFilter(x));
                Draw(position, property, label, items);
            }
            else EditorGUI.PropertyField(position, property, label);
        }

        public static void Draw(Rect rect, SerializedProperty property, GUIContent label, IEnumerable<Item> items)
        {
            bool emptyLable = string.IsNullOrEmpty(label.text);
            float labelWidth = emptyLable ? 0 : EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(labelRect, label, property);
            EditorGUI.LabelField(labelRect, label);
            EditorGUI.EndProperty();
            var buttonRect = new Rect(rect.x + (emptyLable ? 0 : labelWidth + 2), rect.y, rect.width - labelWidth - (emptyLable ? 0 : 2), EditorGUIUtility.singleLineHeight);
            label = EditorGUI.BeginProperty(buttonRect, label, property);
            if (property.objectReferenceValue) EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
            var item = property.objectReferenceValue as Item;
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(item ? item.Name : $"{L10n.Tr("None")} ({typeof(Item).Name})", tooltip(property.objectReferenceValue as Item)) { image = item ? (item.Icon ? item.Icon.texture : null) : null }, FocusType.Keyboard))
            {
                var dropdown = new AdvancedDropdown<Item>(items, selectCallback: i =>
                                              {
                                                  property.objectReferenceValue = i;
                                                  property.serializedObject.ApplyModifiedProperties();
                                              },
                                              nameGetter: i => i.Name, title: label.text,
                                              groupGetter: i => i.Type.Name, iconGetter: i => i.Icon ? i.Icon.texture : null, tooltipGetter: tooltip,
                                              addCallbacks: (typeof(Item).Name, addCallback));
                dropdown.displayNone = true;
                dropdown.Show(buttonRect);

                void addCallback()
                {
                    property.objectReferenceValue = Editor.ItemEditor.OpenAndCreateItem();
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndProperty();
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        if (DragAndDrop.objectReferences.Length == 1 && items.Contains(DragAndDrop.objectReferences[0] as Item))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            Event.current.Use();
                        }
                        break;
                    case EventType.DragPerform:
                        if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is Item i && items.Contains(i))
                        {
                            DragAndDrop.AcceptDrag();
                            property.objectReferenceValue = i;
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        break;
                    case EventType.DragExited:
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                        break;
                }
            }

            static string tooltip(Item item)
            {
                if (!item) return null;
                return $"[ID] {item.ID}\n[{L10n.Tr("Name")}] {item.Name}\n[{L10n.Tr("Description")}] {item.Description}";
            }

            static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
            {
                if (property.objectReferenceValue is not Item)
                    return;

                menu.AddItem(EditorGUIUtility.TrTextContent("Location"), false, () =>
                {
                    EditorGUIUtility.PingObject(property.objectReferenceValue);
                });
                menu.AddItem(EditorGUIUtility.TrTextContent("Select"), false, () =>
                {
                    EditorGUIUtility.PingObject(property.objectReferenceValue);
                    Selection.activeObject = property.objectReferenceValue;
                });
                menu.AddItem(EditorGUIUtility.TrTextContent("Edit"), false, () =>
                {
                    Editor.ItemEditor.CreateWindow(property.objectReferenceValue as Item);
                });
            }
        }
    }
}