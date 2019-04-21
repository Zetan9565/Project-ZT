using UnityEngine;

[CreateAssetMenu(fileName = "quest item", menuName = "ZetanStudio/任务/任务道具")]
[System.Serializable]
public class QuestItem : ItemBase
{
    public QuestItem()
    {
        itemType = ItemType.Quest;
        discardAble = false;
        sellAble = false;
    }
}
