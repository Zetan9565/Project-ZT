using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.DialogueSystem.Editor
{
    public class DialogueEditorSettings : ScriptableObject
    {
        public VisualTreeAsset treeUxml;
        public StyleSheet treeUss;
        public Vector2 minWindowSize = new Vector2(1280, 600);
        public LanguageMap language;

        private static DialogueEditorSettings Find()
        {
            var settings = Utility.Editor.LoadAssets<DialogueEditorSettings>();
            if (settings.Count > 1) Debug.LogWarning(L.Tr(settings[0].language, "找到多个对话编辑器配置，将使用第一个"));
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static DialogueEditorSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<DialogueEditorSettings>();
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath($"Assets/Scripts/DialogueSystem/Editor/Resources/{ObjectNames.NicifyVariableName(typeof(DialogueEditorSettings).Name)}.asset"));
            }
            return settings;
        }
        private static class ZSDESettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSDESettingsProvider()
            {
                var settings = GetOrCreate();
                var provider = new SettingsProvider("Project/Zetan Studio/ZSDESettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = L.Tr(settings ? settings.language : null, "对话编辑器"),
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(GetOrCreate());

                        Label title = new Label() { text = L.Tr(settings ? settings.language : null, "对话编辑器设置") };
                        title.style.paddingLeft = 10f;
                        title.style.fontSize = 19f;
                        title.AddToClassList("title");
                        rootElement.Add(title);

                        var properties = new VisualElement()
                        {
                            style = { flexDirection = FlexDirection.Column }
                        };
                        properties.AddToClassList("property-list");
                        rootElement.Add(properties);

                        properties.Add(new InspectorElement(serializedObject));

                        rootElement.Bind(serializedObject);
                    },
                };

                return provider;
            }
        }

    }
}