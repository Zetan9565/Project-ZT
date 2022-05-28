using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;

[DefaultExecutionOrder(-1)]
public abstract class Window : MonoBehaviour, IFadeAble<Window>
{
    public virtual string LKName => GetType().Name;

    [Label("淡入淡出")]
    public bool animated = true;
    [Label("持续时间"), HideIf("animated", false)]
    public float duration = 0.05f;

    [SerializeField, Label("窗体")]
    protected CanvasGroup content;
    [SerializeField, Label("关闭按钮")]
    protected Button closeButton;
    public Canvas WindowCanvas { get; protected set; }
    public virtual bool IsOpen { get; protected set; }

#pragma warning disable IDE1006 // 命名样式
    /// <summary>
    /// 最近一次发起<see cref="OpenBy(object, object[])"/>的对象
    /// </summary>
    public object openBy { get; protected set; }
    /// <summary>
    /// 最近一次发起<see cref="CloseBy(object, object[])"/>的对象
    /// </summary>
    public object closeBy { get; protected set; }
    /// <summary>
    /// 窗口关闭事件，每次绑定后只生效一次
    /// </summary>
    public event Action onClose;
#pragma warning restore IDE1006 // 命名样式

    public Window MonoBehaviour => this;
    public CanvasGroup FadeTarget => content;
    public Coroutine FadeCoroutine { get; set; }

    /// <summary>
    /// 打开窗口
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功打开</returns>
    public bool Open(params object[] args)
    {
        args ??= new object[0];
        if (OnOpen(args))
        {
            WindowsManager.Push(this);
            NotifyCenter.PostNotify(WindowStateChanged, GetType().Name, WindowStates.Open);
            IsOpen = true;
            closeBy = null;
            if (animated) IFadeAble<Window>.FadeTo(this, 1, duration, () => content.blocksRaycasts = true);
            else
            {
                content.alpha = 1;
                content.blocksRaycasts = true;
            }
            return true;
        }
        else return false;
    }
    /// <summary>
    /// 通过某对象打开，发起打开的对象保留到此窗口关闭
    /// </summary>
    /// <param name="openBy">发起打开的对象</param>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功打开</returns>
    public bool OpenBy(object openBy, params object[] args)
    {
        this.openBy = openBy;
        return Open(args);
    }
    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public bool Close(params object[] args)
    {
        args ??= new object[0];
        if (OnClose(args))
        {
            WindowsManager.Remove(this);
            NotifyCenter.PostNotify(WindowStateChanged, GetType().Name, WindowStates.Closed);
            IsOpen = false;
            onClose?.Invoke();
            onClose = null;
            openBy = null;
            if (animated)
            {
                content.blocksRaycasts = false;
                IFadeAble<Window>.FadeTo(this, 0, duration);
            }
            else
            {
                content.alpha = 0;
                content.blocksRaycasts = false;
            }
            return true;
        }
        else return false;
    }
    /// <summary>
    /// 通过某对象关闭，发起关闭的对象保留到此窗口下次开启
    /// </summary>
    /// <param name="closeBy">发起关闭的对象</param>
    /// <param name="args">变长参数</param>
    /// <returns>是否成功关闭</returns>
    public bool CloseBy(object closeBy, params object[] args)
    {
        this.closeBy = closeBy;
        return Close(args);
    }

    #region 虚方法
    /// <summary>
    /// 开启窗口前，默认返回 true
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>可否开启</returns>
    protected virtual bool OnOpen(params object[] args) => true;
    /// <summary>
    /// 关闭窗口前，默认返回 true
    /// </summary>
    /// <param name="args">变长参数</param>
    /// <returns>可否关闭</returns>
    protected virtual bool OnClose(params object[] args) => true;
    /// <summary>
    /// <see cref="Awake"/>时调用，默认为空
    /// </summary>
    protected virtual void OnAwake() { }
    /// <summary>
    /// <see cref="Start"/>时调用，默认进行消息监听注册操作
    /// </summary>
    protected virtual void OnStart()
    {
        RegisterNotify();
    }
    /// <summary>
    /// <see cref="OnDestroy"/>时调用，默认进行取消消息监听操作
    /// </summary>
    protected virtual void OnDestroy_()
    {
        UnregisterNotify();
    }
    /// <summary>
    /// 注册消息监听，默认为空
    /// </summary>
    protected virtual void RegisterNotify() { }
    /// <summary>
    /// 取消消息监听
    /// </summary>
    protected virtual void UnregisterNotify() { NotifyCenter.RemoveListener(this); }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        WindowsManager.Cache(this);
        if (!content.gameObject.GetComponent<GraphicRaycaster>()) content.gameObject.AddComponent<GraphicRaycaster>();
        WindowCanvas = content.GetComponent<Canvas>();
        WindowCanvas.overrideSorting = true;
        WindowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        if (closeButton) closeButton.onClick.AddListener(() => Close());
        OnAwake();
    }
    private void Start()
    {
        OnStart();
    }
    private void OnDestroy()
    {
        OnDestroy_();
    }
    #endregion

    /// <summary>
    /// 参数格式：([窗口名称: <see cref="string"/>], [窗口状态: <see cref="WindowStates"/>])
    /// </summary>
    public const string WindowStateChanged = "WindowStateChanged";

    public string Tr(string text)
    {
        return LM.Tr(LKName, text);
    }
    public string Tr(string text, params object[] args)
    {
        return LM.Tr(LKName, text, args);
    }
    public IEnumerable<string> TrM(string text, params string[] texts)
    {
        return LM.TrM(LKName, text, texts);
    }

    public static bool IsName<T>(string name) where T : Window
    {
        return typeof(T).Name == name;
    }
}

public abstract class SingletonWindow<T> : Window where T : Window
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance) instance = FindObjectOfType<T>();
            return instance;
        }
    }
}
public enum WindowStates
{
    Open,
    Closed,
    Hidden,
    Shown
}