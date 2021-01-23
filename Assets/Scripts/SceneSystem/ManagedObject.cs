using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class ManagedObject : MonoBehaviour
{
    public UnityEvent onInit;

    public bool IsInit
    {
        get;
        protected set;
    }

    public virtual bool Init()
    {
        IsInit = true;
        onInit?.Invoke();
        return true;
    }

    public virtual bool Reset()
    {
        return true;
    }

    public virtual bool OnSaveGame(SaveData data)
    {
        return true;
    }

    public virtual bool OnLoadGame(SaveData data)
    {
        return true;
    }
}