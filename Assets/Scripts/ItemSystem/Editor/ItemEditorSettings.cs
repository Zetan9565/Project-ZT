using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZetanStudio.Item
{
    public class ItemEditorSettings : ScriptableObject
    {
        [DisplayName("编辑器UXML")]
        public VisualTreeAsset treeUxml;
        [DisplayName("编辑器USS")]
        public StyleSheet treeUss;
        [DisplayName("编辑器最小尺寸")]
        public Vector2 minWindowSize = new Vector2(800, 600);
        [DisplayName("模块脚本模板")]
        public TextAsset scriptTemplate;
        [DisplayName("新模块脚本默认路径")]
        public string newScriptFolder = "Assets/Scripts/ItemSystem/Base/Module";
        [DisplayName("使用数据库"), Tooltip("若否，则为每个道具新建一个'*.asset'文件")]
        public bool useDatabase = false;

        private static ItemEditorSettings Find()
        {
            var settings = ZetanUtility.Editor.LoadAssets<ItemEditorSettings>();
            if (settings.Count > 1) Debug.LogWarning("找到多个道具编辑器配置，将使用第一个");
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static ItemEditorSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<ItemEditorSettings>();
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/ItemSystem/Editor/ItemEditorSettings.asset"));
            }
            return settings;
        }
        private static class ZSIESettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSIESettingsProvider()
            {
                var provider = new SettingsProvider("Project/Zetan Studio/ZSIESettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = "道具编辑器",
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(GetOrCreate());

                        Label title = new Label() { text = "道具编辑器设置" };
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