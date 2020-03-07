using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/UI窗口管理器")]
public class WindowsManager : SingletonMonoBehaviour<WindowsManager>
{
    private int topOrder = 0;
    public int TopOrder
    {
        get
        {
            return topOrder;
        }
        private set
        {
            if (value < 1) topOrder = 1;
            else topOrder = value;
        }
    }

    private readonly Stack<IWindowHandler> windowStack = new Stack<IWindowHandler>();

    private readonly Dictionary<IWindowHandler, bool> windowPauseState = new Dictionary<IWindowHandler, bool>();

    public UnityEvent OnPushWindow { get; private set; } = new UnityEvent();

    public int WindowsCount { get { return windowStack.Count; } }

    public void CloseTop()
    {
        if (windowStack.Count < 1) return;
        IWindowHandler topWin;
        Stack<IWindowHandler> tempWins = new Stack<IWindowHandler>();//不受影响的窗口集
        while (WindowsCount > 0)
        {
            topWin = windowStack.Pop();
            TopOrder--;
            if (topWin.IsUIOpen && !topWin.IsPausing)
            {
                topWin.CloseWindow();
                break;//此时已经关闭最顶层的可被关闭的窗口，就破开循环，接下去把不受影响的窗口放回去
            }
            else tempWins.Push(topWin);
        }
        while (tempWins.Count > 0)
            PushWithoutNotify(tempWins.Pop());//重新把不受影响的窗口按新打开的方式放回去，使其有新的Sort Order
    }

    public void PushToTop(IWindowHandler window)
    {
        Remove(window);
        PushWithoutNotify(window);
    }

    public void CloseAll(params IWindowHandler[] exceptions)
    {
        while (WindowsCount > 0)
        {
            if (!exceptions.Contains(windowStack.Peek()))
                windowStack.Pop().CloseWindow();
        }
    }

    public void PauseAll(bool pause, params IWindowHandler[] exceptions)
    {
        foreach (IWindowHandler window in windowStack)
        {
            if (!exceptions.Contains(window))
            {
                if (windowPauseState.TryGetValue(window, out var isPauingBef))
                {
                    if (isPauingBef && !pause) continue;//PauseAll()之前暂停了，现在却不想暂停，就不行
                }
                else windowPauseState.Add(window, window.IsPausing);
                window.PauseDisplay(pause);
            }
        }
        if (pause) UIManager.Instance.HideAll();
        else UIManager.Instance.ShowAll();
    }

    public void Push(IWindowHandler window)
    {
        PushWithoutNotify(window);
        OnPushWindow?.Invoke();
    }

    private void PushWithoutNotify(IWindowHandler window)
    {
        windowStack.Push(window);
        window.CanvasToSort.sortingOrder = TopOrder + 1;
        TopOrder = window.CanvasToSort.sortingOrder;
    }

    public void Remove(IWindowHandler window)
    {
        if (WindowsCount < 1) return;
        if (window == null || !windowStack.Contains(window)) return;
        Stack<IWindowHandler> tempWins = new Stack<IWindowHandler>();//不受影响的窗口集
        while (windowStack.Count > 0)
        {
            IWindowHandler topWin = windowStack.Pop();
            if (topWin != window) tempWins.Push(topWin);
            TopOrder--;
        }
        while (tempWins.Count > 0)
            PushWithoutNotify(tempWins.Pop());//重新把不受影响的窗口按新打开的方式回去，使其有新的Sort Order
        windowPauseState.Remove(window);
    }

    public void Clear()
    {
        windowStack.Clear();
        windowPauseState.Clear();
        topOrder = 0;
    }
}

public abstract class WindowHandler<UI_T, Mono_T> : SingletonMonoBehaviour<Mono_T>, IWindowHandler where UI_T : WindowUI where Mono_T : SingletonMonoBehaviour<Mono_T>
{
    [SerializeField]
    protected UI_T UI;

    public virtual bool IsUIOpen { get; protected set; }

    public virtual bool IsPausing { get; protected set; }

    public virtual Canvas CanvasToSort => UI ? UI.windowCanvas : null;

    public virtual void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return ;
        if (IsPausing) return;
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        WindowsManager.Instance.Remove(this);
        IsUIOpen = false;
    }

    public virtual void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
    }

    public virtual void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.window.alpha = 1;
            UI.window.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.window.alpha = 0;
            UI.window.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public virtual void SetUI(UI_T UI)
    {
        this.UI = UI;
    }
}

public abstract class WindowUI : MonoBehaviour
{
    public CanvasGroup window;

    [HideInInspector]
    public Canvas windowCanvas;

    public Button closeButton;

    protected virtual void Awake()
    {
        if (!window.gameObject.GetComponent<GraphicRaycaster>()) window.gameObject.AddComponent<GraphicRaycaster>();
        windowCanvas = window.GetComponent<Canvas>();
        windowCanvas.overrideSorting = true;
        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
    }
}