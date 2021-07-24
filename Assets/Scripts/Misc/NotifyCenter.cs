using System.Collections.Generic;

public class NotifyCenter : SingletonMonoBehaviour<NotifyCenter>
{
    public abstract class CommonKeys
    {
        public const string DayChange = "DayChange";
        public const string QuestChange = "QuestChange";
        public const string ObjectiveChange = "ObjectiveChange";
        public const string TriggerChange = "TriggerChange";
        public const string WindowStateChange = "WindowStateChange";
        public const string GatheringStateChange = "GatheringStateChange";
    }

    private readonly Dictionary<string, Notify> notifies = new Dictionary<string, Notify>();

    private readonly Dictionary<object, Dictionary<string, HashSet<NotifyListener>>> notifiesWithOwner = new Dictionary<object, Dictionary<string, HashSet<NotifyListener>>>();

    public bool IsInit { get; private set; }

    public bool Init()
    {
        notifies.Clear();
        return true;
    }

    public void AddListener(string msgType, NotifyListener listener, object owner = null)
    {
        if (notifies.TryGetValue(msgType, out Notify find)) find.AddListener(listener);
        else notifies.Add(msgType, new Notify(listener));

        object target = owner ?? listener.Target;
        if (target != null)
        {
            if (notifiesWithOwner.TryGetValue(target, out var dict))
            {
                if (dict.TryGetValue(msgType, out var list))
                {
                    if (!list.Contains(listener))
                        list.Add(listener);
                }
                else
                    dict.Add(msgType, new HashSet<NotifyListener>() { listener });
            }
            else
                notifiesWithOwner.Add(target, new Dictionary<string, HashSet<NotifyListener>>() { { msgType, new HashSet<NotifyListener>() { listener } } });
        }
    }

    public void RemoveListener(string msgType, NotifyListener listener)
    {
        if (notifies.TryGetValue(msgType, out Notify find))
            find.RemoveListener(listener);
        if (notifiesWithOwner.TryGetValue(listener.Target, out var dict))
            if (dict.TryGetValue(msgType, out var list))
                list.Remove(listener);
    }

    public void RemoveListener(object owner)
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

    public void PostNotify(string msgType, params object[] msg)
    {
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
            Event -= listener;//‘§∑¿÷ÿ∏¥º‡Ã˝
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