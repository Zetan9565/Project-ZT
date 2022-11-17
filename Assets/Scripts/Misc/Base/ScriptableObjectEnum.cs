using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio
{
    public abstract class ScriptableObjectEnum : ScriptableObject
    {
        public abstract ReadOnlyCollection<ScriptableObjectEnumItem> GetEnum();
        public abstract IEnumerable<string> GetNames();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void FindEditorInstance()
        {
            foreach (var type in UnityEditor.TypeCache.GetTypesDerivedFrom<ScriptableObjectEnum>())
            {
                if (type.BaseType.IsGenericType
                    && typeof(ScriptableObjectEnum<,>).IsAssignableFrom(type.BaseType.GetGenericTypeDefinition())
                    && !type.IsAbstract
                    && !type.IsGenericType
                    && !type.IsGenericTypeDefinition)
                    type.BaseType.GetField("instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, instance(type));
            }

            static ScriptableObjectEnum instance(Type type)
            {
                ScriptableObjectEnum instance = Utility.LoadResource(type) as ScriptableObjectEnum;
                if (!instance) instance = Utility.Editor.LoadAsset(type) as ScriptableObjectEnum;
                return instance;
            }
        }
#endif
    }

    public abstract class ScriptableObjectEnum<TS, TI> : ScriptableObjectEnum where TS : ScriptableObjectEnum<TS, TI> where TI : ScriptableObjectEnumItem, new()
    {
        private static TS instance;
        public static TS Instance
        {
            get
            {
                if (!instance) instance = Utility.LoadResource<TS>();
#if UNITY_EDITOR
                if (!instance) instance = Utility.Editor.LoadAsset<TS>();
#endif
                if (!instance) instance = CreateInstance<TS>();
                return instance;
            }
        }

        public virtual TI this[int index]
        {
            get
            {
                if (index < 0 || index > Count - 1) return new TI();
                else return _enum[index];
            }
        }

        public virtual TI this[string name] => this[Array.FindIndex(_enum, x => x.Name == name)];

        public int Count => _enum.Length;

        [SerializeField]
        protected TI[] _enum = { };
        public virtual ReadOnlyCollection<TI> Enum => new ReadOnlyCollection<TI>(_enum);

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

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}