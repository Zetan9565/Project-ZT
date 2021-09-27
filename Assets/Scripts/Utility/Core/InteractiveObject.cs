using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    /**
    * 这个类和不带Object那个的主要区别是：
    * 这个是一个完整的可继承并直接生效可用的组件，
    * 而后者则需要自行搭配其它组件并选择相关回调才可正常使用
    * **/

    public bool activated = true;
    public virtual bool _3D => false;

    [SerializeField]
    protected bool customName;

    protected virtual string CustomName { get => _name; }

    [SerializeField, HideIf("customName", true)]
    protected string _name = "可交互对象";
    public string Name { get => customName ? CustomName : _name; }

    [SerializeField, SpriteSelector]
    private Sprite icon;
    public Sprite Icon => icon;

    public bool hidePanelOnInteract;

    /// <summary>
    /// 可否交互
    /// </summary>
    public virtual bool IsInteractive { get; protected set; } = true;

    /// <summary>
    /// 进行交互
    /// </summary>
    /// <returns>交互是否成功</returns>
    public virtual bool DoInteract()
    {
        if (hidePanelOnInteract)
            InteractionManager.Instance.ShowOrHidePanelBy(this, false);
        return true;
    }

    /// <summary>
    /// 结束交互，每次DoInteract()后必须手动调用一次
    /// </summary>
    public virtual void FinishInteraction()
    {
        if (hidePanelOnInteract)
            InteractionManager.Instance.ShowOrHidePanelBy(this, true);
    }

    #region 额外触发器事件
    protected virtual void OnExit(Collider other)
    {

    }

    protected virtual void OnStay(Collider other)
    {

    }

    protected virtual void OnEnter(Collider other)
    {

    }

    protected virtual void OnExit(Collider2D collision)
    {

    }

    protected virtual void OnStay(Collider2D collision)
    {

    }

    protected virtual void OnEnter(Collider2D collision)
    {

    }
    #endregion

    #region MonoBehaviour
    #region 3D Trigger
    protected void OnTriggerEnter(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter(other);
    }

    protected void OnTriggerStay(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay(other);
    }

    protected void OnTriggerExit(Collider other)
    {
        if (!activated || !_3D) return;
        if (IsInteractive && other.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit(other);
    }
    #endregion

    #region 2D Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        if (IsInteractive && collision.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit(collision);
    }
    #endregion
    #endregion
}