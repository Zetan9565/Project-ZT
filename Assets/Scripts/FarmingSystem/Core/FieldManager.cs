using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.TimeSystem;

public static class FieldManager
{
    private static readonly List<FieldData> fields = new List<FieldData>();

    public static void Reclaim(FieldData field)
    {
        lock (fields)
            fields.Add(field);
    }

    [InitMethod]
    public static void Init()
    {
        fields.Clear();
        TimeManager.Instance.OnTimePassed -= TimePass;
        TimeManager.Instance.OnTimePassed += TimePass;
    }

    private static void TimePass(decimal realTime)
    {
        using var fieldEnum = fields.GetEnumerator();
        while (fieldEnum.MoveNext())
            fieldEnum.Current.TimePass((float)realTime);
    }

    #region 消息
    public const string FieldCropPlanted = "FieldCropPlanted";
    #endregion
}