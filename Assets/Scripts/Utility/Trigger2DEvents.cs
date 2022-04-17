using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Trigger2DEvents : MonoBehaviour
{
    public bool activated = true;

    public Collider2DEvent OnEnter = new Collider2DEvent();
    public Collider2DEvent OnStay = new Collider2DEvent();
    public Collider2DEvent OnExit = new Collider2DEvent();

    #region Monobehaviour
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated) return;
        OnEnter?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!activated) return;
        OnStay?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!activated) return;
        OnExit?.Invoke(collision);
    }
    #endregion
}
[System.Serializable]
public class Collider2DEvent : UnityEvent<Collider2D> { }