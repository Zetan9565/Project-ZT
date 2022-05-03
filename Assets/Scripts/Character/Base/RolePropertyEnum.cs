using System;
using UnityEditor;
using UnityEngine;

namespace ZetanStudio.Character
{
    public class RolePropertyEnum : SingletonScriptableObject<RolePropertyEnum>
    {
    }

    public abstract class RoleProperty
    {
        public string name;
        public string formula;

        public abstract ValueType GetValue();
        public abstract void SetValue(RoleAttributeGroup value);
    }
    public abstract class RoleProperty<T> : RoleProperty where T : struct
    {
        protected T value;
        public T Value
        {
            get
            {
                return value;
            }
            protected set
            {
                if (!Equals(this.value, value))
                {
                    var oldValue = this.value;
                    this.value = value;
                    OnValueChanged?.Invoke(this, value);
                }
            }
        }

        public override ValueType GetValue()
        {
            return Value;
        }

        public override void SetValue(RoleAttributeGroup value)
        {

        }

        public event Action<RoleProperty, T> OnValueChanged;

        public static implicit operator T(RoleProperty<T> self)
        {
            return self.Value;
        }
    }
}