using UnityEngine;

public abstract class Interactive2D : InteractiveBase
{
    #region MonoBehaviour
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Insert();
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Insert();
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Remove();
    }
    #endregion
}