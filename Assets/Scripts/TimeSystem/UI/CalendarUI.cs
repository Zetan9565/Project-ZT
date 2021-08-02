using UnityEngine;
using UnityEngine.UI;

public class CalendarUI : WindowUI
{
    public Text month;
    public Text season;

    public DateAgent dateCellPrefab;
    public Transform dateCellsParent;

    //仅用于测试，后期会删掉
    public Slider slider;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(CalendarManager.Instance.CloseWindow);
        slider.onValueChanged.AddListener(delegate (float value) { TimeManager.Instance.multiples = (int)value; });
    }
}
