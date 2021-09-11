using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public abstract class SharedVariable
    {
        [SerializeField]
        protected string _name;
        public string name => _name;

        [HideInInspector]
        public bool isGlobal;
        [HideInInspector]
        public bool isShared;

        public abstract object GetValue();
        public abstract void SetValue(object value);

        public Action<object> onValueChanged;
    }

    [System.Serializable]
    public abstract class SharedVariable<T> : SharedVariable
    {
        [SerializeField]
        protected T value;
        public T Value {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                onValueChanged?.Invoke(value);
            }
        }

        public override object GetValue()
        {
            return value;
        }
        public override void SetValue(object value)
        {
            this.value = (T)value;
        }

        public virtual T GetGenericValue()
        {
            return value;
        }
        public virtual void SetGenericValue(T value)
        {
            this.value = value;
        }

        public static implicit operator T(SharedVariable<T> self)
        {
            return self.value;
        }
    }

    public interface ISharedVariableHandler
    {
        public List<SharedVariable> Variables { get; }

        public Dictionary<string, SharedVariable> KeyedVariables { get; }

        public SharedVariable GetVariable(string name);

        public SharedVariable<T> GetVariable<T>(string name);

        public List<SharedVariable> GetVariables(Type type);

        public List<SharedVariable<T>> GetVariables<T>();

        public bool SetVariable(string name, object value);

        public bool SetVariable<T>(string name, T value);
    }
}