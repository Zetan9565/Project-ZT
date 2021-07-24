using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "item box", menuName = "ZetanStudio/道具/箱子")]
[System.Serializable]
public class BoxItem : ItemBase
{
    [SerializeField]
    private int minAmount;
    public int MinAmount => minAmount;

    [SerializeField]
    private int maxAmount;
    public int MaxAmount => maxAmount;

    [SerializeField, NonReorderable]
    private List<DropItemInfo> itemsInBox = new List<DropItemInfo>();
    public List<DropItemInfo> ItemsInBox
    {
        get
        {
            return itemsInBox;
        }
    }

    public BoxItem()
    {
        itemType = ItemType.Box;
    }

    public ItemInfo[] GetItems()
    {
        List<ItemInfo> lootItems = DropItemInfo.Drop(itemsInBox);
        return lootItems.ToArray();
    }
}