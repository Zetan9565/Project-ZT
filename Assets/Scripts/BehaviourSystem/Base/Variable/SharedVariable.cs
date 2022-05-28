using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [Serializable]
    public abstract partial class SharedVariable
    {
        [SerializeField]
        protected string _name;
#pragma warning disable IDE1006 // 命名样式
        public string name => linkedVariable?._name ?? _name;
#pragma warning restore IDE1006 // 命名样式

        [HideInInspector]
        public bool isGlobal;
        [HideInInspector]
        public bool isShared;

        [SerializeReference]
        protected SharedVariable linkedVariable;

        public bool IsValid => !isGlobal && !isShared || !string.IsNullOrEmpty(name);

        /// <summary>
        /// 关联共享或全局变量（结点成员变量专用，在<see cref="BehaviourTree.Variables"/>或<see cref="GlobalVariables"/>里的变量不应使用）
        /// </summary>
        /// <param name="variable">关联的变量</param>
        public void Link(SharedVariable variable)
        {
            if (linkedVariable == variable) return;
            linkedVariable = variable;
        }
        public void Unlink()
        {
            linkedVariable = null;
        }
        public abstract object GetValue();
        public abstract void SetValue(object value);

        public Action<object> onValueChanged;

        public override string ToString()
        {
            return _name;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 请勿用于游戏逻辑
        /// </summary>
        [HideInInspector]
        public string linkedSVName;
        /// <summary>
        /// 请勿用于游戏逻辑
        /// </summary>
        [HideInInspector]
        public string linkedGVName;
#endif
    }

    [Serializable]
    public abstract class SharedVariable<T> : SharedVariable
    {
        [SerializeField]
        protected T value;
        /// <summary>
        /// 在运行中赋值请使用此属性或<see cref="SetValue(object)"/>方法<br/>如果直接把泛型值赋值给此对象，它会被覆盖，泛型值赋值只是为了在声明时方便赋初始值
        /// </summary>
        public T Value
        {
            get
            {
                if (linkedVariable != null) return (T)linkedVariable.GetValue();
                else return value;
            }
            set
            {
                if (linkedVariable != null) linkedVariable.SetValue(value);
                else this.value = value;
                onValueChanged?.Invoke(value);
                onGenericValueChanged?.Invoke(value);
            }
        }

        public Action<T> onGenericValueChanged;

        public override object GetValue() => Value;
        public override void SetValue(object value) => Value = (T)value;

        static SharedVariable()
        {
            var type = typeof(T);
            if (typeof(SharedVariable).IsAssignableFrom(type))
                throw new NotSupportedException($"{nameof(T)} is not a supported generic argument");
            if (type.IsGenericType && Array.Exists(type.GetGenericArguments(), typeof(SharedVariable).IsAssignableFrom))
                throw new NotSupportedException($"{nameof(T)} is not a supported generic argument");
        }

        public virtual T GetGenericValue()
        {
            return Value;
        }
        public virtual void SetGenericValue(T value)
        {
            Value = value;
        }

        public static implicit operator T(SharedVariable<T> self) => self.Value;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (typeof(T) == obj.GetType()) return Equals(Value, obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(_name);
            hash.Add(name);
            hash.Add(isGlobal);
            hash.Add(isShared);
            hash.Add(linkedVariable);
            hash.Add(IsValid);
            hash.Add(onValueChanged);
#if UNITY_EDITOR
            hash.Add(linkedSVName);
            hash.Add(linkedGVName);
#endif
            hash.Add(value);
            hash.Add(Value);
            hash.Add(onGenericValueChanged);
            return hash.ToHashCode();
        }
    }

    public interface ISharedVariableHandler
    {
        public List<SharedVariable> Variables { get; }

        public SharedVariable GetVariable(string name);

        public bool TryGetVariable(string name, out SharedVariable value);

        public List<SharedVariable> GetVariables(Type type);

        public List<SharedVariable<T>> GetVariables<T>();

        public bool SetVariable(string name, object value);

        public bool SetVariable<T>(string name, T value);
    }
}