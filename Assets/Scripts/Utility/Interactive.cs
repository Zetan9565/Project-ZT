using System;
using UnityEngine;
using UnityEngine.Events;

public sealed class Interactive : MonoBehaviour
{
    /**
     * �����ʹ�Object�Ǹ�����Ҫ�����ǣ�
     * ������һ�������Ŀɼ̳в�ֱ����Ч���õ������
     * ���������Ҫ���д������������ѡ����ػص��ſ�����ʹ��
     * **/

    public bool activated = true;

    [SerializeField]
    private string _name = "�ɽ�������";
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
    [SerializeField, Tooltip("����ֵ�ǲ����Ҳ�����")]
    private string interactMethod;
    [SerializeField, Tooltip("����ֵ�ǲ����Ҳ�����")]
    private string interactiveMethod;
    [SerializeField, Tooltip("����ֵ���ַ����Ҳ�����")]
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