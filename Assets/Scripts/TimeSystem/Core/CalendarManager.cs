using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/日历管理器")]
public class CalendarManager : WindowHandler<CalendarUI, CalendarManager>, IOpenCloseAbleWindow
{
    private readonly List<DateAgent> dateAgents = new List<DateAgent>();

    public void Init()
    {
        RemakeGrid();
        NotifyCenter.Instance.RemoveListener(this);
        NotifyCenter.Instance.AddListener(NotifyCenter.CommonKeys.DayChange, OnDayChange);
    }

    private void RemakeGrid()
    {
        int countBef = dateAgents.Count;
        for (int i = 0; i < 42 - countBef; i++)
        {
            DateAgent da = Instantiate(UI.dateCellPrefab, UI.dateCellsParent);
            dateAgents.Add(da);
        }
    }

    public void OnDayChange(params object[] msg)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateMonth();
        UpdateToday();
    }

    public void UpdateMonth()
    {
        if (!UI || !UI.gameObject || !IsUIOpen || !TimeManager.Instance) return;
        RemakeGrid();
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
        int todayIndex = TimeManager.Instance.DayOfMonth + startIndex - 1;
        if (todayIndex > -1 && todayIndex < dateAgents.Count) dateAgents[todayIndex].SetToday(true);
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        UpdateUI();
    }

    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) OpenWindow();
        else CloseWindow();
    }

    public override void SetUI(CalendarUI UI)
    {
        dateAgents.RemoveAll(x => !x || !x.gameObject);
        CloseWindow();
        base.SetUI(UI);
        Init();
    }
}
