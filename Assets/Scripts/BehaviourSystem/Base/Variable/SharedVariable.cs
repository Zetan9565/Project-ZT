using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [Serializable]
    public abstract class SharedVariable
    {
        [SerializeField]
        protected string _name;
        public string name => _name;

        [HideInInspector]
        public bool isGlobal;
        [HideInInspector]
        public bool isShared;

        protected SharedVariable linkedVariable;
        protected HashSet<SharedVariable> linkedVariables = new HashSet<SharedVariable>();

        public bool IsValid => !isGlobal && !isShared || (isGlobal || isShared) && !string.IsNullOrEmpty(_name);

        /// <summary>
        /// 关联共享或全局变量（结点成员变量专用，在共享变量或全局变量列表里的变量不应使用）
        /// </summary>
        /// <param name="variable">关联的变量</param>
        public void Link(SharedVariable variable)
        {
            if (linkedVariable == variable) return;
            Unlink();
            if (variable != null)
            {
                linkedVariable = variable;
                if (!variable.linkedVariables.Contains(this)) variable.linkedVariables.Add(this);
            }
        }
        public void Unlink()
        {
            linkedVariable?.linkedVariables.Remove(this);
            linkedVariable = null;
        }
        public abstract object GetValue();
        public abstract void SetValue(object value);

        public Action<object> onValueChanged;
    }

    [Serializable]
    public abstract class SharedVariable<T> : SharedVariable
    {
        [SerializeField]
        protected T value;
        /// <summary>
        /// 在运行中赋值请使用此属性或SetValue方法，如果直接把泛型值赋值给此对象，此对象会被覆盖，泛型值赋值只是为了方便声明变量的初始值
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
            }
        }

        public override object GetValue()
        {
            return Value;
        }
        public override void SetValue(object value)
        {
            Value = (T)value;
        }

        public virtual T GetGenericValue()
        {
            return Value;
        }
        public virtual void SetGenericValue(T value)
        {
            Value = value;
        }

        public static implicit operator T(SharedVariable<T> self)
        {
            return self.Value;
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