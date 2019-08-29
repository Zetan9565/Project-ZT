using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalendarManager : SingletonMonoBehaviour<CalendarManager>, IWindowHandler
{
    [SerializeField]
    private CalendarUI UI;

    private readonly List<DateAgent> dateAgents = new List<DateAgent>();

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            if (UI) return UI.windowCanvas;
            else return null;
        }
    }

    public void Init()
    {
        int countBef = dateAgents.Count;
        for (int i = 0; i < 42 - countBef; i++)
        {
            DateAgent da = Instantiate(UI.dateCellPrefab, UI.dateCellsParent);
            dateAgents.Add(da);
        }
    }

    public void UpdateUI()
    {
        UpdateMonth();
        UpdateToday();
    }

    public void UpdateMonth()
    {
        if (!UI || !UI.gameObject || !IsUIOpen || !TimeManager.Instance) return;
        Init();
        foreach (DateAgent da in dateAgents)
            da.Empty();
        int startIndex = (int)TimeManager.Instance.WeekDayOfTheFirstDayOfCurrentMonth;
        for (int i = 1; i < 31; i++)
        {
            int index = i + startIndex - 1;
            dateAgents[index].Init(i, (index + 1) % 7 == 0 || index % 7 == 0 ? Color.red : Color.black);
        }
        for (int i = startIndex - 1; i >= 0; i--)
            dateAgents[i].Init(30 - (startIndex - 1 - i), Color.gray);
        for (int i = startIndex + 30; i < 42; i++)
            dateAgents[i].Init(1 + i - (startIndex + 30), Color.gray);
        UI.month.text = TimeManager.Instance.Date.GetMonthString(TimeManager.Instance.TimeSystem);
    }
    public void UpdateToday()
    {
        if (!UI || !UI.gameObject || !IsUIOpen || !TimeManager.Instance) return;
        using (var dateAgentEnum = dateAgents.GetEnumerator())
            while (dateAgentEnum.MoveNext())
                dateAgentEnum.Current.SetToday(false);
        int startIndex = (int)TimeManager.Instance.WeekDayOfTheFirstDayOfCurrentMonth;
        int todayIndex = TimeManager.Instance.DaysOfMonth + startIndex - 1;
        if (todayIndex > -1 && todayIndex < dateAgents.Count) dateAgents[todayIndex].SetToday(true);
    }

    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        if (IsUIOpen) return;
        UpdateUI();
        UI.calendarWindow.alpha = 1;
        UI.calendarWindow.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        if (!IsUIOpen) return;
        UI.calendarWindow.alpha = 0;
        UI.calendarWindow.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
    }

    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen)
            OpenWindow();
        else CloseWindow();
    }

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.calendarWindow.alpha = 1;
            UI.calendarWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.calendarWindow.alpha = 0;
            UI.calendarWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void SetUI(CalendarUI UI)
    {
        dateAgents.RemoveAll(x => !x || !x.gameObject);
        CloseWindow();
        this.UI = UI;
        Init();
    }
}
