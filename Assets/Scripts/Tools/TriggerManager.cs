using UnityEngine;
using System.Collections.Generic;

public delegate void TriggerStateListner(string name, bool value);
[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/触发器管理器")]
public class TriggerManager : SingletonMonoBehaviour<TriggerManager>
{
    private readonly Dictionary<string, TriggerState> triggers = new Dictionary<string, TriggerState>();

    private event TriggerStateListner OnTriggerSetEvent;

    public void SetTrigger(string triggerName, bool value)
    {
        if (!triggers.ContainsKey(triggerName))
            triggers.Add(triggerName, value ? TriggerState.On : TriggerState.Off);
        else triggers[triggerName] = value ? TriggerState.On : TriggerState.Off;
        OnTriggerSetEvent?.Invoke(triggerName, value);
        QuestManager.Instance.UpdateUI();
    }

    public TriggerState GetTriggerState(string triggerName)
    {
        if (!triggers.TryGetValue(triggerName, out var state))
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
        if (triggers.TryGetValue(holder.TriggerName, out var state))
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

    public void SaveData(SaveData data)
    {
        foreach (var trigger in triggers)
            data.triggerDatas.Add(new TriggerData(trigger.Key, trigger.Value));
    }
    public void LoadData(SaveData data)
    {
        triggers.Clear();
        foreach (TriggerData td in data.triggerDatas)
            SetTrigger(td.triggerName, td.triggerState == (int)TriggerState.On);
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