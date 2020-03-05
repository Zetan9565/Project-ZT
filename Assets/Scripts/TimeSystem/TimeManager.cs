using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/时间管理器")]
public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    #region 静态属性
    /// <summary>
    /// 一个游戏分在现实中的耗时
    /// </summary>
    public static float OneMinute => Instance ? Instance.Scale : 1;
    /// <summary>
    /// 一个游戏时在现实中的耗时
    /// </summary>
    public static float OneHour => OneMinute * 60;
    /// <summary>
    /// 一个游戏日在现实中的耗时
    /// </summary>
    public static float OneDay => OneHour * 24;
    /// <summary>
    /// 一个游戏月在现实中的耗时
    /// </summary>
    public static float OneMonth => OneDay * 30;
    /// <summary>
    /// 一个游戏季在现实中的耗时
    /// </summary>
    public static float OneSeason => OneMonth * 3;
    /// <summary>
    /// 一个游戏年在现实中的耗时
    /// </summary>
    public static float OneYear => OneSeason * 4;
    #endregion

    [SerializeField]
    private float timeline = 8;
    public float Timeline
    {
        get
        {
            return timeline;
        }
    }

    public float NormalizeTimeline => Timeline / 24;

    [SerializeField]
    [Tooltip("现实中的 1 秒折合游戏中的多少分钟？")]
    private float multiples = 1;
    public float Multiples
    {
        get => multiples;
        set { multiples = value; }
    }

    private float Scale => multiples / 60;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("12小时制", "24小时制", "十二时辰")]
