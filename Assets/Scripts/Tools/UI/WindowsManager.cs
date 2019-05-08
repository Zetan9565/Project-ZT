using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WindowsManager : MonoBehaviour
{
    private static WindowsManager instance;
    public static WindowsManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<WindowsManager>();
            return instance;
        }
    }

    [SerializeField]
    private int topOrder = 0;
    public int TopOrder
    {
        get
        {
            return topOrder;
        }
        set
        {
            if (value < 1) topOrder = 1;
            else topOrder = value;
        }
    }

    private Stack<IWindow> Windows = new Stack<IWindow>();

    public int WindowsCount { get { return Windows.Count; } }

    public void CloseTop()
    {
        if (Windows.Count < 1) return;
        IWindow window;
        Stack<IWindow> tempWins = new Stack<IWindow>();//不受影响的窗口集
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

    public void CloseAll(params IWindow[] exceptions)
    {
        while (WindowsCount > 0)
        {
            if (!exceptions.Contains(Windows.Peek()))
                Windows.Pop().CloseWindow();
        }
    }

    public void PauseAll(bool pause, params IWindow[] exceptions)
    {
        foreach (IWindow window in Windows)
        {
            if (!exceptions.Contains(window))
                window.PauseDisplay(pause);
        }
    }

    public void Push(IWindow window)
    {
        Windows.Push(window);
        window.SortCanvas.sortingOrder = TopOrder + 1;
        TopOrder = window.SortCanvas.sortingOrder;
    }

    public void Remove(IWindow window)
    {
        if (WindowsCount < 1) return;
        if (window == null || !Windows.Contains(window)) return;
        Stack<IWindow> tempWins = new Stack<IWindow>();//不受影响的窗口集
        while (Windows.Count > 0)
        {
            IWindow tempWin = Windows.Pop();
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
    }
}