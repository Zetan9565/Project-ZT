using UnityEngine;

[DisallowMultipleComponent]
public class MessageManager : MonoBehaviour
{
    private static MessageManager instance;
    public static MessageManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<MessageManager>();
            return instance;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("消息根")]
#endif
    private Transform messageRoot;
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("消息预制体")]
#endif
    private GameObject messagePrefab;

    public void NewMessage(string message, float lifeTime = 2.0f)
    {
        MessageAgent ma = ObjectPool.Instance.Get(messagePrefab, messageRoot).GetComponent<MessageAgent>();
        ma.messageText.text = message;
        ObjectPool.Instance.Put(ma.gameObject, lifeTime);
    }
}
