using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Backpack
{
    [HideInInspector]
    public ScopeInt backpackSize = new ScopeInt(50);

    [HideInInspector]
    public ScopeFloat weightLoad = new ScopeFloat(150);

    private long money;
    public long Money
    {
        get
        {
            return money;
        }
        private set
        {
            if (value < 0) value = money;
            money = value;
        }
    }

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

    public void GetMoneySimple(long value)
    {
        if (value >= 0) Money += value;
    }

    public void LoseMoneySimple(long value)
    {
        if (value >= 0) Money -= value;
    }

    public void GetItemSimple(ItemBase item, int amount = 1)
    {
        if (item.StackAble)
        {
            if (Items.Exists(x => x.Item == item))
            {
                Items.Find(x => x.Item == item).Amount += amount;
                weightLoad += item.Weight * amount;
            }
            else
            {
                Items.Add(new ItemInfo(item, amount));
                backpackSize++;
                weightLoad += item.Weight * amount;
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                Items.Add(new ItemInfo(item));
                backpackSize++;
                weightLoad += item.Weight;
            }
        }
    }

    public void LoseItemSimple(ItemInfo info, int amount = 1)
    {
        info.Amount -= amount;
        weightLoad -= info.Item.Weight * amount;
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
        Items.Sort((x, y) =>
        {
            if (x.Item.ItemType == y.Item.ItemType)
            {
                if (x.Item.Quality < y.Item.Quality)
                    return 1;
                else if (x.Item.Quality > y.Item.Quality)
                    return -1;
                else return string.Compare(x.ItemID, y.ItemID);
            }
            else
            {
                if (x.Item.ItemType == ItemType.Weapon) return -1;
                else if (y.Item.ItemType == ItemType.Weapon) return 1;
                else if (x.Item.ItemType == ItemType.Armor) return -1;
                else if (y.Item.ItemType == ItemType.Armor) return 1;
                else if (x.Item.ItemType == ItemType.Jewelry) return -1;
                else if (y.Item.ItemType == ItemType.Jewelry) return 1;
                else if (x.Item.ItemType == ItemType.Tool) return -1;
                else if (y.Item.ItemType == ItemType.Tool) return 1;
                else if (x.Item.ItemType == ItemType.Cuisine) return -1;
                else if (y.Item.ItemType == ItemType.Cuisine) return 1;
                else if (x.Item.ItemType == ItemType.Medicine) return -1;
                else if (y.Item.ItemType == ItemType.Medicine) return 1;
                else if (x.Item.ItemType == ItemType.DanMedicine) return -1;
                else if (y.Item.ItemType == ItemType.DanMedicine) return 1;
                else if (x.Item.ItemType == ItemType.Box) return -1;
                else if (y.Item.ItemType == ItemType.Box) return 1;
                else if (x.Item.ItemType == ItemType.Valuables) return -1;
                else if (y.Item.ItemType == ItemType.Valuables) return 1;
                else if (x.Item.ItemType == ItemType.Quest) return -1;
                else if (y.Item.ItemType == ItemType.Quest) return 1;
                else if (x.Item.ItemType == ItemType.Material) return -1;
                else if (y.Item.ItemType == ItemType.Material) return 1;
                else if (x.Item.ItemType == ItemType.Other) return -1;
                else if (y.Item.ItemType == ItemType.Other) return 1;
                else return 0;
            }
        });
    }
}