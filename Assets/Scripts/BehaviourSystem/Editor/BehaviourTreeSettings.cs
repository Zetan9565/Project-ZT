using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ZetanStudio.BehaviourTree
{
    public sealed class BehaviourTreeSettings : ScriptableObject
    {
        [DisplayName("编辑器UXML")]
        public VisualTreeAsset treeUxml;
        [DisplayName("编辑器USS")]
        public StyleSheet treeUss;
        [DisplayName("编辑器最小尺寸")]
        public Vector2 minWindowSize = new Vector2(800, 600);
        [DisplayName("结点UXML")]
        public VisualTreeAsset nodeUxml;
        [DisplayName("行为结点脚本模板")]
        public TextAsset scriptTemplateAction;
        [DisplayName("条件结点脚本模板")]
        public TextAsset scriptTemplateConditional;
        [DisplayName("复合结点脚本模板")]
        public TextAsset scriptTemplateComposite;
        [DisplayName("修饰结点脚本模板")]
        public TextAsset scriptTemplateDecorator;
        [DisplayName("共享变量脚本模板")]
        public TextAsset scriptTemplateVariable;
        [DisplayName("新结点脚本默认路径")]
        public string newNodeScriptFolder = "Assets/Scripts/BehaviourSystem/Base/Node";
        [DisplayName("新变量脚本默认路径")]
        public string newVarScriptFolder = "Assets/Scripts/BehaviourSystem/Base/Variable";
        [DisplayName("选取树时同步更改编辑器")]
        public bool changeOnSelected;

        private static BehaviourTreeSettings Find()
        {
            var settings = ZetanUtility.Editor.LoadAssets<BehaviourTreeSettings>();
            if (settings.Count > 1) Debug.LogWarning("找到多个行为树编辑器配置，将使用第一个");
            if (settings.Count > 0) return settings[0];
            return null;
        }

        public static BehaviourTreeSettings GetOrCreate()
        {
            var settings = Find();
            if (settings == null)
            {
                settings = CreateInstance<BehaviourTreeSettings>();
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/BehaviourSystem/Editor/BehaviourTreeSettings.asset"));
            }
            return settings;
        }
        private static class ZSBTSettingsUIElementsRegister
        {
            [SettingsProvider]
            public static SettingsProvider CreateZSBTSettingsProvider()
            {
                var provider = new SettingsProvider("Project/Zetan Studio/ZSBTSettingsUIElementsSettings", SettingsScope.Project)
                {
                    label = "行为树编辑器",
                    activateHandler = (searchContext, rootElement) =>
                    {
                        SerializedObject serializedObject = new SerializedObject(BehaviourTreeSettings.GetOrCreate());

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
}