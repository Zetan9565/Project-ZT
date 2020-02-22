using UnityEngine;
using UnityEngine.UI;
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

    private Stack<IWindowHandler> Windows = new Stack<IWindowHandler>();

    public int WindowsCount { get { return Windows.Count; } }

    public void CloseTop()
    {
        if (Windows.Count < 1) return;
        IWindowHandler window;
        Stack<IWindowHandler> tempWins = new Stack<IWindowHandler>();//不受影响的窗口集
        while (WindowsCount > 0)
        {
            window = Windows.Pop();
            TopOrder--;
            if (window.IsUIOpen && !window.IsPausing)
            {
                window.CloseWindow();
                break;//此时已经关闭最顶层的可被关闭的窗口，就破开循环，接下去把不受影响的窗口放回去
            }
            else tempWins.Push(window);
        }
        while (tempWins.Count > 0)
            Push(tempWins.Pop());//重新把不受影响的窗口按新打开的方式放回去，使其有新的Sort Order
    }

    public void CloseAll(params IWindowHandler[] exceptions)
    {
        while (WindowsCount > 0)
        {
            if (!exceptions.Contains(Windows.Peek()))
                Windows.Pop().CloseWindow();
        }
    }

    public void PauseAll(bool pause, params IWindowHandler[] exceptions)
    {
        foreach (IWindowHandler window in Windows)
        {
            if (!exceptions.Contains(window))
                window.PauseDisplay(pause);
        }
        if (pause)
            UIManager.Instance.HideAll();
        else UIManager.Instance.ShowAll();
    }

    public void Push(IWindowHandler window)
    {
        Windows.Push(window);
        window.CanvasToSort.sortingOrder = TopOrder + 1;
        TopOrder = window.CanvasToSort.sortingOrder;
    }

    public void Remove(IWindowHandler window)
    {
        if (WindowsCount < 1) return;
        if (window == null || !Windows.Contains(window)) return;
        Stack<IWindowHandler> tempWins = new Stack<IWindowHandler>();//不受影响的窗口集
        while (Windows.Count > 0)
        {
            IWindowHandler tempWin = Windows.Pop();
            if (tempWin != window) tempWins.Push(tempWin);
            TopOrder--;
        }
        while (tempWins.Count > 0)
            Push(tempWins.Pop());//重新把不受影响的窗口按新打开的方式回去，使其有新的Sort Order
    }

    public void Clear()
    {
        Windows.Clear();
        topOrder = 0;
        Physics2D.BoxCast(Vector2.one, Vector2.one, 90, Vector2.zero);
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