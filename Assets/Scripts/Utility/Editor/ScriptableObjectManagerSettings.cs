using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio
{
    public class ScriptableObjectManagerSettings : ScriptableObject
    {
        public VisualTreeAsset treeUxml;
        public StyleSheet treeUss;
        public Vector2 minWindowSize = new Vector2(600, 600);
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplate;
        [Folder]
        public string newScriptFolder = "Assets/Scripts";
        public LanguageMap language;

        private static ScriptableObjectManagerSettings Find()
        {
            var settings = ZetanUtility.Editor.LoadAssets<ScriptableObjectManagerSettings>();
            if (settings.Count > 1) Debug.LogWarning(L.Tr(settings[0].language, "找到多个ScriptableObject管理器配置，将使用第一个"));
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static ScriptableObjectManagerSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<ScriptableObjectManagerSettings>();
                settings.treeUxml = ZetanUtility.Editor.LoadAssetWhere<VisualTreeAsset>(x => x.name == typeof(ScriptableObjectManager).Name);
                settings.treeUss = ZetanUtility.Editor.LoadAssetWhere<StyleSheet>(x => x.name == typeof(ScriptableObjectManager).Name);
                settings.scriptTemplate = ZetanUtility.Editor.LoadAssetWhere<TextAsset>(x => x.name.Contains("NewScriptableObject.cs"), extension: "txt");
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/Utility/Editor/Resources/so manager settings.asset"));
            }
            return settings;
        }
        private static class ZSSMSettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSSMSettingsProvider()
            {
                var settings = GetOrCreate();
                var provider = new SettingsProvider("Project/Zetan Studio/ZSSMSettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = L.Tr(settings ? settings.language : null, "ScriptableObject管理器"),
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(settings);

                        Label title = new Label() { text = L.Tr(settings ? settings.language : null, "ScriptableObject管理器设置") };
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
