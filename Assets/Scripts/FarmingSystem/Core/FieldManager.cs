using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Zetan Studio/管理器/农田管理器")]
public class FieldManager : SingletonMonoBehaviour<FieldManager>
{
    private readonly List<FieldData> fields = new List<FieldData>();

    public void Reclaim(FieldData field)
    {
        lock (fields)
            fields.Add(field);
    }

    public void Init()
    {
        fields.Clear();
        TimeManager.Instance.OnTimePassed -= TimePass;
        TimeManager.Instance.OnTimePassed += TimePass;
    }

    private void TimePass(decimal realTime)
    {
        using var fieldEnum = fields.GetEnumerator();
        while (fieldEnum.MoveNext())
            fieldEnum.Current.TimePass((float)realTime);
    }

    #region 消息
    public const string FieldCropPlanted = "FieldCropPlanted";
    #endregion
}