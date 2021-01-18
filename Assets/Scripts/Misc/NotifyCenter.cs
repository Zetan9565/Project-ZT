using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class NotifyCenter : SingletonMonoBehaviour<NotifyCenter>
{
    private readonly Dictionary<string, Notify> notifies = new Dictionary<string, Notify>();

    public bool IsInit { get; private set; }

    public bool Init()
    {
        notifies.Clear();
        return true;
    }

    public void AddListener(string msgType, NotifyListener listener)
    {
        if (notifies.TryGetValue(msgType, out Notify find))
        {
            find.AddListener(listener);
        }
        else
        {
            notifies.Add(msgType, new Notify(listener));
        }
    }

    public void RemoveListener(string msgType, NotifyListener listener)
    {
        if (notifies.TryGetValue(msgType, out Notify find))
        {
            find.RemoveListener(listener);
        }
    }

    public void PostNotify(string msgType, params object[] args)
    {
        if (notifies.TryGetValue(msgType, out Notify find))
        {
            find.Invoke(args);
        }
    }
}

public delegate void NotifyListener(params object[] args);
public class Notify
{
    private event NotifyListener OnListen;

    public Notify(NotifyListener listener)
    {
        AddListener(listener);
    }

    public void AddListener(NotifyListener listener)
    {
        OnListen += listener;
    }

    public void RemoveListener(NotifyListener listener)
    {
        OnListen -= listener;
    }

    public void Invoke(params object[] args)
    {
        OnListen?.Invoke(args);
    }
}