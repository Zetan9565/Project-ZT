using System;
using UnityEngine;

public sealed class Interactive : MonoBehaviour
{
    /**
     * 这个类和带Object那个的主要区别是：
     * 后者是一个完整的可继承并直接生效可用的组件，
     * 而这个则需要自行搭配其它组件并设置相关回调才可正常使用
     * **/

    public bool activated = true;
    public bool _3D;

    [SerializeField]
    private string _name = "可交互对象";
    public string Name
    {
        get
        {
            if (getNameFunc == null) return _name;
            return getNameFunc();
        }
    }

    [SerializeField, SpriteSelector]
    private Sprite icon;
    public Sprite Icon => icon;

    public bool hidePanelOnInteract;

    /// <summary>
    /// 可否交互
    /// </summary>
    public bool IsInteractive => interactiveFunc != null && interactiveFunc();

    [SerializeField]
    private Component component;
    [SerializeField, Tooltip("返回值是布尔且不含参")]
    private string interactMethod;
    [SerializeField, Tooltip("返回值是布尔且不含参")]
    private string interactiveMethod;
    [SerializeField, Tooltip("返回值是字符串且不含参")]
    private string nameMethod;

    public Func<bool> interactFunc;
    public Func<bool> interactiveFunc;
    public Func<string> getNameFunc;

    public ColliderEvent OnEnter;
    public ColliderEvent OnStay;
    public ColliderEvent OnExit;

    public Collider2DEvent OnEnter2D;
    public Collider2DEvent OnStay2D;
    public Collider2DEvent OnExit2D;

    private void Init()
    {
        if (component)
        {
            var type = component.GetType();
            interactFunc = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, type.GetMethod(interactMethod));
            interactiveFunc = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, type.GetMethod(interactiveMethod));
            if (!string.IsNullOrEmpty(nameMethod))
                getNameFunc = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), component, type.GetMethod(nameMethod));
        }
    }

    /// <summary>
    /// 进行交互
    /// </summary>
    /// <returns>交互是否成功</returns>
    public bool DoInteract()
    {
        if (interactFunc())
        {
            if (hidePanelOnInteract)
                InteractionManager.Instance.ShowOrHidePanelBy(this, false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 结束交互，每次DoInteract()后必须手动调用一次
    /// </summary>
    public void FinishInteraction()
    {
        if (hidePanelOnInteract)
            InteractionManager.Instance.ShowOrHidePanelBy(this, true);
    }

    #region Monobehaviour
    private void Awake()
    {
        Init();
    }

    #region 3D Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit?.Invoke(other);
    }
    #endregion

    #region 2D Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter2D?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay2D?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit2D?.Invoke(collision);
    }
    #endregion
    #endregion
}