using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Warehouse
{
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

    [HideInInspector]
    public ScopeInt size = new ScopeInt(50);

    public bool IsFull { get { return size.IsMax; } }

    public ItemInfo Latest
    {
        get
        {
            if (Items.Count < 1) return null;
            return Items[Items.Count - 1];
        }
    }

    public List<ItemInfo> Items { get; }

    public Warehouse(int size = 50)
    {
        this.size = new ScopeInt(size);
        Items = new List<ItemInfo>();
    }

    public void GetItemSimple(ItemInfo info, int amount = 1)
    {
        if (info.item.StackAble)
        {
            if (Items.Exists(x => x.item == info.item))
            {
                Items.Find(x => x.item == info.item).Amount += amount;
            }
            else
            {
                Items.Add(info.Cloned);
                Latest.Amount = amount;
                size++;
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                Items.Add(info.Cloned);
                size++;
            }
        }
    }

    public void LoseItemSimple(ItemInfo info, int amount = 1)
    {
        info.Amount -= amount;
        if (info.Amount <= 0)
        {
            Items.Remove(info);
            size--;
        }
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

    public static implicit operator bool(Warehouse self)
    {
        return self != null;
    }
}
