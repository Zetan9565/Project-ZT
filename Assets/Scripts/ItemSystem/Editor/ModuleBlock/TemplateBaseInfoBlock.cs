using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using ZetanStudio.Extension.Editor;

namespace ZetanStudio.Item
{
    public class TemplateBaseInfoBlock : ItemInspectorBlock
    {
        SerializedObject serializedObject;
        private HelpBox helpBox;
        public Action onInspectorChanged;

        public TemplateBaseInfoBlock(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;
            text = "基本信息";
            helpBox = new HelpBox();
            Add(helpBox);
            helpBox.text = "无错误";
            helpBox.messageType = HelpBoxMessageType.Info;
            IMGUIContainer inspector = new IMGUIContainer(() =>
            {
                if (serializedObject.targetObject)
                {
                    EditorGUI.BeginChangeCheck();
                    serializedObject.UpdateIfRequiredOrScript();
                    SerializedProperty iterator = serializedObject.GetIterator();
                    if (iterator != null)
                    {
                        bool enterChildren = true;
                        while (iterator.NextVisible(enterChildren) && iterator.propertyPath != "modules")
                        {

                            if ("m_Script" != iterator.propertyPath)
                                EditorGUILayout.PropertyField(iterator, true);
                            enterChildren = false;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        CheckError();
                        onInspectorChanged?.Invoke();
                    }
                }
            });
            Add(inspector);
            CheckError();
        }

        public void CheckError()
        {
            SerializedProperty name = serializedObject.FindAutoProperty("Name");
            if (string.IsNullOrEmpty(name.stringValue))
            {
                text = "基本信息(可能有误)";
                helpBox.text = "模板名为空";
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