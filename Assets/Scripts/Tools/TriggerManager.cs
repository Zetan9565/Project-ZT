using UnityEngine;
using System.Collections.Generic;

public delegate void TriggerStateListner(string name, bool value);
public class TriggerManager : MonoBehaviour
{
    private static TriggerManager instance;
    public static TriggerManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<TriggerManager>();
            return instance;
        }
    }

    public Dictionary<string, bool> Triggers { get; } = new Dictionary<string, bool>();

    public event TriggerStateListner OnTriggerSetEvent;

    public void SetTrigger(string triggerName, bool value)
    {
        if (!Triggers.ContainsKey(triggerName))
            Triggers.Add(triggerName, value);
        else Triggers[triggerName] = value;
        OnTriggerSetEvent?.Invoke(triggerName, value);
        QuestManager.Instance.UpdateUI();
    }

    public bool GetTriggerState(string triggerName)
    {
        if (!Triggers.ContainsKey(triggerName)) return false;
        else return Triggers[triggerName];
    }
}