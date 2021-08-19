using UnityEditor;
using UnityEngine;

public class QuestEditor : ConfigurationEditor<Quest>
{
    [MenuItem("Zetan Studio/配置管理/任务")]
    public static void CreateWindow()
    {
        QuestEditor window = GetWindowWithRect<QuestEditor>(new Rect(0, 0, 450, 720), false, "任务管理器");
        window.Show();
    }

    protected override string GetConfigurationName()
    {
        return "任务";
    }

    protected override bool CompareKey(Quest element, out string remark)
    {
        remark = string.Empty;
        if (!element) return false;
        if(element.Title.Contains(keyWords))
        {
            remark = "标题：" + ZetanEditorUtility.TrimContentByKey(element.Title, keyWords, 16);
            return true;
        }
        else if (element.Description.Contains(keyWords))
        {
            remark = "描述：" + ZetanEditorUtility.TrimContentByKey(element.Description, keyWords, 20);
            return true;
        }
        else
        {
            for (int i = 0; i < element.Objectives.Count; i++)
            {
                var obj = element.Objectives[i];
                if(obj.DisplayName.Contains(keyWords))
                {
                    remark = "第[" + i + "]个目标标题：" + ZetanEditorUtility.TrimContentByKey(obj.DisplayName, keyWords, 16);
                    return true;
                }
            }
        }
        return base.CompareKey(element, out remark);
    }

    protected override string GetElementNameLabel()
    {
        return "标题";
    }

    protected override string GetElementName(Quest element)
    {
        return element.Title;
    }
}