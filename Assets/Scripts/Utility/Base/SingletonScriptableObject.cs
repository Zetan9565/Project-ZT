using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance) instance = CreateInstance<T>();
            return instance;
        }
    }

    public SingletonScriptableObject()
    {
        instance = this as T;
    }
    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}