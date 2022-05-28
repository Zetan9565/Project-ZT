using UnityEngine;

public abstract class SingletonScriptableObject : ScriptableObject { }

public abstract class SingletonScriptableObject<T> : SingletonScriptableObject where T : SingletonScriptableObject<T>
{
    protected static T instance;
    public static T Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<T>("");
#if UNITY_EDITOR
            if (!instance) instance = ZetanUtility.Editor.LoadAsset<T>();
#endif
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

#if UNITY_EDITOR
    public static T GetOrCreate()
    {
        if (!instance) instance = Resources.Load<T>("");
        if (!instance) instance = ZetanUtility.Editor.LoadAsset<T>();
        if (!instance)
        {
            instance = CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(instance, UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/New {UnityEditor.ObjectNames.NicifyVariableName(typeof(T).Name)}.asset"));
        }
        return instance;
    }
#endif
}