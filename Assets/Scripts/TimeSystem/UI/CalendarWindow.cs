using UnityEngine;
using UnityEngine.UI;
using ZetanStudio.TimeSystem;
using ZetanStudio.TimeSystem.UI;

public class CalendarWindow : Window
{
    public Text month;
    public Text season;

    public DateGrid dateGrid;

    //仅用于测试，后期会删掉
    public Slider slider;

    private Month latestMonth;
    private (int, Color)[] dates = new (int, Color)[42];

    protected override void OnAwake()
    {
        slider.value = TimeManager.Instance.multiples;
        slider.onValueChanged.AddListener(value => TimeManager.Instance.multiples = (int)value);
    }

    protected override void RegisterNotify()
    {
        NotifyCenter.AddListener(NotifyCenter.CommonKeys.DayChanged, OnDayChange, this);
    }

    protected override bool OnOpen(params object[] args)
    {
        if (!TimeManager.Instance) return false;
        latestMonth = TimeManager.Instance.CurrentMonth;
        RefreshMonth();
        RefreshToday();
        return base.OnOpen(args);
    }

    private void RefreshMonth()
    {
        if (!TimeManager.Instance) return;
        month.text = TimeManager.Instance.Date.GetMonthString(TimeManager.Instance.ClockSystem);
        int startIndex = (int)TimeManager.Instance.FirstWeekDayOfCurrentMonth;//本月第一天的星期
        for (int i = 1; i < 31; i++)
        {
            int index = i + startIndex - 1;
            dates[index] = (i, (index + 1) % 7 == 0 || index % 7 == 0 ? Color.red : Color.black);
        }
        for (int i = startIndex - 1; i >= 0; i--)
            dates[i] = (30 - (startIndex - 1 - i), Color.gray);
        for (int i = startIndex + 30; i < 42; i++)
            dates[i] = (1 + i - (startIndex + 30), Color.gray);
        dateGrid.Refresh(dates);
    }
    private void RefreshToday()
    {
        if (!TimeManager.Instance) return;
        int startIndex = (int)TimeManager.Instance.FirstWeekDayOfCurrentMonth;
        int todayIndex = TimeManager.Instance.DayOfMonth + startIndex - 1;
        dateGrid.ForEach(x =>
        {
            x.IsSelected = x.Index == todayIndex;
        });
    }

    private void OnDayChange(object[] msg)
    {
        var currentMonth = TimeManager.Instance.CurrentMonth;
        if (latestMonth != currentMonth)
        {
            latestMonth = currentMonth;
            RefreshMonth();
        }
        RefreshToday();
    }
}