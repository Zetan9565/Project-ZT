using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public abstract class ScriptableObjectEnum : ScriptableObject
{
    public abstract ReadOnlyCollection<ScriptableObjectEnumItem> GetEnum();
    public abstract IEnumerable<string> GetNames();
}

public abstract class ScriptableObjectEnum<TS, TI> : ScriptableObjectEnum where TS : ScriptableObjectEnum<TS, TI> where TI : ScriptableObjectEnumItem, new()
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

    public TI this[int index]
    {
        get
        {
            if (index < 0 || index > Count - 1) return new TI();
            else return _enum[index];
        }
    }

    public TI this[string name] => this[Array.FindIndex(_enum, x => x.Name == name)];

    public int Count => _enum.Length;

    [SerializeField]
    protected TI[] _enum;
    public ReadOnlyCollection<TI> Enum => new ReadOnlyCollection<TI>(_enum);

    public static int NameToIndex(string name)
    {
        if (!instance || string.IsNullOrEmpty(name)) return 0;
        else
        {
            int index = Array.FindIndex(instance._enum, x => x.Name == name);
            index = index < 0 ? 0 : index;
            return index;
        }
    }
    public static string IndexToName(int index)
    {
        if (!instance || index < 0 || index > instance._enum.Length) return string.Empty;
        else return Instance._enum[index].Name;
    }
    public static int IndexOf(TI item)
    {
        if (!instance) return -1;
        return Array.IndexOf(Instance._enum, item);
    }

    public ScriptableObjectEnum()
    {
        instance = this as TS;
        _enum = new TI[0];
    }

    public sealed override ReadOnlyCollection<ScriptableObjectEnumItem> GetEnum()
    {
        return new ReadOnlyCollection<ScriptableObjectEnumItem>(_enum);
    }

    public override IEnumerable<string> GetNames()
    {
        return _enum.Select(x => x.Name);
    }
}

public abstract class ScriptableObjectEnumItem
{
    [field: SerializeField]
    public string Name { get; protected set; }

    public ScriptableObjectEnumItem() : this("未定义") { }

    public ScriptableObjectEnumItem(string name)
    {
        Name = name;
    }

    public static bool operator ==(ScriptableObjectEnumItem left, ScriptableObjectEnumItem right)
    {
        if ((object)left == right)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }
    public static bool operator !=(ScriptableObjectEnumItem left, ScriptableObjectEnumItem right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is ScriptableObjectEnumItem item && GetType() == obj.GetType() && item.Name == Name) return true;
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
    }
}