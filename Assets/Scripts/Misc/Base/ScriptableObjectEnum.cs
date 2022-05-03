using System.Collections.ObjectModel;
using UnityEngine;

public abstract class ScriptableObjectEnum : ScriptableObject
{
    public abstract ReadOnlyCollection<ScriptableObjectEnumItem> GetEnum();
}

public abstract class ScriptableObjectEnum<TS, T> : ScriptableObjectEnum where TS : ScriptableObjectEnum<TS, T> where T : ScriptableObjectEnumItem, new()
{
    protected static TS instance;
    public static TS Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<TS>("");
#if UNITY_EDITOR
            if (!instance) instance = ZetanUtility.Editor.LoadAsset<TS>();
#endif
            if (!instance) instance = CreateInstance<TS>();
            return instance;
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index > Count - 1) return new T();
            else return @enum[index];
        }
    }

    public T this[string name] => this[System.Array.FindIndex(@enum, x => x.Name == name)];

    public int Count => @enum.Length;

    [SerializeField]
    protected T[] @enum;
    public ReadOnlyCollection<T> Enum => new ReadOnlyCollection<T>(@enum);

    public static int NameToIndex(string name)
    {
        if (!instance || string.IsNullOrEmpty(name)) return 0;
        else return System.Array.FindIndex(instance.@enum, x => x.Name == name);
    }

    public ScriptableObjectEnum()
    {
        instance = this as TS;
        @enum = new T[0];
    }

    public sealed override ReadOnlyCollection<ScriptableObjectEnumItem> GetEnum()
    {
        return new ReadOnlyCollection<ScriptableObjectEnumItem>(@enum);
    }
}

public abstract class ScriptableObjectEnumItem
{
    [field: SerializeField]
    public string Name { get; protected set; }

    public ScriptableObjectEnumItem()
    {
        Name = "未定义";
    }
}