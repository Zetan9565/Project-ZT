using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Zetan Studio/事件/实体触发器")]
public class TriggerHolder : MonoBehaviour
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string triggerName;
    public string TriggerName => triggerName;

    [SerializeField]
    private bool setStateAtFirst;
    public bool isSetAtFirst;
    [SerializeField]
    private bool originalState;

    public List<TriggerAction> triggerSetActions = new List<TriggerAction>();
    public List<TriggerAction> triggerResetActions = new List<TriggerAction>();

    public void Init()
    {
        if (TriggerManager.Instance && !string.IsNullOrEmpty(triggerName))
        {
            TriggerManager.Instance.RegisterTriggerHolder(this);
            if (setStateAtFirst && !isSetAtFirst)
            {
                isSetAtFirst = true;
                TriggerManager.Instance.SetTrigger(TriggerName, originalState);
            }
        }
    }

    public void OnTriggerSet(string name, bool value)
    {
        if (name == TriggerName && !string.IsNullOrEmpty(name))
            if (value) foreach (var set in triggerSetActions)
                    ActionStack.Push(set.action, set.delay);
            else foreach (var reset in triggerResetActions)
                    ActionStack.Push(reset.action, reset.delay);
    }

    public void LoadData(TriggerHolderSaveData data)
    {
        if (data.ID != ID) return;
        if (TriggerManager.Instance && !string.IsNullOrEmpty(triggerName))
        {
            TriggerManager.Instance.RegisterTriggerHolder(this);
            if (setStateAtFirst && !data.isSetAtFirst)
            {
                isSetAtFirst = true;
                TriggerManager.Instance.SetTrigger(TriggerName, originalState);
            }
        }
    }

    private void OnDestroy()
    {
        if (TriggerManager.Instance) TriggerManager.Instance.DeleteTriggerHolder(this);
    }
}
[System.Serializable]
public class TriggerAction
{
    public float delay;
    public ActionExecutor action;
}