using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= listner;
    }
    public static void DeleteTriggerHolder(TriggerHolder holder)
    {
        if (!holder || !holders.ContainsKey(holder.ID)) return;
        holders.Remove(holder.ID);
        if (OnTriggerSetEvent != null) OnTriggerSetEvent -= holder.OnTriggerSet;
    }

    [SaveMethod]
    public static void SaveData(SaveData data)
    {
        foreach (var trigger in triggers)
            data.triggerData.stateDatas.Add(new TriggerStateSaveData(trigger.Key, trigger.Value));
        foreach (var holder in holders)
            data.triggerData.holderDatas.Add(new TriggerHolderSaveData(holder.Value));
    }
    [LoadMethod]
    public static void LoadData(SaveData data)
    {
        triggers.Clear();
        foreach (TriggerStateSaveData sd in data.triggerData.stateDatas)
        {
            bool state = sd.triggerState == (int)TriggerState.On;
            if (!triggers.ContainsKey(sd.triggerName))
                triggers.Add(sd.triggerName, state ? TriggerState.On : TriggerState.Off);
            else triggers[sd.triggerName] = state ? TriggerState.On : TriggerState.Off;
        }
        foreach (var holder in TriggerManager.holders.Values.ToArray())
            DeleteTriggerHolder(holder);
        var holders = UnityEngine.Object.FindObjectsOfType<TriggerHolder>();
        foreach (TriggerHolderSaveData hd in data.triggerData.holderDatas)
            foreach (var holder in holders)
                holder.LoadData(hd);
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