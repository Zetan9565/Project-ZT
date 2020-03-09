using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmManager : SingletonMonoBehaviour<ConfirmManager>, IWindowHandler
{
    [SerializeField]
    private CanvasGroup confirmWindow;

    [HideInInspector]
    private Canvas windowCanvas;

    [SerializeField]
    private Text dialogText;

    [SerializeField]
    private Button yes;

    private Action onYesClick;

    [SerializeField]
    private Button no;

    private Action onNoClick;

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public Canvas CanvasToSort
    {
        get
        {
            return windowCanvas;
        }
    }

    private void Awake()
    {
        yes.onClick.AddListener(Confirm);
        no.onClick.AddListener(Cancel);
        if (!confirmWindow.GetComponent<GraphicRaycaster>()) confirmWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = confirmWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        WindowsManager.Instance.OnPushWindow.AddListener(Top);
    }

    private void OnDestroy()
    {
        if (WindowsManager.Instance) WindowsManager.Instance.OnPushWindow.RemoveListener(Top);
    }

    public void New(string dialog)
    {
        dialogText.text = dialog;
        onYesClick = null;
        onNoClick = null;
        ZetanUtility.SetActive(no.gameObject, false);
        (this as IWindowHandler).OpenWindow();
    }

    public void New(string dialog, Action yesAction)
    {
        dialogText.text = dialog;
        onYesClick = yesAction;
        onNoClick = null;
        ZetanUtility.SetActive(no.gameObject, true);
        (this as IWindowHandler).OpenWindow();
    }

    public void New(string dialog, Action yesAction, Action noAction)
    {
        dialogText.text = dialog;
        onYesClick = yesAction;
        onNoClick = noAction;
        ZetanUtility.SetActive(no.gameObject, true);
        (this as IWindowHandler).OpenWindow();
    }

    private void Top()
    {
        if (IsUIOpen) WindowsManager.Instance.PushToTop(this);
    }

    public void Confirm()
    {
        onYesClick?.Invoke();
        CloseWindow();
    }

    public void Cancel()
    {
        onNoClick?.Invoke();
        CloseWindow();
    }

    void IWindowHandler.OpenWindow()
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
        onYesClick = null;
        onNoClick = null;
        confirmWindow.alpha = 0;
        confirmWindow.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
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
