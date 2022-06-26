using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio
{
    [CustomPropertyDrawer(typeof(Dialogue))]
    public class DialogueDrawer : PropertyDrawer
    {
        private IEnumerable<Dialogue> dialogues;
        private IEnumerable<TalkerInformation> talkers;
        private IEnumerable<ItemSystem.Item> items;
        private IEnumerable<EnemyInformation> enemies;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            dialogues ??= ZetanUtility.Editor.LoadAssets<Dialogue>();
            talkers ??= ZetanUtility.Editor.LoadAssets<TalkerInformation>();
            items ??= ItemSystem.Item.GetItems();
            enemies ??= ZetanUtility.Editor.LoadAssets<EnemyInformation>();
            Draw(position, property, label, dialogues, talkers, items, enemies);
        }

        public static void Draw(Rect rect, SerializedProperty property, GUIContent label, IEnumerable<Dialogue> dialogues, params IEnumerable<ScriptableObject>[] caches)
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
            var item = property.objectReferenceValue as Dialogue;
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(item.name, tooltip(property.objectReferenceValue as Dialogue)) { image = ZetanUtility.Editor.GetIconForObject(property.objectReferenceValue) }, FocusType.Keyboard))
            {
                var dropdown = new AdvancedDropdown<Dialogue>(dialogues, selectCallback: i =>
                                                {
                                                    property.objectReferenceValue = i;
                                                    property.serializedObject.ApplyModifiedProperties();
                                                },
                                              title: label.text, nameGetter: d => d.name,
                                              tooltipGetter: tooltip,
                                              addCallbacks: (typeof(Dialogue).Name, addCallback));
                dropdown.displayNone = true;
                dropdown.Show(buttonRect);

                void addCallback()
                {
                    AddCallback(property);
                }
                static void AddCallback(SerializedProperty property)
                {
                    var obj = ZetanUtility.Editor.SaveFilePanel(ScriptableObject.CreateInstance<Dialogue>, ping: true);
                    if (obj)
                    {
                        property.objectReferenceValue = obj;
                        property.serializedObject.ApplyModifiedProperties();
                        EditorUtility.OpenPropertyEditor(obj);
                    }
                }
            }
            EditorGUI.EndProperty();
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        if (DragAndDrop.objectReferences.Length == 1 && dialogues.Contains(DragAndDrop.objectReferences[0] as Dialogue))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            Event.current.Use();
                        }
                        break;
                    case EventType.DragPerform:
                        if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is Dialogue i && dialogues.Contains(i))
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

            string tooltip(Dialogue dialogue)
            {
                return Dialogue.PreviewDialogue(dialogue, caches);
            }
            static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
            {
                if (property.objectReferenceValue is null)
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
                menu.AddItem(EditorGUIUtility.TrTextContent("Properties..."), false, () =>
                {
                    EditorUtility.OpenPropertyEditor(property.objectReferenceValue);
                });
            }
        }
    }
}
