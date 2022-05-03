using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestEditorSettings : ScriptableObject
{
    [DisplayName("编辑器UXML")]
    public VisualTreeAsset treeUxml;
    [DisplayName("编辑器USS")]
    public StyleSheet treeUss;
    [DisplayName("编辑器最小尺寸")]
    public Vector2 minWindowSize = new Vector2(800, 600);

    private static QuestEditorSettings Find()
    {
        var settings = ZetanUtility.Editor.LoadAssets<QuestEditorSettings>();
        if (settings.Count > 1) Debug.LogWarning("找到多个任务编辑器配置，将使用第一个");
        if (settings.Count > 0) return settings[0];
        return null;
    }

    public static QuestEditorSettings GetOrCreate()
    {
        var settings = Find();
        if (settings == null)
        {
            settings = CreateInstance<QuestEditorSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Scripts/QuestSystem/Editor/QuestEditorSettings.asset");
        }
        return settings;
    }
    private static class ZSQESettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateZSQESettingsProvider()
        {
            var provider = new SettingsProvider("Project/Zetan Studio/ZSQESettingsUIElementsSettings", SettingsScope.Project)
            {
                label = "任务编辑器",
                activateHandler = (searchContext, rootElement) =>
                {
                    SerializedObject serializedObject = new SerializedObject(GetOrCreate());

                    Label title = new Label() { text = "任务编辑器设置" };
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