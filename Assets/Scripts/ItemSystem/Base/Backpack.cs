using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Backpack
{
    [HideInInspector]
    public ScopeInt size = new ScopeInt(50);

    [HideInInspector]
    public ScopeFloat weight = new ScopeFloat(150);

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

    public bool IsFull { get { return size.IsMax; } }

    /// <summary>
    /// 负重上限
    /// </summary>
    public float WeightLimit => weight.Max;
    /// <summary>
    /// 开始导致减速的负重
    /// </summary>
    public float WeightOver => weight.Max / 1.5f;

    public ItemInfo LatestInfo => Items.Count < 1 ? null : Items[Items.Count - 1];

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
            if (Items.Exists(x => x.item != null && (x.item == item || x.ItemID == item.ID)))
            {
                Items.Find(x => x.item == item || x.ItemID == item.ID).Amount += amount;
            }
            else
            {
                Items.Add(new ItemInfo(item, amount));
                size++;
            }
            weight.Current += item.Weight * amount;
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                Items.Add(new ItemInfo(item));
                size++;
                weight.Current += item.Weight;
            }
        }
    }

    public void GetItemSimple(ItemInfo info, int amount = 1)
    {
        if (info.item.StackAble)
        {
            if (Items.Exists(x => x.item != null && (x.item == info.item || x.ItemID == info.ItemID)))
            {
                Items.Find(x => x.item == info.item || x.ItemID == info.ItemID).Amount += amount;
            }
            else
            {
                ItemInfo newInfo = info.Cloned;
                newInfo.Amount = amount;
                Items.Add(newInfo);
                size++;
            }
            weight.Current += info.item.Weight * amount;
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                ItemInfo newInfo = info.Cloned;
                newInfo.Amount = 1;
                Items.Add(newInfo);
                size++;
                weight.Current += info.item.Weight;
            }
        }
    }

    public void LoseItemSimple(ItemInfo info, int amount = 1)
    {
        if (!Items.Contains(info)) return;
        info.Amount -= amount;
        if (info.Amount <= 0)
        {
            Items.Remove(info);
            size--;
        }
        weight.Current -= info.item.Weight * amount;
    }

    public ItemInfo Find(string itemID)
    {
        return Items.Find(x => x.ItemID == itemID);
    }
    public ItemInfo Find(ItemBase item)
    {
        return Items.Find(x => x.item == item);
    }

    public IEnumerable<ItemInfo> FindAll(string itemID)
    {
        return Items.FindAll(x => x.ItemID == itemID).AsEnumerable();
    }
    public IEnumerable<ItemInfo> FindAll(ItemBase item)
    {
        return Items.FindAll(x => x.item == item).AsEnumerable();
    }

    public ItemInfo FirstNotStackAble(ItemBase notStkAblItem)
    {
        return Items.Find(x => x.item.StackAble && x.item == notStkAblItem);
    }

    public int GetItemAmount(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        var items = Items.FindAll(x => x.ItemID == id);
        if (items.Count < 1) return 0;
        if (items[0].item.StackAble) return items[0].Amount;
        return items.Count;
    }

    public int GetItemAmount(ItemBase item)
    {
        if (!item) return 0;
        var items = Items.FindAll(x => x.item == item);
        if (items.Count < 1) return 0;
        if (items[0].item.StackAble) return items[0].Amount;
        return items.Count;
    }

    public void Arrange()
    {
        Items.Sort((x, y) =>
        {
            x.indexInGrid = -1;
            y.indexInGrid = -1;
            if (x.item.ItemType == y.item.ItemType)
            {
                if (x.item.Quality < y.item.Quality)
                    return 1;
                else if (x.item.Quality > y.item.Quality)
                    return -1;
                else return string.Compare(x.ItemID, y.ItemID);
            }
            else
            {
                if (x.item.ItemType == ItemType.Weapon) return -1;
                else if (y.item.ItemType == ItemType.Weapon) return 1;
                else if (x.item.ItemType == ItemType.Armor) return -1;
                else if (y.item.ItemType == ItemType.Armor) return 1;
                else if (x.item.ItemType == ItemType.Jewelry) return -1;
                else if (y.item.ItemType == ItemType.Jewelry) return 1;
                else if (x.item.ItemType == ItemType.Tool) return -1;
                else if (y.item.ItemType == ItemType.Tool) return 1;
                else if (x.item.ItemType == ItemType.Cuisine) return -1;
                else if (y.item.ItemType == ItemType.Cuisine) return 1;
                else if (x.item.ItemType == ItemType.Medicine) return -1;
                else if (y.item.ItemType == ItemType.Medicine) return 1;
                else if (x.item.ItemType == ItemType.Elixir) return -1;
                else if (y.item.ItemType == ItemType.Elixir) return 1;
                else if (x.item.ItemType == ItemType.Box) return -1;
                else if (y.item.ItemType == ItemType.Box) return 1;
                else if (x.item.ItemType == ItemType.Book) return -1;
                else if (y.item.ItemType == ItemType.Book) return 1;
                else if (x.item.ItemType == ItemType.Valuables) return -1;
                else if (y.item.ItemType == ItemType.Valuables) return 1;
                else if (x.item.ItemType == ItemType.Quest) return -1;
                else if (y.item.ItemType == ItemType.Quest) return 1;
                else if (x.item.ItemType == ItemType.Material) return -1;
                else if (y.item.ItemType == ItemType.Material) return 1;
                else if (x.item.ItemType == ItemType.Seed) return -1;
                else if (y.item.ItemType == ItemType.Seed) return 1;
                else if (x.item.ItemType == ItemType.Bag) return -1;
                else if (y.item.ItemType == ItemType.Bag) return 1;
                else if (x.item.ItemType == ItemType.Other) return -1;
                else if (y.item.ItemType == ItemType.Other) return 1;
                else return 0;
            }
        });
    }

    public static implicit operator bool(Backpack self)
    {
        return self != null;
    }
}