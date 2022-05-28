using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class MessageManager : SingletonMonoBehaviour<MessageManager>
{
    [SerializeField]
#if UNITY_EDITOR
    [Label("消息根")]
#endif
    private UnityEngine.UI.VerticalLayoutGroup messageRoot;
    [SerializeField]
#if UNITY_EDITOR
    [Label("消息预制件")]
#endif
    private GameObject messagePrefab;

    private Canvas rootCanvas;

    private readonly List<MessageAgent> messages = new List<MessageAgent>();

    public void New(string message)
    {
        MessageAgent ma = ObjectPool.Get(messagePrefab, messageRoot.transform).GetComponent<MessageAgent>();
        ma.messageText.text = message;
        messages.Add(ma);
        StartCoroutine(RecycleMessageDelay(ma, 2));
    }

    public void New(string message, float lifeTime)
    {
        MessageAgent ma = ObjectPool.Get(messagePrefab, messageRoot.transform).GetComponent<MessageAgent>();
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
        message.messageText.text = string.Empty;
        ObjectPool.Put(message.gameObject);
    }

    public void Init()
    {
        StopAllCoroutines();
        foreach (var message in messages)
        {
            if (message && message.gameObject)
                {
                    message.messageText.text = string.Empty;
                    ObjectPool.Put(message.gameObject);
                }
        }
        messages.Clear();
    }
}
