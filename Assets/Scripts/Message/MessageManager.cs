using UnityEngine;

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
    [DisplayName("消息预制体")]
#endif
    private GameObject messagePrefab;

    private Canvas rootCanvas;

    public void NewMessage(string message, float lifeTime = 2.0f)
    {
        MessageAgent ma = ObjectPool.Instance.Get(messagePrefab, messageRoot.transform).GetComponent<MessageAgent>();
        ma.messageText.text = message;
        ObjectPool.Instance.Put(ma.gameObject, lifeTime);
    }

    private void Awake()
    {
        if (!messageRoot.GetComponent<Canvas>()) messageRoot.gameObject.AddComponent<Canvas>();
        rootCanvas = messageRoot.GetComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingLayerID = SortingLayer.NameToID("UI");
        rootCanvas.sortingOrder = 999;
    }
}
