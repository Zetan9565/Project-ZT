using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ZetanStudio.BehaviourTree.Editor
{
    public sealed class BehaviourTreeEditorSettings : ScriptableObject
    {
        public VisualTreeAsset treeUxml;
        public StyleSheet treeUss;
        public Vector2 minWindowSize = new Vector2(800, 600);
        public VisualTreeAsset nodeUxml;
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplateAction;
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplateConditional;
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplateComposite;
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplateDecorator;
        [ObjectSelector(typeof(TextAsset), extension: ".cs.txt")]
        public TextAsset scriptTemplateVariable;
        [Folder]
        public string newNodeScriptFolder = "Assets/Scripts/BehaviourSystem/Base/Node";
        [Folder]
        public string newVarScriptFolder = "Assets/Scripts/BehaviourSystem/Base/Variable";
        [Folder]
        public string newAssetFolder = "Assets/Resources/Configuration/AI";
        public bool changeOnSelected;
        public LanguageMap language;

        private static BehaviourTreeEditorSettings Find()
        {
            var settings = ZetanUtility.Editor.LoadAssets<BehaviourTreeEditorSettings>();
            if (settings.Count > 1) Debug.LogWarning(Language.Tr(settings[0].language, "找到多个行为树编辑器配置，将使用第一个"));
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static BehaviourTreeEditorSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<BehaviourTreeEditorSettings>();
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/BehaviourSystem/Editor/BehaviourTreeSettings.asset"));
            }
            return settings;
        }
        private static class ZSBTSettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSBTSettingsProvider()
            {
                var settings = GetOrCreate();
                var provider = new SettingsProvider("Project/Zetan Studio/ZSBTSettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = Language.Tr(settings ? settings.language : null, "行为树编辑器"),
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(GetOrCreate());

                        Label title = new Label() { text = Language.Tr(settings ? settings.language : null, "行为树编辑器设置") };
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