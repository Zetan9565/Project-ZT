using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class WindowsManager : SingletonMonoBehaviour<WindowsManager>
{
    [SerializeField]
    private int startSortingOrder = 100;
    public static int StartSortingOrder
    {
        get => Instance ? Instance.startSortingOrder : 0;
        set
        {
            if (Instance)
            {
                if (Instance.startSortingOrder != value)
                {
                    Instance.startSortingOrder = value;
                    for (int i = 0; i < Instance.windows.Count; i++)
                    {
                        Instance.windows[i].WindowCanvas.sortingOrder = i + Instance.startSortingOrder;
                    }
                }
            }
        }
    }

    public static int Count => Instance ? Instance.windows.Count : 0;

    private readonly Dictionary<string, Window> caches = new Dictionary<string, Window>();
    private readonly Dictionary<string, IHideable> hideableCache = new Dictionary<string, IHideable>();
    private readonly Dictionary<IHideable, bool> windowHideState = new Dictionary<IHideable, bool>();

    #region 开启窗口
    public static Window OpenWindow(string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return null;
    }
    /// <summary>
    /// 打开窗口，参数格式详见<typeparamref name="T"/>.OnOpen()
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>成功打开的窗口</returns>
    public static T OpenWindow<T>(params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return null;
    }
    public static Window OpenWindowBy(object openBy, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.OpenBy(openBy, args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return null;
    }
    /// <summary>
    /// 通过某对象打开窗口，发起打开的对象保留到此窗口关闭，参数格式详见<typeparamref name="T"/>.OnOpen()
    /// </summary>
    /// <param name="openBy">发起打开的对象</param>
    /// <param name="args">变长参数</param>
    /// <returns>成功打开的窗口</returns>
    public static T OpenWindowBy<T>(object openBy, params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            if (window.OpenBy(openBy, args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return null;
    }
    public static Window OpenWindowWithAction(Action onOpen, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
            {
                onOpen?.Invoke();
                return window;
            }
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return null;
    }

    public static void Clear()
    {
        if (!Instance) return;
        CloseAll();
        Instance.windowHideState.Clear();
        Instance.windows.Clear();
    }

    /// <summary>
    /// 打开窗口，并在打开后执行回调，参数格式详见<typeparamref name="T"/>.OnOpen()
    /// </summary>
    /// <param name="onOpen">成功打开时回调</param>
    /// <param name="args">变长参数</param>
    /// <returns>成功打开的窗口</returns>
    public static T OpenWindowWithAction<T>(Action onOpen, params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
            {
                onOpen?.Invoke();
                return window;
            }
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return null;
    }
    public static void OpenClose(string name)
    {
        var window = FindWindow(name);
        if (!window) return;
        if (window.IsOpen) window.Close();
        else window.Open();
    }
    public static void OpenClose<T>() where T : Window
    {
        var window = FindWindow<T>();
        if (!window) return;
        if (window.IsOpen) window.Close();
        else window.Open();
    }
    public static T UnhideOrOpenWindow<T>() where T : Window, IHideable
    {
        if (IsWindowHidden<T>(out var window))
        {
            HideWindow(window, false);
            return window;
        }
        else return OpenWindow<T>();
    }
    #endregion

    #region 关闭窗口
    public static bool CloseWindow(string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            return window.Close(args);
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return false;
    }
    /// <summary>
    /// 关闭窗口，参数格式详见<typeparamref name="T"/>.OnClose()
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public static bool CloseWindow<T>(params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            return window.Close(args);
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return false;
    }
    public static bool CloseWindowBy(object closeBy, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            return window.CloseBy(closeBy, args);
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return false;
    }
    /// <summary>
    /// 通过某对象关闭，发起关闭的对象保留到此窗口下次开启，参数格式详见<typeparamref name="T"/>.OnClose()
    /// </summary>
    /// <param name="closeBy">发起关闭的对象</param>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public static bool CloseWindowBy<T>(object closeBy, params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            return window.CloseBy(closeBy, args);
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return false;
    }
    public static bool CloseWindowWithAction(Action onClose, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (FindWindow(name) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Close(args))
            {
                onClose?.Invoke();
                return true;
            }
        }
        else Debug.LogWarning($"找不到类型为{name}的窗口");
        return false;
    }
    /// <summary>
    /// 关闭窗口，并在关闭后执行回调，参数格式详见<typeparamref name="T"/>.OnClose()
    /// </summary>
    /// <param name="onClose">成功关闭时回调</param>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public static bool CloseWindowWithAction<T>(Action onClose, params object[] args) where T : Window
    {
        if (FindWindow<T>() is T window)
        {
            if (args == null) args = new object[0];
            if (window.Close(args))
            {
                onClose?.Invoke();
                return true;
            }
        }
        else Debug.LogWarning($"找不到类型为{typeof(T).Name}的窗口");
        return false;
    }
    public static void CloseTop()
    {
        if (Pop() is Window window) window.Close();
    }
    public static void CloseAll()
    {
        if (!Instance) return;
        foreach (var window in Instance.windows)
        {
            window.Close();
        }
    }
    public static void CloseAllExceptName(params string[] exceptions)
    {
        if (!Instance) return;
        HashSet<string> names = new HashSet<string>(exceptions ?? new string[0]);
        foreach (var window in Instance.windows)
        {
            string name = window.GetType().Name;
            if (!names.Contains(name))
                window.Close();
        }
    }
    public static void CloseAllExceptType(params Type[] exceptions)
    {
        if (!Instance) return;
        HashSet<Type> types = new HashSet<Type>(exceptions ?? new Type[0]);
        foreach (var window in Instance.windows)
        {
            Type type = window.GetType();
            if (!types.Contains(type))
                window.Close();
        }
    }
    public static void CloseAllExcept(params Window[] exceptions)
    {
        if (!Instance) return;
        HashSet<Window> windows = new HashSet<Window>(exceptions ?? new Window[0]);
        foreach (var window in Instance.windows)
        {
            if (!windows.Contains(window))
                window.Close();
        }
    }
    #endregion

    #region 显隐窗口
    public static void HideWindow(string name, bool hide, params object[] args)
    {
        if (!Instance || string.IsNullOrEmpty(name)) return;
        if (!Instance.hideableCache.TryGetValue(name, out var window) || window == null)
            window = FindWindow(name) as IHideable;
        HideWindow(window, hide, args);
    }
    /// <summary>
    /// 隐藏窗口，参数格式详见<typeparamref name="T"/>.Hide()
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>成功打开的窗口</returns>
    public static void HideWindow<T>(bool hide, params object[] args) where T : Window, IHideable
    {
        if (!Instance) return;
        string name = typeof(T).Name;
        if (!Instance.hideableCache.TryGetValue(name, out var window) || window == null)
            window = FindWindow<T>();
        HideWindow(window, hide, args);
    }
    public static void HideWindow(IHideable window, bool hide, params object[] args)
    {
        if (!Instance) return;
        if (window != null && window.IsHidden != hide && (window as Window).IsOpen)
        {
            if (args == null) args = new object[0];
            window.Hide(hide, args);
            Instance.windowHideState[window] = hide;
            NotifyCenter.PostNotify(Window.WindowStateChanged, window.GetType().Name, hide ? WindowStates.Hidden : WindowStates.Shown);
        }
    }
    public static void HideAll(bool hide)
    {
        if (!Instance) return;
        foreach (var window in Instance.caches.Values)
        {
            if (window.IsOpen && window is IHideable hideable)
            {
                if (Instance.windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else Instance.windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
            }
        }
    }
    public static void HideAllExceptName(bool hide, params string[] exceptions)
    {
        if (!Instance) return;
        HashSet<string> names = new HashSet<string>(exceptions ?? new string[0]);
        foreach (var window in Instance.caches.Values)
        {
            string name = window.GetType().Name;
            if (!names.Contains(name) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (Instance.windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else Instance.windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, name, WindowStates.Hidden);
            }
        }
    }
    public static void HideAllExceptType(bool hide, params Type[] exceptions)
    {
        if (!Instance) return;
        HashSet<Type> types = new HashSet<Type>(exceptions ?? new Type[0]);
        foreach (var window in Instance.caches.Values)
        {
            Type type = window.GetType();
            if (!types.Contains(type) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (Instance.windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else Instance.windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, type.Name, WindowStates.Hidden);
            }
        }
    }
    public static void HideAllExcept(bool hide, params Window[] exceptions)
    {
        if (!Instance) return;
        HashSet<Window> windows = new HashSet<Window>(exceptions ?? (new Window[0]));
        foreach (var window in Instance.caches.Values)
        {
            if (!windows.Contains(window) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (Instance.windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else Instance.windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, window.GetType().Name, WindowStates.Hidden);
            }
        }
    }
    #endregion

    #region 查找相关
    public static Window FindWindow(string name)
    {
        if (!Instance || string.IsNullOrEmpty(name)) return null;
        if (Instance.caches.TryGetValue(name, out var window) && window) return window;
        else window = FindObjectOfType(ZetanUtility.GetTypeWithoutAssembly(name), true) as Window;
        Cache(window);
        return window;
    }
    public static T FindWindow<T>() where T : Window
    {
        if (!Instance) return null;
        string name = typeof(T).Name;
        if (Instance.caches.TryGetValue(name, out var window) && window) return window as T;
        else window = FindObjectOfType<T>(true);
        Cache(window);
        return window as T;
    }
    public static void Cache(Window window)
    {
        if (!Instance || !window) return;
        string name = window.GetType().Name;
        Instance.caches[name] = window;
        if (window is IHideable h) Instance.hideableCache[name] = h;
    }
    #endregion

    #region 判断相关
    public static bool IsWindowOpen(string name)
    {
        var window = FindWindow(name);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen<T>() where T : Window
    {
        var window = FindWindow<T>();
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen(string name, out Window window)
    {
        window = FindWindow(name);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen<T>(out T window) where T : Window
    {
        window = FindWindow<T>();
        if (!window) return false;
        else return window.IsOpen;
    }

    public static bool IsWindowHidden(string name)
    {
        if (FindWindow(name) is not IHideable window) return false;
        else return window.IsHidden;
    }
    public static bool IsWindowHidden<T>() where T : Window, IHideable
    {
        var window = FindWindow<T>();
        if (!window) return false;
        else return window.IsHidden;
    }
    public static bool IsWindowHidden(string name, out Window window)
    {
        window = FindWindow(name);
        if (window is not IHideable hideable) return false;
        else return hideable.IsHidden;
    }
    public static bool IsWindowHidden<T>(out T window) where T : Window, IHideable
    {
        window = FindWindow<T>();
        if (!window) return false;
        else return window.IsHidden;
    }
    #endregion

    #region 窗口栈结构
    private readonly List<Window> windows = new List<Window>();

    /// <summary>
    /// 压入窗口栈，仅供<see cref="Window.Open(object[])"/>内部使用，其它地方请勿使用
    /// </summary>
    /// <param name="window">窗口</param>
    public static void Push(Window window)
    {
        if (!Instance || !window) return;
        Remove(window);
        window.WindowCanvas.sortingOrder = Instance.windows.Count + Instance.startSortingOrder;
        Instance.windows.Add(window);
    }
    private static Window Pop()
    {
        var window = Peek();
        if (window) Remove(window);
        return window;
    }
    /// <summary>
    /// 返回开启且未隐藏的顶窗口
    /// </summary>
    /// <returns>开启且未隐藏的顶窗口</returns>
    public static Window Peek()
    {
        if (Instance)
            for (int i = Instance.windows.Count - 1; i >= 0; i--)
            {
                var window = Instance.windows[i];
                if (window is not IHideable hideable || !hideable.IsHidden)
                    return window;
            }
        return null;
    }
    /// <summary>
    /// 返回顶窗口
    /// </summary>
    /// <returns>顶窗口</returns>
    public static Window Top()
    {
        if (Instance && Instance.windows.Count > 0) return Instance.windows[^1];
        return null;
    }
    /// <summary>
    /// 从窗口栈移除，仅供<see cref="Window.Close(object[])"/>内部使用，其它地方请勿使用
    /// </summary>
    /// <param name="window">窗口</param>
    public static void Remove(Window window)
    {
        if (!Instance || !window) return;
        Instance.windows.Remove(window);
        if (window is IHideable hideable) Instance.windowHideState.Remove(hideable);
        for (int i = 0; i < Instance.windows.Count; i++)
        {
            Instance.windows[i].WindowCanvas.sortingOrder = i + Instance.startSortingOrder;
        }
    }
    #endregion
}