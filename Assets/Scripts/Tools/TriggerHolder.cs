using UnityEngine;

[AddComponentMenu("ZetanStudio/实体触发器")]
public class TriggerHolder : MonoBehaviour
{
    [SerializeField]
    private string triggerName;
    public string TriggerName
    {
        get
        {
            return triggerName;
        }
    }

    [SerializeField]
    private bool setStateAtFirst;
    [SerializeField]
    private bool originalState;

    [SerializeField]
    private UnityEngine.Events.UnityEvent onTriggerSet;
    [SerializeField]
    private UnityEngine.Events.UnityEvent onTriggerReset;

    private void Awake()
    {
        if (TriggerManager.Instance && !string.IsNullOrEmpty(triggerName))
        {
            TriggerManager.Instance.RegisterTriggerHolder(this);
            if (setStateAtFirst) TriggerManager.Instance.SetTrigger(TriggerName, originalState);
        }
    }

    public void OnTriggerSet(string name, bool value)
    {
        if (name == TriggerName && !string.IsNullOrEmpty(name))
            if (value) onTriggerSet?.Invoke();
            else onTriggerReset?.Invoke();
    }

    private void OnDestroy()
    {
        if (TriggerManager.Instance) TriggerManager.Instance.DeleteTriggerHolder(this);
    }
}
