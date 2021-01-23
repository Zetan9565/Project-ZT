using UnityEngine;

public enum TimeSystem
{
    [InspectorName("12小时制")]
    System12,

    [InspectorName("24小时制")]
    System24,

    [InspectorName("十二时辰制")]
    Twelve
}

public enum TimeUnit
{
    [InspectorName("分")]
    Minute,

    [InspectorName("时")]
    Hour,

    [InspectorName("天")]
    Day,

    [InspectorName("月")]
    Month,

    [InspectorName("季")]
    Season,

    [InspectorName("年")]
    Year
}

public enum Month
{
    January = 1,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    Decamber
}