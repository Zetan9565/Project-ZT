using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ZetanStudio.BehaviourTree
{
    public sealed class BehaviourTreeSettings : ScriptableObject
    {
        [DisplayName("树视图UXML")]
        public VisualTreeAsset treeUxml;
        [DisplayName("树视图USS")]
        public StyleSheet treeUss;
        [DisplayName("结点UXML")]
        public VisualTreeAsset nodeUxml;
        [DisplayName("行为结点脚本模版")]
        public TextAsset scriptTemplateAction;
        [DisplayName("条件结点脚本模版")]
        public TextAsset scriptTemplateConditional;
        [DisplayName("复合结点脚本模版")]
        public TextAsset scriptTemplateComposite;
        [DisplayName("修饰结点脚本模版")]
        public TextAsset scriptTemplateDecorator;
        [DisplayName("新结点脚本默认路径")]
        public string newScriptFolder = "Assets/Scripts/BehaviourSystem/Base/Nodes";

        private static BehaviourTreeSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:BehaviourTreeSettings");
            if (guids.Length > 1) Debug.LogWarning("找到多个行为树编辑器配置，将使用第一个");

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<BehaviourTreeSettings>(path);
            }
        }

        public static BehaviourTreeSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateInstance<BehaviourTreeSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Scripts/BehaviourSystem/BehaviourTreeSettings.asset");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }
    }

    static class ZSBTSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateZSBTSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Zetan Studio/ZSBTSettingsUIElementsSettings", SettingsScope.Project)
            {
                label = "行为树编辑器",
                activateHandler = (searchContext, rootElement) =>
                {
                    SerializedObject serializedObject = new SerializedObject(BehaviourTreeSettings.GetOrCreateSettings());

                    Label title = new Label() { text = "行为树编辑器设置" };
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