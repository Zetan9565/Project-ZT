using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZetanStudio;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : EnhancedPropertyDrawer
{
    [MenuItem("Zetan Studio/编辑器工具/收集Label标签")]
    private static void Collect()
    {
        if (EditorUtility.DisplayDialog(Tr("提示"), Tr("将会在本地创建一个语言映射表并使用，是否继续？"), Tr("继续"), Tr("取消")))
        {
            var language = ScriptableObject.CreateInstance<LanguageSet>();
            var maps = new List<LanguageMap>();;
            maps.Clear();
            var keys = new HashSet<string>();
            foreach (var field in TypeCache.GetFieldsWithAttribute<LabelAttribute>())
            {
                string label = field.GetCustomAttribute<LabelAttribute>().name;
                if (!keys.Contains(label))
                {
                    keys.Add(label);
                    maps.Add(new LanguageMap(label, label));
                }
            }
            LanguageSet.Editor.SetMaps(language, maps.ToArray());
            AssetDatabase.CreateAsset(language, AssetDatabase.GenerateUniqueAssetPath($"Assets/new label language.asset"));
            EditorGUIUtility.PingObject(language);
            var singleton = LabelLocalization.GetOrCreate();
            singleton.language = language;
            Utility.Editor.SaveChange(singleton);
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