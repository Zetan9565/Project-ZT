using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZetanStudio;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : EnhancedAttributeDrawer
{
    [MenuItem("Zetan Studio/编辑器工具/收集Label标签")]
    private static void Collect()
    {
        if (EditorUtility.DisplayDialog(Tr("提示"), Tr("将会在本地创建一个语言映射表并使用，是否继续？"), Tr("继续"), Tr("取消")))
        {
            var language = ScriptableObject.CreateInstance<LanguageMap>();
            var items = typeof(LanguageMap).GetField("items", ZetanUtility.CommonBindingFlags).GetValue(language) as IList;
            items.Clear();
            foreach (var field in TypeCache.GetFieldsWithAttribute<LabelAttribute>())
            {
                string label = field.GetCustomAttribute<LabelAttribute>().name;
                items.Add(new LanguageMapItem(label, label));
            }
            AssetDatabase.CreateAsset(language, AssetDatabase.GenerateUniqueAssetPath($"Assets/new label language.asset"));
            EditorGUIUtility.PingObject(language);
            var singleton = LabelLocalization.GetOrCreate();
            singleton.language = language;
            ZetanUtility.Editor.SaveChange(singleton);
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute attribute = this.attribute as LabelAttribute;
        label.text = Tr(attribute.name);
        PropertyField(position, property, label);
    }

    private static string Tr(string text)
    {
        return LabelLocalization.Tr(text);
    }
}