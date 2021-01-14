using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public delegate void TriggerStateListner(string name, bool value);
[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/触发器管理器")]
public class TriggerManager : SingletonMonoBehaviour<TriggerManager>
{
    private readonly Dictionary<string, TriggerState> triggers = new Dictionary<string, TriggerState>();
    private readonly Dictionary<string, TriggerHolder> holders = new Dictionary<string, TriggerHolder>();

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
        if (!holder && holders.ContainsKey(holder.ID)) return;
        OnTriggerSetEvent += holder.OnTriggerSet;
        if (triggers.TryGetValue(holder.TriggerName, out var state))
            holder.OnTriggerSet(holder.name, state == TriggerState.On ? true : false);
        holders.Add(holder.ID, holder);
    }

    public void DeleteTriggerListner(TriggerStateListner listner)
    {
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= listner;
    }
    public void DeleteTriggerHolder(TriggerHolder holder)
    {
        if (!holder || !holders.ContainsKey(holder.ID)) return;
        holders.Remove(holder.ID);
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= holder.OnTriggerSet;
    }

    public void SaveData(SaveData data)
    {
        foreach (var trigger in triggers)
            data.triggerData.stateDatas.Add(new TriggerStateData(trigger.Key, trigger.Value));
        foreach (var holder in holders)
            data.triggerData.holderDatas.Add(new TriggerHolderData(holder.Value));
    }
    public void LoadData(SaveData data)
    {
        triggers.Clear();
        foreach (TriggerStateData sd in data.triggerData.stateDatas)
        {
            bool state = sd.triggerState == (int)TriggerState.On;
            if (!triggers.ContainsKey(sd.triggerName))
                triggers.Add(sd.triggerName, state ? TriggerState.On : TriggerState.Off);
            else triggers[sd.triggerName] = state ? TriggerState.On : TriggerState.Off;
        }
        foreach (var holder in this.holders.Values.ToArray())
            DeleteTriggerHolder(holder);
        var holders = FindObjectsOfType<TriggerHolder>();
        foreach (TriggerHolderData hd in data.triggerData.holderDatas)
            foreach (var holder in holders)
                holder.LoadData(hd);
        QuestManager.Instance.UpdateUI();
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