using UnityEngine;

public sealed class InteractiveExternal2D : InteractiveExternalBase
{
    #region Monobehaviour
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Insert();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Insert();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (activated && collision.CompareTag("Player")) Remove();
    }
    #endregion
}