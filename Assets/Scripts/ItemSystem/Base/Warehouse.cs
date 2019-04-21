using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Warehouse
{
    [HideInInspector]
    public ScopeInt backpackSize = new ScopeInt(300);

    public bool IsFull { get { return backpackSize.IsMax; } }

    public ItemInfo Latest
    {
        get
        {
            if (Items.Count < 1) return null;
            return Items[Items.Count - 1];
        }
    }

    public List<ItemInfo> Items { get; } = new List<ItemInfo>();

    public void GetItemSimple(ItemInfo info, int amount = 1)
    {
        if (info.Item.StackAble)
        {
            if (Items.Exists(x => x.Item == info.Item))
            {
                Items.Find(x => x.Item == info.Item).Amount += amount;
            }
            else
            {
                Items.Add(info.Clone() as ItemInfo);
                Latest.Amount = amount;
                backpackSize++;
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                Items.Add(info.Clone() as ItemInfo);
                backpackSize++;
            }
        }
    }

    public void LoseItemSimple(ItemInfo info, int amount = 1)
    {
        info.Amount -= amount;
        if (info.Amount <= 0)
        {
            Items.Remove(info);
            backpackSize--;
        }
    }

    public ItemInfo Find(string itemID)
    {
        return Items.Find(x => x.ItemID == itemID);
    }
    public ItemInfo Find(ItemBase item)
    {
        return Items.Find(x => x.Item == item);
    }

    public IEnumerable<ItemInfo> FindAll(string itemID)
    {
        return Items.FindAll(x => x.ItemID == itemID).AsEnumerable();
    }
    public IEnumerable<ItemInfo> FindAll(ItemBase item)
    {
        return Items.FindAll(x => x.Item == item).AsEnumerable();
    }

    public ItemInfo FirstNotStackAble(ItemBase notStkAblItem)
    {
        return Items.Find(x => x.Item.StackAble && x.Item == notStkAblItem);
    }

    public void Sort()
    {
        Items.Sort((i1, i2) =>
        {
            if (i1.Item.ItemType == i2.Item.ItemType)
            {
                return string.Compare(i1.ItemID, i2.ItemID);
            }
            else
            {
                if (i1.Item.ItemType == ItemType.Weapon) return -1;
                else if (i2.Item.ItemType == ItemType.Weapon) return 1;
                else if (i1.Item.ItemType == ItemType.Armor) return -1;
                else if (i2.Item.ItemType == ItemType.Armor) return 1;
                else if (i1.Item.ItemType == ItemType.Jewelry) return -1;
                else if (i2.Item.ItemType == ItemType.Jewelry) return 1;
                else if (i1.Item.ItemType == ItemType.Tool) return -1;
                else if (i2.Item.ItemType == ItemType.Tool) return 1;
                else if (i1.Item.ItemType == ItemType.Cuisine) return -1;
                else if (i2.Item.ItemType == ItemType.Cuisine) return 1;
                else if (i1.Item.ItemType == ItemType.Medicine) return -1;
                else if (i2.Item.ItemType == ItemType.Medicine) return 1;
                else if (i1.Item.ItemType == ItemType.DanMedicine) return -1;
                else if (i2.Item.ItemType == ItemType.DanMedicine) return 1;
                else if (i1.Item.ItemType == ItemType.Box) return -1;
                else if (i2.Item.ItemType == ItemType.Box) return 1;
                else if (i1.Item.ItemType == ItemType.Valuables) return -1;
                else if (i2.Item.ItemType == ItemType.Valuables) return 1;
                else if (i1.Item.ItemType == ItemType.Quest) return -1;
                else if (i2.Item.ItemType == ItemType.Quest) return 1;
                else if (i1.Item.ItemType == ItemType.Material) return -1;
                else if (i2.Item.ItemType == ItemType.Material) return 1;
                else if (i1.Item.ItemType == ItemType.Other) return -1;
                else if (i2.Item.ItemType == ItemType.Other) return 1;
                else return 0;
            }
        });
    }
}
