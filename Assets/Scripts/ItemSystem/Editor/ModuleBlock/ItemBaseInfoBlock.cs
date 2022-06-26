using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZetanStudio.Extension.Editor;
using ZetanStudio.ItemSystem.Editor;

namespace ZetanStudio.ItemSystem
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
        private List<Item> items;
        private HashSet<string> ids;
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
            text = Tr("基本信息");
            RefreshCache();
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
            string oldID = ID.stringValue;
            if (string.IsNullOrEmpty(ID.stringValue) || ids.Contains(ID.stringValue))
            {
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 110, rect.height), ID);
                if (GUI.Button(new Rect(rect.x + rect.width - 108, rect.y, 50, rect.height), Tr("生成ID")))
                {
                    ID.stringValue = Item.Editor.GetAutoID(serializedObject.targetObject as Item, items, IDPrefix);
                    serializedObject.ApplyModifiedProperties();
                    EditorApplication.delayCall += CheckError;
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
                if (oldID != ID.stringValue && AssetDatabase.IsSubAsset(serializedObject.targetObject))
                {
                    serializedObject.targetObject.name = ID.stringValue;
                    EditorUtility.SetDirty(serializedObject.targetObject);
                }
            }
        }

        public void CheckError()
        {
            EditorApplication.delayCall -= CheckError;
            SerializedProperty id = serializedObject.FindAutoProperty("ID");
            bool empty = string.IsNullOrEmpty(id.stringValue);
            bool dump = ids.Contains(id.stringValue);
            bool invalid = empty || dump;
            if (invalid)
            {
                text = $"{Tr("基本信息")}({Tr("存在错误")})";
                this.Q<Toggle>().tooltip = $"{Tr("错误类型")}: {(empty ? Tr("ID为空") : Tr("ID重复"))}";
            }
            else
            {
                SerializedProperty name = serializedObject.FindAutoProperty("Name");
                if (string.IsNullOrEmpty(name.stringValue))
                {
                    text = $"{Tr("基本信息")}({Tr("可能有误")})";
                    this.Q<Toggle>().tooltip = $"{Tr("错误类型")}：{Tr("道具名为空")}";
                }
                else
                {
                    SerializedProperty icon = serializedObject.FindAutoProperty("Icon");
                    if (!icon.objectReferenceValue)
                    {
                        text = $"{Tr("基本信息")}({Tr("可能有误")})";
                        this.Q<Toggle>().tooltip = $"{Tr("错误类型")}：{Tr("图标为空")}";
                    }
                    else
                    {
                        text = Tr("基本信息");
                        this.Q<Toggle>().tooltip = null;
                    }
                }
            }
        }
        public void RefreshCache()
        {
            if (!Item.UseDatabase) items = ZetanUtility.Editor.LoadAssets<Item>();
            else items = ItemDatabase.Editor.GetItems();
            ids = items.Where(x => x != serializedObject.targetObject).Select(x => x.ID).ToHashSet();
        }
    }
}