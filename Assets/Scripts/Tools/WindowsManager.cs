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

    private Stack<IOpenCloseable> Windows = new Stack<IOpenCloseable>();

    public void CloseTopWindow()
    {
        IOpenCloseable window;
        while (Windows.Count > 0)
        {
            window = Windows.Peek();
            if (window.IsUIOpen && !window.IsPausing)
            {
                window.CloseUI();
                Windows.Pop();
                break;
            }
        }
    }

    public void CloseAllWindows(params IOpenCloseable[] exceptions)
    {
        while (Windows.Count > 0)
        {
            if (!exceptions.Contains(Windows.Peek()))
                Windows.Pop().CloseUI();
        }
    }

    public void PauseAllWindows(bool state, params IOpenCloseable[] exceptions)
    {
        foreach (IOpenCloseable window in Windows)
        {
            if (!exceptions.Contains(window))
                window.PauseDisplay(state);
        }
    }

    public void PushWindow(IOpenCloseable window)
    {
        Windows.Push(window);
    }
}