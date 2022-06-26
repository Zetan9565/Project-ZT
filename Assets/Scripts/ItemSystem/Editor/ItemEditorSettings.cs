using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.ItemSystem
{
    public class ItemEditorSettings : ScriptableObject
    {
        public VisualTreeAsset treeUxml;
        public StyleSheet treeUss;
        public Vector2 minWindowSize = new Vector2(800, 600);
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplate;
        [Folder]
        public string newScriptFolder = "Assets/Scripts/ItemSystem/Base/Module";
        public LanguageMap language;

        private static ItemEditorSettings Find()
        {
            var settings = ZetanUtility.Editor.LoadAssets<ItemEditorSettings>();
            if (settings.Count > 1) Debug.LogWarning(L.Tr(settings[0].language, "找到多个道具编辑器配置，将使用第一个"));
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static ItemEditorSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<ItemEditorSettings>();
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/ItemSystem/Editor/Resources/item editor settings.asset"));
            }
            return settings;
        }
        private static class ZSIESettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSIESettingsProvider()
            {
                var settings = GetOrCreate();
                var provider = new SettingsProvider("Project/Zetan Studio/ZSIESettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = L.Tr(settings ? settings.language : null, "道具编辑器"),
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(settings);

                        Label title = new Label() { text = L.Tr(settings ? settings.language : null, "道具编辑器设置") };
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