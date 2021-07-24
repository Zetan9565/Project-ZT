using UnityEngine;
using UnityEngine.Events;

public abstract class InteractiveObject : MonoBehaviour
{
    /**
    * 这个类和不带Object那个的主要区别是：
    * 这个是一个完整的可继承并直接生效可用的组件，
    * 而后者则需要自行搭配其它组件并选择相关回调才可正常使用
    * **/

    public bool activated = true;

    [SerializeField]
    protected bool customName;

    [SerializeField, ConditionalHide("customName", false)]
    protected string _name = "可交互对象";
    public virtual new string name { get => _name; protected set => _name = value; }

    [SerializeField, SpriteSelector]
    private Sprite icon;
    public Sprite Icon => icon;

    /// <summary>
    /// 可否触发交互
    /// </summary>
    public virtual bool Interactive { get; protected set; } = true;

    public virtual bool DoInteract()
    {
        return true;
    }

    #region 额外触发器事件
    //protected virtual void OnExit(Collider other)
    //{

    //}

    //protected virtual void OnStay(Collider other)
    //{

    //}

    //protected virtual void OnEnter(Collider other)
    //{

    //}

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
    //protected void OnTriggerEnter(Collider other)
    //{
    //    if (!activated) return;
    //    if (Interactive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Insert(this);
    //    OnEnter(other);
    //}

    //protected void OnTriggerStay(Collider other)
    //{
    //    if (!activated) return;
    //    if (Interactive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Insert(this);
    //    OnStay(other);
    //}

    //protected void OnTriggerExit(Collider other)
    //{
    //    if (!activated) return;
    //    if (Interactive && other.CompareTag("Player"))
    //        InteractionManager.Instance.Remove(this);
    //    OnExit(other);
    //}
    #endregion

    #region 2D Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated) return;
        if (Interactive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnEnter(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated) return;
        if (Interactive && collision.CompareTag("Player"))
            InteractionManager.Instance.Insert(this);
        OnStay(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated) return;
        if (Interactive && collision.CompareTag("Player"))
            InteractionManager.Instance.Remove(this);
        OnExit(collision);
    }
    #endregion
    #endregion
}