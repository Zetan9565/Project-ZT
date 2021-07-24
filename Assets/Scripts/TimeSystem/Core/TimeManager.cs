using System;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/时间管理器")]
public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    #region 常量
    public const int HourToSeconds = 3600;
    public const int DayToSeconds = 86400;
    public const int WeekToSeconds = 604800;
    public const int MonthToSeconds = 2592000;
    public const int YearToSeconds = 31104000;
    #endregion

    #region 静态成员
    /// <summary>
    /// 一个游戏分在现实中的耗时(秒)
    /// </summary>
    public static float OneMinute => Instance ? 60 / Instance.Multiples : 1;
    /// <summary>
    /// 一个游戏时在现实中的耗时(秒)
    /// </summary>
    public static float OneHour => OneMinute * 60;
    /// <summary>
    /// 一个游戏日在现实中的耗时(秒)
    /// </summary>
    public static float OneDay => OneHour * 24;
    /// <summary>
    /// 一个游戏月在现实中的耗时(秒)
    /// </summary>
    public static float OneMonth => OneDay * 30;
    /// <summary>
    /// 一个游戏季在现实中的耗时(秒)
    /// </summary>
    public static float OneSeason => OneMonth * 3;
    /// <summary>
    /// 一个游戏年在现实中的耗时(秒)
    /// </summary>
    public static float OneYear => OneSeason * 4;

    public static string WeekDayToString(DayOfWeek dayOfWeek, TimeSystem timeSystem = TimeSystem.System24)
    {
        DayOfWeek day = dayOfWeek;
        switch (day)
        {
            case DayOfWeek.Monday:
                return timeSystem == TimeSystem.Twelve ? "月曜" : "星期一";
            case DayOfWeek.Tuesday:
                return timeSystem == TimeSystem.Twelve ? "金曜" : "星期二";
            case DayOfWeek.Wednesday:
                return timeSystem == TimeSystem.Twelve ? "木曜" : "星期三";
            case DayOfWeek.Thursday:
                return timeSystem == TimeSystem.Twelve ? "水曜" : "星期四";
            case DayOfWeek.Friday:
                return timeSystem == TimeSystem.Twelve ? "火曜" : "星期五";
            case DayOfWeek.Saturday:
                return timeSystem == TimeSystem.Twelve ? "土曜" : "星期六";
            case DayOfWeek.Sunday:
            default:
                return timeSystem == TimeSystem.Twelve ? "日曜" : "星期日";
        }
    }

    public static Seaon MonthToSeason(Month month)
    {
        switch (month)
        {
            case Month.January:
            case Month.February:
            case Month.March:
                return Seaon.Spring;
            case Month.April:
            case Month.May:
            case Month.June:
                return Seaon.Summer;
            case Month.July:
            case Month.August:
            case Month.September:
                return Seaon.Autumn;
            case Month.October:
            case Month.November:
            case Month.Decamber:
            default:
                return Seaon.Winter;
        }
    }
    #endregion

    #region 可控成员
    [SerializeField]
    private TimeUI UI;

    [SerializeField]
    [Tooltip("现实中的 1 秒折合游戏中的多少秒钟？")]
    private int multiples = 60;
    public int Multiples
    {
        get => multiples;
        set { multiples = value; }
    }

    [SerializeField]
    private float timeline = 8;
    public float Timeline
    {
        get => timeline;
        set
        {
            timeline = value % 24;
            totalTime = ((Days - 1) * DayToSeconds + (decimal)timeline * HourToSeconds) / multiples;
        }
    }

    [SerializeField]
    private TimeSystem timeSystem = TimeSystem.System24;
    public TimeSystem TimeSystem
    {
        get
        {
            return timeSystem;
        }
    }

    public delegate void TimePassedListner(decimal realSecond);
    public event TimePassedListner OnTimePassed;
    #endregion

    #region 日期相关
    public string DateString
    {
        get
        {
            Date.month = CurrentMonth;
            Date.dayOfMonth = DayOfMonth;
            Date.dayOfWeek = DayOfWeek;
            return Date.ToString(timeSystem);
        }
    }
    public string TimeString
    {
        get
        {
            switch (TimeSystem)
            {
                case TimeSystem.System12:
                    return string.Format("{0}:{1}{2}", (int)timeline % 12 == 0 ? "12" : ((int)timeline % 12 + ""),
                        ((int)((timeline - (int)timeline) * 60) % 60).ToString().PadLeft(2, '0'), timeline >= 12 ? "PM" : "AM");
                case TimeSystem.System24:
                default:
                    return string.Format("{0}:{1}", (int)timeline, ((int)((timeline - (int)timeline) * 60) % 60).ToString().PadLeft(2, '0'));
                case TimeSystem.Twelve:
                    string GetHour()
                    {
                        if ((timeline >= 23 && timeline <= 24) || (timeline >= 0 && timeline < 1))
                            return "子";
                        else if (timeline >= 1 && timeline < 3)
                            return "丑";
                        else if (timeline >= 3 && timeline < 5)
                            return "寅";
                        else if (timeline >= 5 && timeline < 7)
                            return "卯";
                        else if (timeline >= 7 && timeline < 9)
                            return "辰";
                        else if (timeline >= 9 && timeline < 11)
                            return "巳";
                        else if (timeline >= 11 && timeline < 13)
                            return "午";
                        else if (timeline >= 13 && timeline < 15)
                            return "未";
                        else if (timeline >= 15 && timeline < 17)
                            return "申";
                        else if (timeline >= 17 && timeline < 19)
                            return "酉";
                        else if (timeline >= 19 && timeline < 21)
                            return "戌";
                        else
                            return "亥";
                    }
                    string GetMoment()
                    {
                        float dec = timeline - (int)timeline;
                        dec = ((int)timeline % 2 == 0 ? 1 + dec : dec) * 0.5f;
                        int moment = (int)(dec * 8) % 8;
                        switch (moment)
                        {
                            case 0:
                                return "初";
                            case 1:
                                return "时一刻";
                            case 2:
                                return "时二刻";
                            case 3:
                                return "时三刻";
                            case 4:
                                return "正";
                            case 5:
                                return "时五刻";
                            case 6:
                                return "时六刻";
                            case 7:
                                return "时七刻";
                            default:
                                return string.Empty;
                        }
                    }
                    return string.Format("{0}{1}", GetHour(), GetMoment());
            }
        }
    }

    public DateInfo Date { get; private set; } = new DateInfo();
    public int DayOfMonth
    {
        get
        {
            int temp = Days % 30;
            return temp == 0 ? 30 : temp;
        }
    }
    public DayOfWeek DayOfWeek
    {
        get
        {
            int day = Days % 7;
            return day == 0 ? DayOfWeek.Saturday : (DayOfWeek)(day - 1);
        }
    }
    public int DayOfYear
    {
        get
        {
            int dayOfYeay = Days % 360;
            return dayOfYeay == 0 ? 360 : dayOfYeay;
        }
        set
        {
            value = (value < 1 ? 1 : value) % 360;
            value = value == 0 ? 360 : value;
            totalTime = ((Years - 1) * YearToSeconds + (value - 1) * DayToSeconds + (decimal)timeline * HourToSeconds) / multiples;
        }
    }
    public Month CurrentMonth
    {
        get
        {
            int monthIndex = Months%12;
            return (Month)(monthIndex == 0 ? 12 : monthIndex);
        }
    }
    public Seaon CurrentSeason => MonthToSeason(CurrentMonth);
    public DayOfWeek WeekDayOfTheFirstDayOfCurrentMonth
    {
        get
        {
            int dayOfMonth = DayOfMonth;
            int dayOfWeek = (int)DayOfWeek;
            for (int i = dayOfMonth - 1; i > 0; i--)
            {
                dayOfWeek--;
                if (dayOfWeek < 0) dayOfWeek = 6;
            }
            return (DayOfWeek)dayOfWeek;
        }
    }
    #endregion

    #region 计数相关
    public int Days
    {
        get
        {
            int days = Mathf.CeilToInt((float)(totalTime * multiples / DayToSeconds));
            return days < 1 ? 1 : days;
        }
    }//从1开始计
    public int Weeks
    {
        get
        {
            int weeks = Mathf.CeilToInt((float)(totalTime * multiples / WeekToSeconds));
            return weeks < 1 ? 1 : weeks;
        }
    }//从1开始计
    public int Months//从1开始计
    {
        get
        {
            int months = Mathf.CeilToInt((float)(totalTime * multiples / MonthToSeconds));
            return months < 1 ? 1 : months;
        }
    }
    public int Years
    {
        get
        {
            int years = Mathf.CeilToInt((float)(totalTime * multiples / YearToSeconds));
            return years < 1 ? 1 : years;
        }
    }//从1开始计
    #endregion

    #region 运行时相关
    public float NormalizeTimeline => timeline / 24;

    [SerializeField]
    private decimal totalTime = 0;
    public decimal TotalTime
    {
        get => totalTime;
        set => totalTime = value;
    }

    private int dayBefore = 0;
    #endregion

    #region UI相关
    public void UpdateUI()
    {
        UpdateDate();
        UpdateTime();
    }
    private void UpdateDate()
    {
        if (UI && UI.days) UI.days.text = "第" + Days + "天";
        if (UI && UI.dayOfDate) UI.dayOfDate.text = DayOfMonth + "日";
        if (UI && UI.date) UI.date.text = DateString;
        if (UI && UI.years) UI.years.text = "第" + Years + "年";
    }
    private void UpdateTime()
    {
        if (UI && UI.time) UI.time.text = TimeString;
    }
    #endregion

    #region 逻辑
    public void TimePase(decimal realSecond)
    {
        OnTimePassed?.Invoke(realSecond);
        totalTime += realSecond;
        timeline = (float)(totalTime * multiples / HourToSeconds % 24);
        CheckDayChange();
        UpdateTime();
    }
    private void CheckDayChange()
    {
        int dayNow = Days;
        if (dayBefore != dayNow)
        {
            UpdateDate();
            NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.DayChange, dayBefore, dayNow);
            dayBefore = dayNow;
        }
    }

    public void SaveData(SaveData data)
    {
        data.totalTime = TotalTime;
    }
    public void LoadData(SaveData data)
    {
        totalTime = data.totalTime;
        SetTime(data.totalTime);
    }

    private void SetTime(decimal totalTime)
    {
        ResetTime();
        TimePase(totalTime);
    }
    private void ResetTime()
    {
        totalTime = 0;
        timeline = 0;
        dayBefore = 0;
        UpdateUI();
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        UpdateUI();
        Timeline = timeline;
        Date.month = CurrentMonth;
        Date.dayOfMonth = DayOfMonth;
        Date.dayOfWeek = DayOfWeek;
        CheckDayChange();
    }

    private void Update()
    {
        TimePase((decimal)Time.deltaTime);
    }
    #endregion

    [Serializable]
    public class DateInfo
    {
        public Month month;
        public int dayOfMonth;
        public DayOfWeek dayOfWeek;

        public string GetMonthString(TimeSystem timeSystem)
        {
            string monthString = (int)month + "月";
            if (timeSystem == TimeSystem.Twelve)
            {
                switch (month)
                {
                    case Month.January: monthString = "正月"; break;
                    case Month.February: monthString = "二月"; break;
                    case Month.March: monthString = "三月"; break;
                    case Month.April: monthString = "四月"; break;
                    case Month.May: monthString = "五月"; break;
                    case Month.June: monthString = "六月"; break;
                    case Month.July: monthString = "七月"; break;
                    case Month.August: monthString = "八月"; break;
                    case Month.September: monthString = "九月"; break;
                    case Month.October: monthString = "十月"; break;
                    case Month.November: monthString = "冬月"; break;
                    case Month.Decamber: monthString = "腊月"; break;
                    default: monthString = string.Empty; break;
                }
            }
            return monthString;
        }

        public string GetDayOfMonthString(TimeSystem timeSystem)
        {
            string dayString = dayOfMonth + "日";
            if (timeSystem == TimeSystem.Twelve)
            {
                int dayOfDate = dayOfMonth;
                if (dayOfDate < 20)
                    switch (dayOfDate)
                    {
                        case 1: dayString = "初一"; break;
                        case 2: dayString = "初二"; break;
                        case 3: dayString = "初三"; break;
                        case 4: dayString = "初四"; break;
                        case 5: dayString = "初五"; break;
                        case 6: dayString = "初六"; break;
                        case 7: dayString = "初七"; break;
                        case 8: dayString = "初八"; break;
                        case 9: dayString = "初九"; break;
                        case 10: dayString = "初十"; break;
                        case 11: dayString = "十一"; break;
                        case 12: dayString = "十二"; break;
                        case 13: dayString = "十三"; break;
                        case 14: dayString = "十四"; break;
                        case 15: dayString = "十五"; break;
                        case 16: dayString = "十六"; break;
                        case 17: dayString = "十七"; break;
                        case 18: dayString = "十八"; break;
                        case 19: dayString = "十九"; break;
                        case 20: dayString = "二十"; break;
                        default: dayString = string.Empty; break;
                    }
                else if (dayOfDate > 20 && dayOfDate < 30)
                    switch (dayOfDate)
                    {
                        case 21: dayString = "廿一"; break;
                        case 22: dayString = "廿二"; break;
                        case 23: dayString = "廿三"; break;
                        case 24: dayString = "廿四"; break;
                        case 25: dayString = "廿五"; break;
                        case 26: dayString = "廿六"; break;
                        case 27: dayString = "廿七"; break;
                        case 28: dayString = "廿八"; break;
                        case 29: dayString = "廿九"; break;
                        default: dayString = string.Empty; break;
                    }
                else if (dayOfDate == 30) dayString = "三十";
            }
            return dayString;
        }

        public string GetWeekDayString(TimeSystem timeSystem)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return timeSystem == TimeSystem.Twelve ? "月曜" : "星期一";
                case DayOfWeek.Tuesday:
                    return timeSystem == TimeSystem.Twelve ? "金曜" : "星期二";
                case DayOfWeek.Wednesday:
                    return timeSystem == TimeSystem.Twelve ? "木曜" : "星期三";
                case DayOfWeek.Thursday:
                    return timeSystem == TimeSystem.Twelve ? "水曜" : "星期四";
                case DayOfWeek.Friday:
                    return timeSystem == TimeSystem.Twelve ? "火曜" : "星期五";
                case DayOfWeek.Saturday:
                    return timeSystem == TimeSystem.Twelve ? "土曜" : "星期六";
                case DayOfWeek.Sunday:
                default:
                    return timeSystem == TimeSystem.Twelve ? "日曜" : "星期日";
            }
        }

        public string ToString(TimeSystem timeSystem = TimeSystem.System24)
        {
            return GetMonthString(timeSystem) + GetDayOfMonthString(timeSystem) + " " + GetWeekDayString(timeSystem);
        }
    }
}