using UnityEngine;
using UnityEngine.UI;

public class CalendarUI : WindowUI
{
    public Text month;
    public Text season;

    public DateAgent dateCellPrefab;
    public Transform dateCellsParent;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(CalendarManager.Instance.CloseWindow);
    }
}
