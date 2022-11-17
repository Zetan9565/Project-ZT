using System;
using System.Reflection;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    [Serializable]
    public abstract class DialogueEvent
    {
        [field: SerializeField]
        public string ID { get; private set; } = "EVT-" + Guid.NewGuid().ToString("N");

        public abstract bool Invoke();

        [AttributeUsage(AttributeTargets.Class)]
        protected sealed class NameAttribute : Attribute
        {
            public readonly string name;

            public NameAttribute(string name)
            {
                this.name = name;
            }
        }

        public static string GetName(Type type)
        {
            return type.GetCustomAttribute<NameAttribute>()?.name ?? type.Name;
        }
    }

    [Serializable, Name("设置触发器")]
    public sealed class SetTriggerEvent : DialogueEvent
    {
        [field: SerializeField]
        public string TriggerName { get; private set; }

        [field: SerializeField]
        public bool State { get; private set; }

        public override bool Invoke()
        {
            TriggerSystem.TriggerManager.SetTrigger(TriggerName, State);
            return true;
        }
    }
}