using UnityEngine;

[CreateAssetMenu(fileName = "quest item", menuName = "Zetan Studio/任务/任务道具", order = 3)]
[System.Serializable]
public class QuestItem : ItemBase
{
    [SerializeField]
    private string triggerName;
    public string TriggerName => triggerName;

    [SerializeField]
    private bool stateToSet;
    public bool StateToSet => stateToSet;

    public QuestItem()
    {
        itemType = ItemType.Quest;
        discardAble = false;
        sellAble = false;
    }
}