using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{
    public readonly float minLimit;
    public readonly float maxLimit;
    public readonly string minLimitField;
    public readonly string maxLimitField;
    public readonly string limitField;

    public MinMaxSliderAttribute(float minLimit, float maxLimit)
    {
        this.minLimit = minLimit;
        this.maxLimit = maxLimit;
    }
    /// <summary>
    /// 只支持UnityEngine.Object派生类的成员
    /// </summary>
    /// <param name="minLimitField">下限字段，只能是float、int</param>
    /// <param name="maxLimitField">上限字段，只能是float、int</param>
    public MinMaxSliderAttribute(string minLimitField, string maxLimitField)
    {
        this.minLimitField = minLimitField;
        this.maxLimitField = maxLimitField;
    }
    /// <summary>
    /// 只支持UnityEngine.Object派生类的成员
    /// </summary>
    /// <param name="limitField">界限字段，只能是Vector2、Vector2Int</param>
    public MinMaxSliderAttribute(string limitField)
    {
        this.limitField = limitField;
    }
}