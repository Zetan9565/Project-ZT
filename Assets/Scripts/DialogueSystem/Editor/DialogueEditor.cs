using UnityEditor;
using UnityEngine;

public class DialogueEditor : ConfigurationEditor<Dialogue>
{
    [MenuItem("Zetan Studio/配置管理/对话")]
    public static void CreateWindow()
    {
        DialogueEditor window = GetWindowWithRect<DialogueEditor>(new Rect(0, 0, 450, 720), false, "对话管理器");
        window.Show();
    }

    protected override bool CompareKey(Dialogue element, out string remark)
    {
        remark = string.Empty;
        if (!element) return false;
        if (element.ID.Contains(keyWords))
        {
            remark = $"识别码：{ZetanEditorUtility.TrimContentByKey(element.ID, keyWords, 16)}";
            return true;
        }
        for (int i = 0; i < element.Words.Count; i++)
        {
            var words = element.Words[i];
            if (words.Content.Contains(keyWords))
            {
                remark = $"第[{i}]句：{ZetanEditorUtility.TrimContentByKey(words.Content, keyWords, 20)}";
                return true;
            }
            else if (MiscFuntion.HandlingKeyWords(words.Content).Contains(keyWords))
            {
                remark = $"第[{i}]句：{ZetanEditorUtility.TrimContentByKey(MiscFuntion.HandlingKeyWords(words.Content, false, objects.ToArray()), keyWords, 20)}";
                return true;
            }
            for (int j = 0; j < words.Options.Count; j++)
            {
                var option = words.Options[j];
                if (option.Title.Contains(keyWords))
                {
                    remark = $"第[{i}]句第[{j}]个选项标题：{ZetanEditorUtility.TrimContentByKey(option.Title, keyWords, 16)}";
                    return true;
                }
            }
        }
        return false;
    }

    protected override string GetNewFileName(System.Type type)
    {
        return "dialogue";
    }

    protected override string GetConfigurationName()
    {
        return "对话";
    }

    protected override string GetElementNameLabel()
    {
        return "识别码";
    }

    protected override string GetElementName(Dialogue element)
    {
        return element.ID;
    }
}