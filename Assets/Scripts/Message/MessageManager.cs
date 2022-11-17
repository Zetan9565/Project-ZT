using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio
{
    [DisallowMultipleComponent]
    public class MessageManager : SingletonMonoBehaviour<MessageManager>
    {
        [SerializeField, Label("消息根")]
        private UnityEngine.UI.VerticalLayoutGroup messageRoot;
        [SerializeField, Label("消息预制件")]
        private MessageAgent messagePrefab;

        private Canvas rootCanvas;

        private SimplePool<MessageAgent> pool;

        private readonly List<MessageAgent> messages = new List<MessageAgent>();

        public void New(string message, float? lifeTime = null)
        {
            MessageAgent ma = pool.Get(messageRoot.transform);
            ma.messageText.text = message;
            messages.Add(ma);
            Timer.Create(() => Recycle(ma), lifeTime ?? 2, true);
        }

        private void Awake()
        {
            if (!messageRoot.GetComponent<Canvas>()) rootCanvas = messageRoot.gameObject.AddComponent<Canvas>();
            else rootCanvas = messageRoot.GetComponent<Canvas>();
            rootCanvas.overrideSorting = true;
            rootCanvas.sortingLayerID = SortingLayer.NameToID("UI");
            rootCanvas.sortingOrder = 999;
            pool = new SimplePool<MessageAgent>(messagePrefab);
        }

        private void Recycle(MessageAgent message)
        {
            messages.Remove(message);
            message.messageText.text = string.Empty;
            pool.Put(message);
        }

        public void Init()
        {
            foreach (var message in messages)
            {
                if (message && message.gameObject)
                {
                    message.messageText.text = string.Empty;
                    pool?.Put(message);
                }
            }
            messages.Clear();
        }
    }
}
