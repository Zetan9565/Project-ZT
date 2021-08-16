using System;
using UnityEngine;
using UnityEngine.Events;

public sealed class Interactive : MonoBehaviour
{
    /**
     * 这个类和带Object那个的主要区别是：
     * 后者是一个完整的可继承并直接生效可用的组件，
     * 而这个则需要自行搭配其它组件并选择相关回调才可正常使用
     * **/

    public bool activated = true;

    [SerializeField]
    private string _name = "可交互对象";
    public new string name
    {
        get
        {
            if (string.IsNullOrEmpty(nameMethod)) return _name;
            return getName();
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [SpriteSelector]
#endif
    private Sprite icon;
    public Sprite Icon => icon;

    public bool hidePanelOnInteract;

    public bool IsInteractive => interactive();

    [SerializeField]
    private Component component;
    [SerializeField, Tooltip("返回值是布尔且不含参")]
    private string interactMethod;
    [SerializeField, Tooltip("返回值是布尔且不含参")]
    private string interactiveMethod;
    [SerializeField, Tooltip("返回值是字符串且不含参")]
    private string nameMethod;

    private Func<bool> interact;
    private Func<bool> interactive;
    private Func<string> getName;

    //[SerializeField]
    //private ColliderEvent OnEnter;
    //[SerializeField]
    //private ColliderEvent OnStay;
    //[SerializeField]
    //private ColliderEvent OnExit;

    [SerializeField]
    private Collider2DEvent OnEnter2D;
    [SerializeField]
    private Collider2DEvent OnStay2D;
    [SerializeField]
    private Collider2DEvent OnExit2D;

    private void Init()
    {
        if (component)
        {
            interact += (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, component.GetType().GetMethod(interactMethod));
            interactive += (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), component, component.GetType().GetMethod(interactiveMethod));
            if (!string.IsNullOrEmpty(nameMethod))
                getName += (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), component, component.GetType().GetMethod(nameMethod));
        }
    }

    public bool DoInteract()
    {
        if (interact())
        {
            if (hidePanelOnInteract)
                InteractionManager.Instance.ShowOrHidePanelBy(this, false);
            return true;
        }
        return false;
    }

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
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!activated) return;
    //    if (IsInteractive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Insert(this);
    //    OnEnter?.Invoke(other);
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    if (!activated) return;
    //    if (IsInteractive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Insert(this);
    //    OnStay?.Invoke(other);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!activated) return;
    //    if (IsInteractive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Remove(this);
    //    OnExit?.Invoke(other);
    //}
    #endregion

    #region 2D Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter2D?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay2D?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit2D?.Invoke(collision);
    }
    #endregion
    #endregion
}