using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmHandler : MonoBehaviour, IWindow
{
    private static ConfirmHandler instance;
    public static ConfirmHandler Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<ConfirmHandler>();
            return instance;
        }
    }

    [SerializeField]
    private CanvasGroup confirmWindow;

    [HideInInspector]
    private Canvas windowCanvas;

    [SerializeField]
    private Text dialogText;

    [SerializeField]
    private Button yes;

    [SerializeField]
    private Button no;

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
        no.onClick.AddListener(CloseWindow);
        if (!confirmWindow.GetComponent<GraphicRaycaster>()) confirmWindow.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = confirmWindow.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
    }

    public void NewConfirm(string dialog, UnityAction yesAction = null)
    {
        dialogText.text = dialog;
        yes.onClick.RemoveAllListeners();
        if (yesAction != null) yes.onClick.AddListener(yesAction);
        yes.onClick.AddListener(CloseWindow);
        OpenWindow();
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
        yes.onClick.RemoveAllListeners();
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
