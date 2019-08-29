using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalendarUI : MonoBehaviour
{
    public CanvasGroup calendarWindow;

    [HideInInspector]
    public Canvas windowCanvas;

    public Text month;
    public Text season;

    public DateAgent dateCellPrefab;
    public Transform dateCellsParent;

    public Button close;

    private void Awake()
    {
        if (!calendarWindow.GetComponent<GraphicRaycaster>()) calendarWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = calendarWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        close.onClick.AddListener(CalendarManager.Instance.CloseWindow);
    }
}
