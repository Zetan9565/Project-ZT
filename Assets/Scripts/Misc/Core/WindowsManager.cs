using System;
using System.Collections.Generic;
using UnityEngine;

public static class WindowsManager
{
    private static int startSortingOrder = 100;
    public static int StartSortingOrder
    {
        get => startSortingOrder;
        set
        {
            if (startSortingOrder != value)
            {
                startSortingOrder = value;
                for (int i = 0; i < windows.Count; i++)
                {
                    windows[i].WindowCanvas.sortingOrder = i + startSortingOrder;
                }
            }
        }
    }

    public static int Count => windows.Count;

    private static readonly Dictionary<string, Window> caches = new Dictionary<string, Window>();
    private static readonly Dictionary<string, IHideable> hideableCache = new Dictionary<string, IHideable>();
    private static readonly Dictionary<IHideable, bool> windowHideState = new Dictionary<IHideable, bool>();

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
    public static Window OpenWindow(Type type, params object[] args)
    {
        if (type == null) return null;
        if (FindWindow(type) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{type}的窗口");
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
        else Debug.LogWarning($"找不到类型为{typeof(T)}的窗口");
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
    public static Window OpenWindowBy(object openBy, Type type, params object[] args)
    {
        if (type == null) return null;
        if (FindWindow(type) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.OpenBy(openBy, args))
                return window;
        }
        else Debug.LogWarning($"找不到类型为{type}的窗口");
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
        else Debug.LogWarning($"找不到类型为{typeof(T)}的窗口");
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
    public static Window OpenWindowWithAction(Action onOpen, Type type, params object[] args)
    {
        if (type == null) return null;
        if (FindWindow(type) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Open(args))
            {
                onOpen?.Invoke();
                return window;
            }
        }
        else Debug.LogWarning($"找不到类型为{type}的窗口");
        return null;
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
        else Debug.LogWarning($"找不到类型为{typeof(T)}的窗口");
        return null;
    }
    public static void OpenClose(string name)
    {
        var window = FindWindow(name, false);
        if (!window) window = FindWindow(name);
        if (!window) return;
        if (window.IsOpen) window.Close();
        else window.Open();
    }
    public static void OpenClose(Type type)
    {
        var window = FindWindow(type, false);
        if (!window) window = FindWindow(type);
        if (!window) return;
        if (window.IsOpen) window.Close();
        else window.Open();
    }
    public static void OpenClose<T>() where T : Window
    {
        var window = FindWindow<T>(false);
        if (!window) window = FindWindow<T>();
        if (!window) return;
        if (window.IsOpen) window.Close();
        else window.Open();
    }
    public static Window UnhideOrOpenWindow(string name)
    {
        if (IsWindowHidden(name, out var window))
        {
            HideWindow(window as IHideable, false);
            return window;
        }
        else return OpenWindow(name);
    }
    public static Window UnhideOrOpenWindow(Type type)
    {
        if (IsWindowHidden(type, out var window))
        {
            HideWindow(window as IHideable, false);
            return window;
        }
        else return OpenWindow(type);
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
        if (FindWindow(name, false) is Window window)
        {
            if (args == null) args = new object[0];
            return window.Close(args);
        }
        return false;
    }
    public static bool CloseWindow(Type type, params object[] args)
    {
        if (type == null) return false;
        if (FindWindow(type, false) is Window window)
        {
            if (args == null) args = new object[0];
            return window.Close(args);
        }
        return false;
    }
    /// <summary>
    /// 关闭窗口，参数格式详见<typeparamref name="T"/>.OnClose()
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public static bool CloseWindow<T>(params object[] args) where T : Window
    {
        if (FindWindow<T>(false) is T window)
        {
            if (args == null) args = new object[0];
            return window.Close(args);
        }
        return false;
    }
    public static bool CloseWindowBy(object closeBy, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (FindWindow(name, false) is Window window)
        {
            if (args == null) args = new object[0];
            return window.CloseBy(closeBy, args);
        }
        return false;
    }
    public static bool CloseWindowBy(object closeBy, Type type, params object[] args)
    {
        if (type == null) return false;
        if (FindWindow(type, false) is Window window)
        {
            if (args == null) args = new object[0];
            return window.CloseBy(closeBy, args);
        }
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
        if (FindWindow<T>(false) is T window)
        {
            if (args == null) args = new object[0];
            return window.CloseBy(closeBy, args);
        }
        return false;
    }
    public static bool CloseWindowWithAction(Action onClose, string name, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (FindWindow(name, false) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Close(args))
            {
                onClose?.Invoke();
                return true;
            }
        }
        return false;
    }
    public static bool CloseWindowWithAction(Action onClose, Type type, params object[] args)
    {
        if (type == null) return false;
        if (FindWindow(type, false) is Window window)
        {
            if (args == null) args = new object[0];
            if (window.Close(args))
            {
                onClose?.Invoke();
                return true;
            }
        }
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
        if (FindWindow<T>(false) is T window)
        {
            if (args == null) args = new object[0];
            if (window.Close(args))
            {
                onClose?.Invoke();
                return true;
            }
        }
        return false;
    }
    public static void CloseTop()
    {
        if (Pop() is Window window) window.Close();
    }
    public static void CloseAll()
    {
        foreach (var window in windows.ConvertAll(w => w))
        {
            window.Close();
        }
    }
    public static void CloseAllExceptName(params string[] exceptions)
    {
        HashSet<string> names = new HashSet<string>(exceptions ?? new string[0]);
        foreach (var window in windows)
        {
            string name = window.GetType().Name;
            if (!names.Contains(name))
                window.Close();
        }
    }
    public static void CloseAllExceptType(params Type[] exceptions)
    {
        HashSet<Type> types = new HashSet<Type>(exceptions ?? new Type[0]);
        foreach (var window in windows)
        {
            Type type = window.GetType();
            if (!types.Contains(type))
                window.Close();
        }
    }
    public static void CloseAllExcept(params Window[] exceptions)
    {
        HashSet<Window> windows = new HashSet<Window>(exceptions ?? new Window[0]);
        foreach (var window in windows)
        {
            if (!windows.Contains(window))
                window.Close();
        }
    }
    #endregion

