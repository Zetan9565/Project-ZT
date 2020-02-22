using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class MessageManager : SingletonMonoBehaviour<MessageManager>
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("消息根")]
#endif
    private UnityEngine.UI.VerticalLayoutGroup messageRoot;
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("消息预制件")]
#endif
    private GameObject messagePrefab;

    private Canvas rootCanvas;

    private readonly List<MessageAgent> messages = new List<MessageAgent>();

    public void NewMessage(string message, float lifeTime = 2.0f)
    {
        MessageAgent ma = ObjectPool.Instance.Get(messagePrefab, messageRoot.transform).GetComponent<MessageAgent>();
        ma.messageText.text = message;
        messages.Add(ma);
        StartCoroutine(RecycleMessageDelay(ma, lifeTime));
    }

    private void Awake()
    {
        if (!messageRoot.GetComponent<Canvas>()) rootCanvas = messageRoot.gameObject.AddComponent<Canvas>();
        else rootCanvas = messageRoot.GetComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        rootCanvas.sortingOrder = 999;
    }

    IEnumerator RecycleMessageDelay(MessageAgent message, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        messages.Remove(message);
        if (ObjectPool.Instance)
        {
            message.messageText.text = string.Empty;
            ObjectPool.Instance.Put(message.gameObject);
        }
        else DestroyImmediate(message.gameObject);
    }

    public void Init()
    {
        StopAllCoroutines();
        foreach (var message in messages)
        {
            if (message && message.gameObject)
                if (ObjectPool.Instance)
                {
                    message.messageText.text = string.Empty;
                    ObjectPool.Instance.Put(message.gameObject);
                }
                else DestroyImmediate(message.gameObject);
        }
        messages.Clear();
    }
}
