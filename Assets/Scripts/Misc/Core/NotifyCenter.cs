using System.Collections.Generic;
using UnityEngine;

public static class NotifyCenter
{
    public static class CommonKeys
    {
        public const string DayChanged = "DayChanged";
        public const string TriggerChanged = "TriggerChanged";
        public const string GatheringStateChanged = "GatheringStateChanged";
        public const string PlayerStateChanged = "PlayerStateChanged";
        public const string PlayerGetHurt = "PlayerGetHurt";
    }

    private static readonly Dictionary<string, Notify> notifies = new Dictionary<string, Notify>();

    private static readonly Dictionary<object, Dictionary<string, HashSet<NotifyListener>>> notifiesWithOwner = new Dictionary<object, Dictionary<string, HashSet<NotifyListener>>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Clear()
    {
        foreach (var owner in new List<object>(notifiesWithOwner.Keys))
        {
            if (owner is Object uo && uo == null) RemoveListener(owner);
        }
    }

    /// <summary>
    /// 消息订阅。订阅者不需要额外对消息进行判空，发布消息时已做非空处理。
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="listener">订阅者</param>
    /// <param name="owner">订阅者拥有人</param>
    public static void AddListener(string msgType, NotifyListener listener, object owner = null)
    {
        if (notifies.TryGetValue(msgType, out Notify find)) find.AddListener(listener);
        else notifies.Add(msgType, new Notify(listener));

        object target = owner ?? listener.Target;
        if (target != null)
        {
            if (notifiesWithOwner.TryGetValue(target, out var dict))
            {
                if (dict.TryGetValue(msgType, out var set))
                {
                    if (!set.Contains(listener))
                        set.Add(listener);
                }
                else
                    dict.Add(msgType, new HashSet<NotifyListener>() { listener });
            }
            else
                notifiesWithOwner.Add(target, new Dictionary<string, HashSet<NotifyListener>>() { { msgType, new HashSet<NotifyListener>() { listener } } });
        }
    }
    public static void RemoveListener(string msgType, NotifyListener listener)
    {
        if (notifies.TryGetValue(msgType, out Notify find))
            find.RemoveListener(listener);
        if (notifiesWithOwner.TryGetValue(listener.Target, out var dict))
            if (dict.TryGetValue(msgType, out var list))
                list.Remove(listener);
    }
    public static void RemoveListener(object owner)
    {
        if (notifiesWithOwner.TryGetValue(owner, out var dict))
            foreach (var item in dict)
            {
                foreach (var listener in item.Value)
                {
                    if (notifies.TryGetValue(item.Key, out Notify find))
                        find.RemoveListener(listener);
                }
            }
        notifiesWithOwner.Remove(owner);
    }
    /// <summary>
    /// 消息发布。发布者不需要额外对消息进行非空处理，发布消息时已做非空处理。
    /// </summary>
    /// <param name="msgType">消息类型</param>
    /// <param name="msg">消息内容</param>
    public static void PostNotify(string msgType, params object[] msg)
    {
        //Debug.Log($"发布消息{msgType}，内容：{ZetanUtility.SerializeObject(msg, true, 2)}");
        if (msg == null) msg = new object[0];
        if (notifies.TryGetValue(msgType, out Notify find))
            find.Invoke(msg);
    }

    public delegate void NotifyListener(params object[] msg);
    private class Notify
    {
        private event NotifyListener Event;

        public Notify(NotifyListener listener)
        {
            AddListener(listener);
        }

        public void AddListener(NotifyListener listener)
        {
            Event -= listener;//预防重复监听
            Event += listener;
        }

        public void RemoveListener(NotifyListener listener)
        {
            Event -= listener;
        }

        public void Invoke(params object[] msg)
        {
            Event?.Invoke(msg);
        }
    }
}