    #region 显隐窗口
    public static void HideWindow(string name, bool hide, params object[] args)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!hideableCache.TryGetValue(name, out var window) || window == null)
            window = FindWindow(name, false) as IHideable;
        HideWindow(window, hide, args);
    }
    public static void HideWindow(Type type, bool hide, params object[] args)
    {
        if (type == null) return;
        if (!hideableCache.TryGetValue(type.Name, out var window) || window == null)
            window = FindWindow(type, false) as IHideable;
        HideWindow(window, hide, args);
    }
    /// <summary>
    /// 隐藏窗口，参数格式详见<typeparamref name="T"/>.Hide()
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>成功打开的窗口</returns>
    public static void HideWindow<T>(bool hide, params object[] args) where T : Window, IHideable
    {
        string name = typeof(T).Name;
        if (!hideableCache.TryGetValue(name, out var window) || window == null)
            window = FindWindow<T>(false);
        HideWindow(window, hide, args);
    }
    public static void HideWindow(IHideable window, bool hide, params object[] args)
    {
        if (window != null && window.IsHidden != hide && (window as Window).IsOpen)
        {
            if (args == null) args = new object[0];
            window.Hide(hide, args);
            windowHideState[window] = hide;
            NotifyCenter.PostNotify(Window.WindowStateChanged, window.GetType(), hide ? WindowStates.Hidden : WindowStates.Shown);
        }
    }
    public static void HideAll(bool hide)
    {
        foreach (var window in caches.Values)
        {
            if (window.IsOpen && window is IHideable hideable)
            {
                if (windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
            }
        }
    }
    public static void HideAllExceptName(bool hide, params string[] exceptions)
    {
        HashSet<string> names = new HashSet<string>(exceptions ?? new string[0]);
        foreach (var window in caches.Values)
        {
            string name = window.GetType().Name;
            if (!names.Contains(name) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, window.GetType(), WindowStates.Hidden);
            }
        }
    }
    public static void HideAllExceptType(bool hide, params Type[] exceptions)
    {
        HashSet<Type> types = new HashSet<Type>(exceptions ?? new Type[0]);
        foreach (var window in caches.Values)
        {
            Type type = window.GetType();
            if (!types.Contains(type) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, type, WindowStates.Hidden);
            }
        }
    }
    public static void HideAllExcept(bool hide, params Window[] exceptions)
    {
        HashSet<Window> windows = new HashSet<Window>(exceptions ?? (new Window[0]));
        foreach (var window in caches.Values)
        {
            if (!windows.Contains(window) && window.IsOpen && window is IHideable hideable && hideable.IsHidden != hide)
            {
                if (windowHideState.TryGetValue(hideable, out var isHiddenBef))
                {
                    if (isHiddenBef && !hide) continue;//HideAll()之前隐藏了，现在却不想隐藏，就不行
                }
                else windowHideState.Add(hideable, hideable.IsHidden);
                hideable.Hide(hide);
                NotifyCenter.PostNotify(Window.WindowStateChanged, window.GetType(), WindowStates.Hidden);
            }
        }
    }
    #endregion

    #region 查找相关
    public static Window FindWindow(string name)
    {
        return FindWindow(name, true);
    }
    private static Window FindWindow(string name, bool create)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (caches.TryGetValue(name, out var window) && window) return window;
        else return FindWindow(ZetanUtility.GetTypeByFullName(name), create);
    }
    public static Window FindWindow(Type type)
    {
        return FindWindow(type, true);
    }
    private static Window FindWindow(Type type, bool create)
    {
        if (type == null) return null;
        if (caches.TryGetValue(type.Name, out var window) && window) return window;
        else window = UnityEngine.Object.FindObjectOfType(type, true) as Window;
        if (!window && create)
        {
            if (WindowPrefabs.Instance.GetWindowPrefab(type) is Window prefab)
                window = UnityEngine.Object.Instantiate(prefab, UIManager.Instance.transform);
        }
        Cache(window);
        return window;
    }
    public static T FindWindow<T>() where T : Window
    {
        return FindWindow<T>(true);
    }
    private static T FindWindow<T>(bool create) where T : Window
    {
        string name = typeof(T).Name;
        if (caches.TryGetValue(name, out var window) && window) return window as T;
        else return FindWindow(typeof(T), create) as T;
    }
    public static void Cache(Window window)
    {
        if (!window) return;
        string name = window.GetType().Name;
        caches[name] = window;
        if (window is IHideable h) hideableCache[name] = h;
    }
    #endregion

    #region 判断相关
    public static bool IsWindowOpen(string name)
    {
        var window = FindWindow(name, false);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen(Type type)
    {
        var window = FindWindow(type, false);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen<T>() where T : Window
    {
        var window = FindWindow<T>(false);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen(string name, out Window window)
    {
        window = FindWindow(name, false);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen(Type type, out Window window)
    {
        window = FindWindow(type, false);
        if (!window) return false;
        else return window.IsOpen;
    }
    public static bool IsWindowOpen<T>(out T window) where T : Window
    {
        window = FindWindow<T>(false);
        if (!window) return false;
        else return window.IsOpen;
    }

    public static bool IsWindowHidden(string name)
    {
        if (FindWindow(name, false) is not IHideable window) return false;
        else return window.IsHidden;
    }
    public static bool IsWindowHidden(Type type)
    {
        if (FindWindow(type, false) is not IHideable window) return false;
        else return window.IsHidden;
    }
    public static bool IsWindowHidden<T>() where T : Window, IHideable
    {
        var window = FindWindow<T>(false);
        if (!window) return false;
        else return window.IsHidden;
    }
    public static bool IsWindowHidden(string name, out Window window)
    {
        window = FindWindow(name, false);
        if (window is not IHideable hideable) return false;
        else return hideable.IsHidden;
    }
    public static bool IsWindowHidden(Type type, out Window window)
    {
        window = FindWindow(type, false);
        if (window is not IHideable hideable) return false;
        else return hideable.IsHidden;
    }
    public static bool IsWindowHidden<T>(out T window) where T : Window, IHideable
    {
        window = FindWindow<T>(false);
        if (!window) return false;
        else return window.IsHidden;
    }
    #endregion

    #region 窗口栈结构
    private static readonly List<Window> windows = new List<Window>();

    /// <summary>
    /// 压入窗口栈，仅供<see cref="Window.Open(object[])"/>内部使用，其它地方请勿使用
    /// </summary>
    /// <param name="window">窗口</param>
    public static void Push(Window window)
    {
        if (!window) return;
        Remove(window);
        window.WindowCanvas.sortingOrder = windows.Count + startSortingOrder;
        windows.Add(window);
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
        for (int i = windows.Count - 1; i >= 0; i--)
        {
            var window = windows[i];
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
        if (windows.Count > 0) return windows[^1];
        return null;
    }
    /// <summary>
    /// 从窗口栈移除，仅供<see cref="Window.Close(object[])"/>内部使用，其它地方请勿使用
    /// </summary>
    /// <param name="window">窗口</param>
    public static void Remove(Window window)
    {
        if (!window) return;
        windows.Remove(window);
        if (window is IHideable hideable) windowHideState.Remove(hideable);
        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].WindowCanvas.sortingOrder = i + startSortingOrder;
        }
    }
    #endregion

    [InitMethod(int.MinValue)]
    public static void Init()
    {
        CloseAll();
        Clear();
        foreach (var window in UnityEngine.Object.FindObjectsOfType<Window>())
        {
            Cache(window);
        }
    }
    public static void Clear()
    {
        caches.Clear();
        hideableCache.Clear();
        windowHideState.Clear();
        windows.Clear();
    }
}