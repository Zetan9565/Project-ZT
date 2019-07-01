﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmManager : SingletonMonoBehaviour<ConfirmManager>, IWindow
{
    [SerializeField]
    private CanvasGroup confirmWindow;

    [HideInInspector]
    private Canvas windowCanvas;

    [SerializeField]
    private Text dialogText;

    [SerializeField]
    private Button yes;

    private UnityEvent onYesClick = new UnityEvent();

    [SerializeField]
    private Button no;

    private UnityEvent onNoClick = new UnityEvent();

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            return windowCanvas;
        }
    }

    private void Awake()
    {
        yes.onClick.AddListener(Yes);
        no.onClick.AddListener(No);
        if (!confirmWindow.GetComponent<GraphicRaycaster>()) confirmWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = confirmWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
    }

    public void NewConfirm(string dialog, UnityAction yesAction = null, UnityAction noAction = null)
    {
        dialogText.text = dialog;
        onYesClick.RemoveAllListeners();
        onNoClick.RemoveAllListeners();
        if (yesAction != null) onYesClick.AddListener(yesAction);
        if (noAction != null) onNoClick.AddListener(noAction);
        OpenWindow();
    }

    private void Yes()
    {
        onYesClick?.Invoke();
        onYesClick.RemoveAllListeners();
        CloseWindow();
    }

    private void No()
    {
        onNoClick?.Invoke();
        onNoClick.RemoveAllListeners();
        CloseWindow();
    }

    public void OpenWindow()
    {
        if (IsUIOpen) return;
        if (IsPausing) return;
        confirmWindow.alpha = 1;
        confirmWindow.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
    }

    public void CloseWindow()
    {
        if (!IsUIOpen) return;
        if (IsPausing) return;
        confirmWindow.alpha = 0;
        confirmWindow.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
    }

    public void OpenCloseWindow()
    {

    }

    public void PauseDisplay(bool pause)
    {
        if (!IsUIOpen) return;
        if (!pause)
        {
            confirmWindow.alpha = 1;
            confirmWindow.blocksRaycasts = true;
        }
        else
        {
            confirmWindow.alpha = 0;
            confirmWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
    }
}