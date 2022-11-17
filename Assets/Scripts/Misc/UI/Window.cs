using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZetanStudio;

[DefaultExecutionOrder(-1)]
public abstract class Window : MonoBehaviour, IFadeAble<Window>
{
    protected virtual string LangSelector => GetType().Name;

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

    Window IFadeAble<Window>.MonoBehaviour => this;
    CanvasGroup IFadeAble<Window>.FadeTarget => content;
    Coroutine IFadeAble<Window>.FadeCoroutine { get; set; }
    public virtual void OnOpenComplete() { }
    public virtual void OnCloseComplete() { }
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
            NotifyCenter.PostNotify(WindowStateChanged, GetType(), WindowStates.Open);
            IsOpen = true;
            closeBy = null;
            if (animated) IFadeAble<Window>.FadeTo(this, 1, duration, () => { content.blocksRaycasts = true; OnOpenComplete(); });
            else
            {
                content.alpha = 1;
                content.blocksRaycasts = true;
                OnOpenComplete();
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
            NotifyCenter.PostNotify(WindowStateChanged, GetType(), WindowStates.Closed);
            IsOpen = false;
            onClose?.Invoke();
            onClose = null;
            openBy = null;
            if (animated)
            {
                content.blocksRaycasts = false;
                IFadeAble<Window>.FadeTo(this, 0, duration, () => OnCloseComplete());
            }
            else
            {
                content.alpha = 0;
                content.blocksRaycasts = false;
                OnCloseComplete();
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
        //WindowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        if (closeButton) closeButton.onClick.AddListener(() => Close());
        OnAwake();
        RegisterNotify();
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
        return LM.Tr(LangSelector, text);
    }
    public string Tr(string text, params object[] args)
    {
        return LM.Tr(LangSelector, text, args);
    }
    public IEnumerable<string> TrM(string text, params string[] texts)
    {
        return LM.TrM(LangSelector, text, texts);
    }

    public static bool IsName<T>(string name) where T : Window
    {
        return typeof(T).Name == name;
    }
    public static bool IsType<T>(Type type) where T : Window
    {
        return typeof(T).IsAssignableFrom(type);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        try
        {
            Vector3[] corners = new Vector3[4];
            content.GetComponent<RectTransform>().GetWorldCorners(corners);
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }
        catch { }
    }
    [UnityEditor.MenuItem("GameObject/Zetan Studio/WindowPanel", true)]
    private static bool CanCreateUI()
    {
        return UnityEditor.Selection.activeGameObject is GameObject go && go.transform is RectTransform;
    }
    [UnityEditor.MenuItem("GameObject/Zetan Studio/WindowPanel")]
    private static void CreateUI()
    {
        var win = new GameObject("UndifinedWindow", typeof(RectTransform));
        win.layer = LayerMask.NameToLayer("UI");
        if (UnityEditor.Selection.activeGameObject is GameObject go && go.transform is RectTransform transform)
        {
            win.transform.SetParent(transform, false);
        }
        var wTrans = win.GetComponent<RectTransform>();
        wTrans.anchorMin = Vector2.zero;
        wTrans.anchorMax = Vector2.one;
        wTrans.sizeDelta = Vector2.zero;
        var content = new GameObject("Content", typeof(CanvasGroup), typeof(Image));
        content.layer = LayerMask.NameToLayer("UI");
        content.transform.SetParent(win.transform, false);
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 600);
        var title = new GameObject("WindowTitle", typeof(Text));
        title.layer = LayerMask.NameToLayer("UI");
        title.transform.SetParent(content.transform, false);
        var tTrans = title.GetComponent<RectTransform>();
        tTrans.anchorMin = Vector2.up;
        tTrans.anchorMax = Vector2.up;
        tTrans.anchoredPosition = new Vector2(90, -20);
        tTrans.sizeDelta = new Vector2(160, 40);
        var tText = title.GetComponent<Text>();
        tText.fontSize = 32;
        tText.horizontalOverflow = HorizontalWrapMode.Overflow;
        tText.alignment = TextAnchor.MiddleLeft;
        tText.text = "Undifined";
        if (ColorUtility.TryParseHtmlString("#323232", out var tColor)) tText.color = tColor;
        else tText.color = Color.black;
        var close = new GameObject("Close", typeof(Image), typeof(Button));
        close.layer = LayerMask.NameToLayer("UI");
        close.transform.SetParent(content.transform, false);
        var cTrans = close.GetComponent<RectTransform>();
        cTrans.anchorMin = Vector2.one;
        cTrans.anchorMax = Vector2.one;
        cTrans.pivot = Vector2.one;
        cTrans.sizeDelta = new Vector2(60, 60);
        UnityEditor.Selection.activeGameObject = win;
    }
#endif
}

public abstract class SingletonWindow<T> : Window where T : Window
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance) instance = FindObjectOfType<T>(true);
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