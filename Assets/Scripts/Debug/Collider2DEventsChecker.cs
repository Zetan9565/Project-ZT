using UnityEngine;

public class Collider2DEventsChecker : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collider event: "+ collision.gameObject.name + " enter " + name);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("collider event: " + collision.gameObject.name + " stay " + name);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("collider event: " + collision.gameObject.name + " exit " + name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("trigger event: " + collision.name + " enter " + name);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("trigger event: " + collision.name + " stay " + name);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("trigger event: " + collision.name + " exit " + name);
    }
}
