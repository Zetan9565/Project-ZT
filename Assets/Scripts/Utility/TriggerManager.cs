using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ZetanStudio.TriggerSystem
{
    using SavingSystem;

    public delegate void TriggerStateListner(string name, bool value);
    public static class TriggerManager
    {
        private static readonly Dictionary<string, TriggerState> triggers = new Dictionary<string, TriggerState>();
        private static readonly Dictionary<string, TriggerHolder> holders = new Dictionary<string, TriggerHolder>();

        private static event TriggerStateListner OnTriggerSetEvent;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            triggers.Clear();
            holders.Clear();
        }

        public static void SetTrigger(string triggerName, bool value)
        {
            if (!triggers.ContainsKey(triggerName))
                triggers.Add(triggerName, value ? TriggerState.On : TriggerState.Off);
            else triggers[triggerName] = value ? TriggerState.On : TriggerState.Off;
            OnTriggerSetEvent?.Invoke(triggerName, value);
            NotifyCenter.PostNotify(NotifyCenter.CommonKeys.TriggerChanged, triggerName, value);
        }

        public static TriggerState GetTriggerState(string triggerName)
        {
            if (!triggers.TryGetValue(triggerName, out var state))
                return TriggerState.NotExist;
            else return state;
        }

        public static void RegisterTriggerEvent(TriggerStateListner listner)
        {
            OnTriggerSetEvent += listner;
        }
        public static void RegisterTriggerHolder(TriggerHolder holder)
        {
            if (!holder && holders.ContainsKey(holder.ID)) return;
            OnTriggerSetEvent += holder.OnTriggerSet;
            if (triggers.TryGetValue(holder.TriggerName, out var state))
                holder.OnTriggerSet(holder.name, state == TriggerState.On);
            holders.Add(holder.ID, holder);
        }

        public static void DeleteTriggerListner(TriggerStateListner listner)
        {
            OnTriggerSetEvent -= listner;
        }
        public static void DeleteTriggerHolder(TriggerHolder holder)
        {
            if (!holder || !holders.ContainsKey(holder.ID)) return;
            holders.Remove(holder.ID);
            OnTriggerSetEvent -= holder.OnTriggerSet;
        }

        [SaveMethod]
        public static void SaveData(SaveData saveData)
        {
            var data = saveData.Write("triggerData", new GenericData());
            var stateData = data.Write("stateData", new GenericData());
            foreach (var trigger in triggers)
            {
                stateData[trigger.Key] = (int)trigger.Value;
            }
            var holderData = data.Write("holderData", new GenericData());
            foreach (var holder in holders)
            {
                holderData[holder.Key] = holder.Value.isSetAtFirst;
            }
        }
        [LoadMethod]
        public static void LoadData(SaveData saveData)
        {
            triggers.Clear();
            if (saveData.TryReadData("triggerData", out var data))
            {
                if (data.TryReadData("stateData", out var stateData))
                    foreach (var kvp in stateData.ReadIntDict())
                    {
                        bool state = kvp.Value == (int)TriggerState.On;
                        if (!triggers.ContainsKey(kvp.Key))
                            triggers.Add(kvp.Key, state ? TriggerState.On : TriggerState.Off);
                        else triggers[kvp.Key] = state ? TriggerState.On : TriggerState.Off;
                    }
                foreach (var holder in TriggerManager.holders.Values.ToArray())
                    DeleteTriggerHolder(holder);
                var holders = Object.FindObjectsOfType<TriggerHolder>();
                if (data.TryReadData("holderData", out var holderData))
                    foreach (var holder in holders)
                    {
                        if (holderData.TryReadBool(holder.TriggerName, out var isSetAtFirst))
                            holder.LoadData(isSetAtFirst);
                    }
            }
        }
    }
    public enum TriggerState
    {
        NotExist,
        On,
        Off
    }

    public enum TriggerActionType
    {
        [InspectorName("无")]
        None,

        [InspectorName("置位")]
        Set,

        [InspectorName("复位")]
        Reset
    }
}