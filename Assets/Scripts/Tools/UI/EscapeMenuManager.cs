using UnityEngine;

public class EscapeMenuManager : SingletonMonoBehaviour<EscapeMenuManager>, IWindow
{
    [SerializeField]
    private EscapeUI UI;

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            return UI.menuCanvas;
        }
    }

    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        UI.escapeMenu.alpha = 1;
        UI.escapeMenu.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
        WindowsManager.Instance.PauseAll(true, this);
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.escapeMenu.alpha = 0;
        UI.escapeMenu.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
        WindowsManager.Instance.PauseAll(false);
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
            UI.escapeMenu.alpha = 1;
            UI.escapeMenu.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.escapeMenu.alpha = 0;
            UI.escapeMenu.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void SetUI(EscapeUI UI)
    {
        this.UI = UI;
    }

    public void ResetUI()
    {
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
    }

    public void Exit()
    {
        ConfirmManager.Instance.NewConfirm("确定退出" + Application.productName + "吗？", Application.Quit);
    }
}