#endif
    private TimeSystem timeSystem = TimeSystem.System24;
    public TimeSystem TimeSystem
    {
        get
        {
            return timeSystem;
        }
    }

    public delegate void TimePassedListner(int second);
    public event TimePassedListner OnTimePassed;

    public delegate void DayPassedListner();
    public event DayPassedListner OnDayPassed;

    public string Time
    {
        get
        {
            switch (TimeSystem)
            {
                case TimeSystem.System12:
                    return string.Format("{0}:{1} {2}", (int)timeline % 12 == 0 ? "12" : ((int)timeline % 12 + ""),
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

    [SerializeField]
    private TimeUI UI;

    [SerializeField]
    private Month currentMonth = Month.January;
    //public DayOfTheWeek DayOfWeekOfFirstDayOfCurrentMonth { get; private set; } = DayOfTheWeek.Sunday;
    public DayOfTheWeek WeekDayOfTheFirstDayOfCurrentMonth
    {
        get
        {
            int daysOfMonth = DaysOfMonth;
            int dayOfTheWeek = (int)DayOfTheWeek;
            for (int i = daysOfMonth - 1; i > 0; i--)
            {
                dayOfTheWeek--;
                if (dayOfTheWeek < 0) dayOfTheWeek = 6;
            }
            return (DayOfTheWeek)dayOfTheWeek;
        }
    }
    public Seaon CurrentSeason => MonthToSeason(CurrentMonth);
    [SerializeField]
    private int days = 1;
    [SerializeField]
    private int daysOfYear = 1;
    [SerializeField]
    private int weeks = 1;
    [SerializeField]
    private int months = 1;
    [SerializeField]
    private int years = 1;

    public string DateString
    {
        get
        {
            Date.month = CurrentMonth;
            Date.daysOfMonth = DaysOfMonth;
            Date.dayOfTheWeek = DayOfTheWeek;
            return Date.ToString(timeSystem);
        }
    }
    public DateInfo Date { get; private set; } = new DateInfo();
    public int DaysOfMonth => Days % 30 == 0 ? 30 : Days % 30;
    public DayOfTheWeek DayOfTheWeek
    {
        get
        {
            int days = Days % 7;
            if (days == 0) return DayOfTheWeek.Saturday;
            else return (DayOfTheWeek)(days - 1);
        }
    }
    public int DaysOfYear => daysOfYear;
    public int Days
    {
        get
        {
            return days;
        }

        private set
        {
            days = value;
            Date.daysOfMonth = DaysOfMonth;
            Date.dayOfTheWeek = DayOfTheWeek;
            daysOfYear = days % 360 == 0 ? 360 : days % 360;
            if (UI && UI.days) UI.days.text = "第 " + days + " 天";
            if (UI && UI.dayOfDate) UI.dayOfDate.text = DaysOfMonth + "日";
            if (UI && UI.date) UI.date.text = DateString;
            CalendarManager.Instance.UpdateToday();
        }
    }//从1开始计
    public int Weeks
    {
        get
        {
            return weeks;
        }

        private set
        {
            weeks = value;
        }
    }//从1开始计
    public int Months//从1开始计
    {
        get
        {
            return months;
        }

        set
        {
            months = value;
            if (UI && UI.months) UI.months.text = "第" + months + "月";
        }
    }
    public int Years
    {
        get
        {
            return years;
        }

        private set
        {
            if (UI && UI.years) UI.years.text = "第" + Years + "年";
            years = value;
        }
    }//从1开始计

    public Month CurrentMonth
    {
        get
        {
            return currentMonth;
        }

        private set
        {
            currentMonth = value;
            Date.month = currentMonth;
            if (UI && UI.monthOfDate) UI.monthOfDate.text = Date.GetMonthString(TimeSystem);
            if (UI && UI.date) UI.date.text = DateString;
        }
    }

    private void Awake()
    {
        UpdateUI();
        Date.month = CurrentMonth;
        Date.daysOfMonth = DaysOfMonth;
        Date.dayOfTheWeek = DayOfTheWeek;
        CalendarManager.Instance.UpdateMonth();
    }

    public void UpdateUI()
    {
        if (UI && UI.days) UI.days.text = "第" + Days + "天";
        if (UI && UI.dayOfDate) UI.dayOfDate.text = DaysOfMonth + "日";
        if (UI && UI.date) UI.date.text = DateString;
        if (UI && UI.years) UI.years.text = "第" + Years + "年";
    }

    private void Update()
    {
        float timelineBef = timeline;
        timeline = (timeline + UnityEngine.Time.deltaTime * Scale) % 24;
        if (UI && UI.time) UI.time.text = Time;
        if (timelineBef > timeline) NextDay();
    }

    public void TimePase(int second)
    {
        float timelineBef = timeline;
        int days = (int)(second * Scale / 24f);
        for (int i = 0; i < days - 1; i++)
            NextDay();
        timeline = (timeline + second * Scale) % 24;
        if (UI && UI.time) UI.time.text = Time;
        if (timelineBef > timeline) NextDay();
        OnTimePassed?.Invoke(second);
    }

    private void NextDayWithoutNotify()
    {
        Debug.Log("一天过去了");
        Days++;
        if (Days % 7 == 1)
        {
            Debug.Log("一周过去了");
            Weeks++;
        }
        if (Days % 30 == 1)
        {
            Debug.Log("一月过去了");
            int monthIndex = Mathf.CeilToInt((Days + 1) * 1.0f / 30);
            monthIndex = monthIndex % 12 == 0 ? 12 : monthIndex % 12;
            CurrentMonth = (Month)monthIndex;
            Date.month = CurrentMonth;
            //int dayOfWeekOfFirstDayOfCurrentMonth = (int)DayOfTheWeek + 1;
            //if (dayOfWeekOfFirstDayOfCurrentMonth > 6) dayOfWeekOfFirstDayOfCurrentMonth = 0;
            //DayOfWeekOfFirstDayOfCurrentMonth = (DayOfTheWeek)dayOfWeekOfFirstDayOfCurrentMonth;
            CalendarManager.Instance.UpdateMonth();
        }
        if (Days % 360 == 1)
        {
            Debug.Log("一年过去了");
            Years++;
        }
    }

    private void NextDay()
    {
        NextDayWithoutNotify();
        OnDayPassed?.Invoke();
    }

    public static string WeekDayToString(DayOfTheWeek DayOfTheWeek, TimeSystem timeSystem = TimeSystem.System24)
    {
        DayOfTheWeek day = DayOfTheWeek;
        switch (day)
        {
            case DayOfTheWeek.Monday:
                return timeSystem == TimeSystem.Twelve ? "月曜" : "星期一";
            case DayOfTheWeek.Tuesday:
                return timeSystem == TimeSystem.Twelve ? "金曜" : "星期二";
            case DayOfTheWeek.Wednesday:
                return timeSystem == TimeSystem.Twelve ? "木曜" : "星期三";
            case DayOfTheWeek.Thursday:
                return timeSystem == TimeSystem.Twelve ? "水曜" : "星期四";
            case DayOfTheWeek.Friday:
                return timeSystem == TimeSystem.Twelve ? "火曜" : "星期五";
            case DayOfTheWeek.Saturday:
                return timeSystem == TimeSystem.Twelve ? "土曜" : "星期六";
            case DayOfTheWeek.Sunday:
            default:
                return timeSystem == TimeSystem.Twelve ? "日曜" : "星期日";
        }
    }

    public static Seaon MonthToSeason(Month month)
    {
        switch (month)
        {
            case global::Month.January:
            case global::Month.February:
            case global::Month.March:
                return Seaon.Spring;
            case global::Month.April:
            case global::Month.May:
            case global::Month.June:
                return Seaon.Summer;
            case global::Month.July:
            case global::Month.August:
            case global::Month.September:
                return Seaon.Autumn;
            case global::Month.October:
            case global::Month.November:
            case global::Month.Decamber:
            default:
                return Seaon.Winter;
        }
    }

    [System.Serializable]
    public class DateInfo
    {
        public Month month;
        public int daysOfMonth;
        public DayOfTheWeek dayOfTheWeek;

        public string GetMonthString(TimeSystem timeSystem)
        {
            string monthString = (int)month + "月";
            if (timeSystem == TimeSystem.Twelve)
            {
                switch (month)
                {
                    case global::Month.January: monthString = "正月"; break;
                    case global::Month.February: monthString = "二月"; break;
                    case global::Month.March: monthString = "三月"; break;
                    case global::Month.April: monthString = "四月"; break;
                    case global::Month.May: monthString = "五月"; break;
                    case global::Month.June: monthString = "六月"; break;
                    case global::Month.July: monthString = "七月"; break;
                    case global::Month.August: monthString = "八月"; break;
                    case global::Month.September: monthString = "九月"; break;
                    case global::Month.October: monthString = "十月"; break;
                    case global::Month.November: monthString = "冬月"; break;
                    case global::Month.Decamber: monthString = "腊月"; break;
                    default: monthString = string.Empty; break;
                }
            }
            return monthString;
        }

        public string GetDayOfMonthString(TimeSystem timeSystem)
        {
            string dayString = daysOfMonth + "日";
            if (timeSystem == TimeSystem.Twelve)
            {
                int dayOfDate = daysOfMonth;
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
                        default: dayString = string.Empty; break;
                    }
                else if (dayOfDate >= 20 && dayOfDate < 30)
                    switch (dayOfDate)
                    {
                        case 20: dayString = "廿十"; break;
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
            DayOfTheWeek day = dayOfTheWeek;
            switch (day)
            {
                case DayOfTheWeek.Monday:
                    return timeSystem == TimeSystem.Twelve ? "月曜" : "星期一";
                case DayOfTheWeek.Tuesday:
                    return timeSystem == TimeSystem.Twelve ? "金曜" : "星期二";
                case DayOfTheWeek.Wednesday:
                    return timeSystem == TimeSystem.Twelve ? "木曜" : "星期三";
                case DayOfTheWeek.Thursday:
                    return timeSystem == TimeSystem.Twelve ? "水曜" : "星期四";
                case DayOfTheWeek.Friday:
                    return timeSystem == TimeSystem.Twelve ? "火曜" : "星期五";
                case DayOfTheWeek.Saturday:
                    return timeSystem == TimeSystem.Twelve ? "土曜" : "星期六";
                case DayOfTheWeek.Sunday:
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

public enum TimeSystem
{
    System12,
    System24,
    Twelve
}

public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month,
    Season,
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

public enum DayOfTheWeek
{
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
}