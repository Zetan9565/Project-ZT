using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "item box", menuName = "ZetanStudio/道具/箱子")]
public class ItemBox : ItemBase, IUsable
{
    [SerializeField]
    private List<ItemInfo> items = new List<ItemInfo>();
    public List<ItemInfo> Items
    {
        get
        {
            return items;
        }
    }

    public ItemBox()
    {
        itemType = ItemType.Box;
    }

    public void OnUse()
    {
        foreach (ItemInfo info in Items)
        {
            int amountBef = BagManager.Instance.GetItemAmountByID(info.ID);
            if (BagManager.Instance.GetItem(info.Item, info.Amount))
                info.Amount -= BagManager.Instance.GetItemAmountByID(info.ID) - amountBef;
        }
        items.RemoveAll(x => x.Amount < 1);
        if (items.Count < 1) BagManager.Instance.LoseItem(this);
    }
}
