using System;
using UnityEditor;
using UnityEngine;

public class TalkerInfoEditor : ConfigurationEditor<TalkerInformation>
{
    [MenuItem("Zetan Studio/配置管理/对话人")]
    private static void CreateWindow()
    {
        TalkerInfoEditor window = GetWindowWithRect<TalkerInfoEditor>(new Rect(0, 0, 450, 720), false, "对话人编辑器");
        window.Show();
    }

    protected override string GetConfigurationName()
    {
        return "对话人";
    }

    protected override void DrawElementOperator(TalkerInformation element, Rect rect)
    {
        if (element.Enable)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y, 60, lineHeight), "不启用"))
                element.GetType().GetField("enable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(element, false);
        }
        else
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 40, rect.y, 40, lineHeight), "启用"))
                element.GetType().GetField("enable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(element, true);
        }
    }

    protected override string GetNewFileName(Type subType)
    {
        return "dialogue";
    }

    protected override bool CompareKey(TalkerInformation element, out string remark)
    {
        remark = string.Empty;
        if (element.ID.Contains(keyWords))
        {
            remark = ZetanEditorUtility.TrimContentByKey(element.ID, keyWords, 16);
            return true;
        }
        else if (element.Name.Contains(keyWords))
        {
            remark = ZetanEditorUtility.TrimContentByKey(element.Name, keyWords, 16);
            return true;
        }
        else if ((keyWords.ToLower() == "{enable}" || keyWords == "{启用}") && element.Enable)
        {
            remark = "已启用";
            return true;
        }
        else if ((keyWords.ToLower() == "{disable}" || keyWords == "{未启用}") && !element.Enable)
        {
            remark = "未启用";
            return true;
        }
        return false;
    }

    protected override string GetElementNameLabel()
    {
        return "名字";
    }

    protected override string GetElementName(TalkerInformation element)
    {
        return element.Name;
    }
}