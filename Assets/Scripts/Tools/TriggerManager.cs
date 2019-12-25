using UnityEngine;
using System.Collections.Generic;

public delegate void TriggerStateListner(string name, bool value);
[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/触发器管理器")]
public class TriggerManager : SingletonMonoBehaviour<TriggerManager>
{
    public Dictionary<string, TriggerState> Triggers { get; } = new Dictionary<string, TriggerState>();

    private event TriggerStateListner OnTriggerSetEvent;

    public void SetTrigger(string triggerName, bool value)
    {
        if (!Triggers.ContainsKey(triggerName))
            Triggers.Add(triggerName, value ? TriggerState.On : TriggerState.Off);
        else Triggers[triggerName] = value ? TriggerState.On : TriggerState.Off;
        OnTriggerSetEvent?.Invoke(triggerName, value);
        QuestManager.Instance.UpdateUI();
    }

    public TriggerState GetTriggerState(string triggerName)
    {
        if (!Triggers.TryGetValue(triggerName, out var state))
            return TriggerState.NotExist;
        else return state;
    }

    public void RegisterTriggerEvent(TriggerStateListner listner)
    {
        OnTriggerSetEvent += listner;
    }
    public void RegisterTriggerHolder(TriggerHolder holder)
    {
        if (!holder) return;
        OnTriggerSetEvent += holder.OnTriggerSet;
        if (Triggers.TryGetValue(holder.TriggerName, out var state))
            holder.OnTriggerSet(holder.name, state == TriggerState.On ? true : false);
    }

    public void DeleteTriggerEvent(TriggerStateListner listner)
    {
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= listner;
    }
    public void DeleteTriggerHolder(TriggerHolder holder)
    {
        if (!holder) return;
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= holder.OnTriggerSet;
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
    None,
    Set,
    Reset
}