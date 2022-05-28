using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Item;

public static class ItemUtility
{
    public static ItemData[] GetContrast(ItemData data)
    {
        //TODO 获取可以比较的道具信息
        if (data)
        {
            //if (data.Model_old is WeaponItem weapon)
            //    if (weapon.IsPrimary && PlayerManager.Instance.PlayerInfo.primaryWeapon.item_old)
            //        return new ItemData[] { new ItemData(PlayerManager.Instance.PlayerInfo.primaryWeapon.item_old, false) };
            //    else if (PlayerManager.Instance.PlayerInfo.secondaryWeapon.item_old) return new ItemData[] { new ItemData(PlayerManager.Instance.PlayerInfo.secondaryWeapon.item_old, false) };
        }
        return null;
    }

    /// <summary>
    /// 尝试获取道具原型（原型，非实例）
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>获得的道具</returns>
    public static Item GetItemByID(string id)
    {
        GameManager.Items.TryGetValue(id, out var item);
        return item;
    }
    public static string GetItemNameByID(string id)
    {
        var item = GetItemByID(id);
        if (!item) return null;
        else return item.Name;
    }
}

public sealed class SlotComparer : IComparer<ItemSlotData>
{
    public static SlotComparer Default => new SlotComparer();

    public int Compare(ItemSlotData x, ItemSlotData y)
    {
        if (x.IsEmpty && !y.IsEmpty)
            return 1;
        else if (!x.IsEmpty && y.IsEmpty)
            return -1;
        else if (x.ModelID == y.ModelID)
        {
            if (x.amount < y.amount)
                return 1;
            else if (x.amount > y.amount)
                return -1;
            else return 0;
        }
        else return Item.Comparer.Default.Compare(x.Model, y.Model);
    }
}

public class ItemWithAmount
{
    public readonly ItemData source;
    public int amount;

    public bool IsValid => source && amount > 0 && source.Model;

    public ItemWithAmount(ItemData source, int amount)
    {
        this.source = source;
        this.amount = amount;
    }

    public ItemWithAmount(Item item, int amount)
    {
        source = new ItemData(item, false);
        this.amount = amount;
    }

    public ItemWithAmount(ItemInfo info) : this(info.item, info.Amount) { }

    public static ItemWithAmount[] Convert(IEnumerable<ItemInfo> infos)
    {
        List<ItemWithAmount> results = new List<ItemWithAmount>();
        foreach (var info in infos)
        {
            results.Add(new ItemWithAmount(info));
        }
        return results.ToArray();
    }

    public static implicit operator bool(ItemWithAmount self)
    {
        return self != null;
    }

    public static explicit operator ItemWithAmount(ItemInfo info)
    {
        return new ItemWithAmount(info);
    }
}