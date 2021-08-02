using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TriggerEvents : MonoBehaviour
{
    public bool activated = true;

    public bool _3D;

    public Collider2DEvent OnEnter2D = new Collider2DEvent();
    public Collider2DEvent OnStay2D = new Collider2DEvent();
    public Collider2DEvent OnExit2D = new Collider2DEvent();

    public ColliderEvent OnEnter = new ColliderEvent();
    public ColliderEvent OnStay = new ColliderEvent();
    public ColliderEvent OnExit = new ColliderEvent();

    #region Monobehaviour
    #region 3D Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!activated || !_3D) return;
        OnEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!activated || !_3D) return;
        OnStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activated || !_3D) return;
        OnExit?.Invoke(other);
    }
    #endregion

    #region 2D Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        OnEnter2D?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        OnStay2D?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated || _3D) return;
        OnExit2D?.Invoke(collision);
    }
    #endregion
    #endregion
}
[Serializable]
public class ColliderEvent : UnityEvent<Collider> { }
[Serializable]
public class Collider2DEvent : UnityEvent<Collider2D> { }