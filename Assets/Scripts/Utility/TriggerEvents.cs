using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TriggerEvents : MonoBehaviour
{
    public bool activated = true;

    public ColliderEvent OnEnter = new ColliderEvent();
    public ColliderEvent OnStay = new ColliderEvent();
    public ColliderEvent OnExit = new ColliderEvent();

    #region Monobehaviour
    private void OnTriggerEnter(Collider other)
    {
        if (!activated) return;
        OnEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!activated) return;
        OnStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activated) return;
        OnExit?.Invoke(other);
    }
    #endregion
}
[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }