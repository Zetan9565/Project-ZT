using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanExtends.Editor;

namespace ZetanStudio.Item
{
    public class ItemBaseInfoBlock : ItemInspectorBlock
    {
        private readonly SerializedObject serializedObject;
        private readonly SerializedProperty ID;
        private readonly SerializedProperty Name;
        private readonly SerializedProperty Icon;
        private readonly SerializedProperty Description;
        private readonly SerializedProperty type;
        private readonly SerializedProperty quality;
        private readonly SerializedProperty Weight;
        private readonly SerializedProperty StackLimit;
        private readonly SerializedProperty Discardable;
        private readonly string IDPrefix;

        private List<ItemNew> items;
        private HashSet<string> ids;
        private readonly HelpBox helpBox;
        public Action onInspectorChanged;

        public ItemBaseInfoBlock(SerializedObject serializedObject, string IDPrefix)
        {
            contentContainer.style.paddingRight = 0;
            this.serializedObject = serializedObject;
            this.IDPrefix = IDPrefix;
            ID = serializedObject.FindAutoProperty("ID");
            Name = serializedObject.FindAutoProperty("Name");
            Icon = serializedObject.FindAutoProperty("Icon");
            Description = serializedObject.FindAutoProperty("Description");
            type = serializedObject.FindProperty("type");
            quality = serializedObject.FindProperty("quality");
            Weight = serializedObject.FindAutoProperty("Weight");
            StackLimit = serializedObject.FindAutoProperty("StackLimit");
            Discardable = serializedObject.FindAutoProperty("Discardable");
            text = "基本信息";
            RefreshCache();
            helpBox = new HelpBox();
            helpBox.text = "无错误";
            helpBox.messageType = HelpBoxMessageType.Info;
            Add(helpBox);
            IMGUIContainer inspector = new IMGUIContainer(() =>
            {
                if (serializedObject.targetObject)
                    OnInspectorGUI(serializedObject);
            });
            Add(inspector);
            CheckError();
        }

        private void OnInspectorGUI(SerializedObject serializedObject)
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.Space(1);
            Rect rect = EditorGUILayout.GetControlRect();
            Icon.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x + rect.width - 54, rect.y, 54, rect.height * 3),
                                                              new GUIContent(""),
                                                              Icon.objectReferenceValue as Sprite,
                                                              typeof(Sprite),
                                                              false);
            if (string.IsNullOrEmpty(ID.stringValue))
            {
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 110, rect.height), ID);
                if (GUI.Button(new Rect(rect.x + rect.width - 108, rect.y, 50, rect.height), "生成ID"))
                {
                    ID.stringValue = ItemNew.Editor.GetAutoID(serializedObject.targetObject as ItemNew, items, IDPrefix);
                    serializedObject.ApplyModifiedProperties();
                    CheckError();
                    onInspectorChanged?.Invoke();
                }
            }
            else EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 58, rect.height), ID);
            rect = EditorGUILayout.GetControlRect();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 58, rect.height), Name);
            EditorGUI.PropertyField(new Rect(rect.x + EditorGUIUtility.labelWidth + 2,
                                             rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                                             rect.width - EditorGUIUtility.labelWidth - 60,
                                             rect.height), Discardable);
            EditorGUILayout.PropertyField(Description);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(quality);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Weight);
            EditorGUILayout.PropertyField(StackLimit);
            EditorGUILayout.EndHorizontal();
            //SerializedProperty iterator = serializedObject.GetIterator();
            //if (iterator != null)
            //{
            //    bool enterChildren = true;
            //    while (iterator.NextVisible(enterChildren) && iterator.propertyPath != "modules")
            //    {

            //        if ("m_Script" != iterator.propertyPath)
            //            EditorGUILayout.PropertyField(iterator, true);
            //        enterChildren = false;
            //    }
            //}
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                CheckError();
                onInspectorChanged?.Invoke();
            }
        }

        public void CheckError()
        {
            SerializedProperty id = serializedObject.FindAutoProperty("ID");
            bool empty = string.IsNullOrEmpty(id.stringValue);
            bool dump = ids.Contains(id.stringValue);
            bool invalid = empty || dump;
            if (invalid)
            {
                text = "基本信息(存在错误)";
                helpBox.text = empty ? "ID为空！" : "ID重复！";
                helpBox.messageType = HelpBoxMessageType.Error;
            }
            else
            {
                SerializedProperty name = serializedObject.FindAutoProperty("Name");
                if (string.IsNullOrEmpty(name.stringValue))
                {
                    text = "基本信息(可能有误)";
                    helpBox.text = "道具名为空";
                    helpBox.messageType = HelpBoxMessageType.Warning;
                }
                else
                {
                    SerializedProperty icon = serializedObject.FindAutoProperty("Icon");
                    if (!icon.objectReferenceValue)
                    {
                        text = "基本信息(可能有误)";
                        helpBox.text = "图标为空";
                        helpBox.messageType = HelpBoxMessageType.Warning;
                    }
                    else
                    {
                        text = "基本信息";
                        helpBox.text = "无错误";
                        helpBox.messageType = HelpBoxMessageType.Info;
                    }
                }
            }
        }
        public void RefreshCache()
        {
            if (!ItemEditorSettings.GetOrCreate().useDatabase) items = ZetanUtility.Editor.LoadAssets<ItemNew>();
            else items = ItemDatabase.Editor.GetItems();
            ids = items.Where(x => x != serializedObject.targetObject).Select(x => x.ID).ToHashSet();
        }
    }
}