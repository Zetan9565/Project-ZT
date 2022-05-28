using UnityEditor;
using UnityEngine;
using ZetanStudio;
using System.Collections;
using System.Reflection;

[CustomPropertyDrawer(typeof(LabelAttribute)), InitializeOnLoad]
public class LabelDrawer : EnhancedAttributeDrawer
{
    private static LabelLanguage language;

    static LabelDrawer()
    {
        if (!language) language = ZetanUtility.Editor.LoadAsset<LabelLanguage>();
        if (!language)
        {
            language = ScriptableObject.CreateInstance<LabelLanguage>();
            AssetDatabase.CreateAsset(language, UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/New {ObjectNames.NicifyVariableName(typeof(LabelLanguage).Name)}.asset"));
        }
    }

    [MenuItem("Zetan Studio/编辑器工具/收集Label标签")]
    private static void Collect()
    {
        if (EditorUtility.DisplayDialog("警告", "将会清除语言映射表并重新搜集，是否继续？", "继续", "取消"))
        {
            var items = typeof(LabelLanguage).GetField("items", ZetanUtility.CommonBindingFlags).GetValue(language) as IList;
            items.Clear();
            foreach (var field in TypeCache.GetFieldsWithAttribute<LabelAttribute>())
            {
                string label = field.GetCustomAttribute<LabelAttribute>().name;
                items.Add(new LanguageMapItem(label, label));
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute attribute = this.attribute as LabelAttribute;
        label.text = Language.Tr(language, attribute.name);
        PropertyField(position, property, label);
    }
}