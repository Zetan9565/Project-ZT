using System;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/时间管理器")]
public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    //**
    //设计思路：
    //一年固定360天，一月固定30天
    //**/

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
    public float ScaleMinuteToReal => 60 / multiples;
    /// <summary>
    /// 一个游戏时在现实中的耗时(秒)
    /// </summary>
    public float ScaleHourToReal => ScaleMinuteToReal * 60;
    /// <summary>
    /// 一个游戏日在现实中的耗时(秒)
    /// </summary>
    public float ScaleDayToReal => ScaleHourToReal * 24;
    /// <summary>
    /// 一个游戏月在现实中的耗时(秒)
    /// </summary>
    public float ScaleMonthToReal => ScaleDayToReal * 30;
    /// <summary>
    /// 一个游戏季在现实中的耗时(秒)
    /// </summary>
    public float ScaleSeasonToReal => ScaleMonthToReal * 3;
    /// <summary>
    /// 一个游戏年在现实中的耗时(秒)
    /// </summary>
    public float ScaleYearToReal => ScaleSeasonToReal * 4;

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

    [Tooltip("现实中的 1 秒折合游戏中的多少秒钟？")]
    public int multiples = 60;

    [SerializeField]
    private float timeline = 8;
    public float Timeline
    {
        get => timeline;
        set
        {
            timeline = value % 24;
            timeStamp = ((Days - 1) * DayToSeconds + (decimal)timeline * HourToSeconds) / multiples;
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
            timeStamp = ((Years - 1) * YearToSeconds + (value - 1) * DayToSeconds + (decimal)timeline * HourToSeconds) / multiples;
        }
    }
    public Month CurrentMonth
    {
        get
        {
            int monthIndex = Months % 12;
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
    private bool daysNeedUpdate;
    private int days;
    public int Days
    {
        get
        {
            if (daysNeedUpdate)
            {
                daysNeedUpdate = false;
                days = Mathf.CeilToInt((float)(timeStamp * multiples / DayToSeconds));
                days = days < 1 ? 1 : days;
            }
            return days;
        }
    }//从1开始计

    private bool weeksNeedUpdate;
    private int weeks;
    public int Weeks
    {
        get
        {
            if (weeksNeedUpdate)
            {
                weeksNeedUpdate = false;
                weeks = Mathf.CeilToInt((float)(timeStamp * multiples / WeekToSeconds));
                weeks = weeks < 1 ? 1 : weeks;
            }
            return weeks;
        }
    }//从1开始计

    private bool monthsNeedUpdate;
    private int months;
    public int Months
    {
        get
        {
            if (monthsNeedUpdate)
            {
                months = Mathf.CeilToInt((float)(timeStamp * multiples / MonthToSeconds));
                months = months < 1 ? 1 : months;
            }
            return months;
        }
    }//从1开始计

    private bool yearsNeedUpdate;
    public int years;
    public int Years
    {
        get
        {
            if (yearsNeedUpdate)
            {
                years = Mathf.CeilToInt((float)(timeStamp * multiples / YearToSeconds));
                years = years < 1 ? 1 : years;
            }
            return years;
        }
    }//从1开始计
    #endregion

    #region 运行时相关
    public float NormalizeTimeline => timeline / 24;

    [SerializeField]
    private decimal timeStamp = 0;
    public decimal TimeStamp
    {
        get => timeStamp;
        set
        {
            OnTimePassed?.Invoke(value - timeStamp);
            timeStamp = value;
        }
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

    #region 数据逻辑
    public void TimePase(decimal realSecond)
    {
        OnTimePassed?.Invoke(realSecond);
        timeStamp += realSecond;
        timeline =  Mathf.Repeat((float)timeStamp * multiples / HourToSeconds, 24);
        CheckDayChange();
        UpdateTime();
        SetDirty();
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
        data.totalTime = TimeStamp;
    }
    public void LoadData(SaveData data)
    {
        timeStamp = data.totalTime;
        SetTime(data.totalTime);
    }

    public int GetDaysSince(decimal timeStamp)
    {
        int days = (int)((this.timeStamp - timeStamp) / (decimal)ScaleDayToReal / DayToSeconds);
        return days;
    }

    public decimal GetRealTimeUntil(float timeline)
    {
        if (timeline == 0f) timeline = 24f;
        if (timeline > this.timeline)
            return (decimal)(DayToSeconds * (timeline - this.timeline) * ScaleDayToReal);
        else
            return (decimal)(DayToSeconds * (24f - this.timeline + timeline) * ScaleDayToReal);
    }

    public void SkipToday()
    {
        TimePase(GetRealTimeUntil(0f));
    }

    private void SetTime(decimal timeStamp)
    {
        ResetTime();
        TimePase(timeStamp);
    }
    private void ResetTime()
    {
        timeStamp = 0m;
        timeline = 0f;
        dayBefore = 0;
        UpdateUI();
    }
    private void SetDirty()
    {
        daysNeedUpdate = true;
        weeksNeedUpdate = true;
        monthsNeedUpdate = true;
        yearsNeedUpdate = true;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        SetDirty();
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
                monthString = month switch
                {
                    Month.January => "正月",
                    Month.February => "二月",
                    Month.March => "三月",
                    Month.April => "四月",
                    Month.May => "五月",
                    Month.June => "六月",
                    Month.July => "七月",
                    Month.August => "八月",
                    Month.September => "九月",
                    Month.October => "十月",
                    Month.November => "冬月",
                    Month.Decamber => "腊月",
                    _ => monthString,
                };
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
                    dayString = dayOfDate switch
                    {
                        1 => "初一",
                        2 => "初二",
                        3 => "初三",
                        4 => "初四",
                        5 => "初五",
                        6 => "初六",
                        7 => "初七",
                        8 => "初八",
                        9 => "初九",
                        10 => "初十",
                        11 => "十一",
                        12 => "十二",
                        13 => "十三",
                        14 => "十四",
                        15 => "十五",
                        16 => "十六",
                        17 => "十七",
                        18 => "十八",
                        19 => "十九",
                        20 => "二十",
                        _ => dayString,
                    };
                else if (dayOfDate > 20 && dayOfDate < 30)
                    dayString = dayOfDate switch
                    {
                        21 => "廿一",
                        22 => "廿二",
                        23 => "廿三",
                        24 => "廿四",
                        25 => "廿五",
                        26 => "廿六",
                        27 => "廿七",
                        28 => "廿八",
                        29 => "廿九",
                        _ => dayString,
                    };
                else if (dayOfDate == 30) dayString = "三十";
            }
            return dayString;
        }

        public string GetWeekDayString(TimeSystem timeSystem)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => timeSystem == TimeSystem.Twelve ? "月曜" : "星期一",
                DayOfWeek.Tuesday => timeSystem == TimeSystem.Twelve ? "金曜" : "星期二",
                DayOfWeek.Wednesday => timeSystem == TimeSystem.Twelve ? "木曜" : "星期三",
                DayOfWeek.Thursday => timeSystem == TimeSystem.Twelve ? "水曜" : "星期四",
                DayOfWeek.Friday => timeSystem == TimeSystem.Twelve ? "火曜" : "星期五",
                DayOfWeek.Saturday => timeSystem == TimeSystem.Twelve ? "土曜" : "星期六",
                _ => timeSystem == TimeSystem.Twelve ? "日曜" : "星期日",
            };
        }

        public string ToString(TimeSystem timeSystem = TimeSystem.System24)
        {
            return GetMonthString(timeSystem) + GetDayOfMonthString(timeSystem) + " " + GetWeekDayString(timeSystem);
        }
    }
}