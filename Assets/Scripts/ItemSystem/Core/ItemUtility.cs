using System.Collections.Generic;
using UnityEngine;

public static class ItemUtility
{
    public static ItemData[] GetContrast(ItemData data)
    {
        //TODO 获取可以比较的道具信息
        if (data)
        {
            if (data.Model is WeaponItem weapon)
                if (weapon.IsPrimary && PlayerManager.Instance.PlayerInfo.primaryWeapon.item)
                    return new ItemData[] { new ItemData(PlayerManager.Instance.PlayerInfo.primaryWeapon.item, false) };
                else if (PlayerManager.Instance.PlayerInfo.secondaryWeapon.item) return new ItemData[] { new ItemData(PlayerManager.Instance.PlayerInfo.secondaryWeapon.item, false) };
        }
        return null;
    }
    public static string GetColorName(ItemBase item)
    {
        if (MiscSettings.Instance && item.Quality > 0)
            return ZetanUtility.ColorText(item.Name, QualityToColor(item.Quality));
        else return item.Name;
    }

    /// <summary>
    /// 尝试获取道具原型（原型，非实例）
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>获得的道具</returns>
    public static ItemBase GetItemByID(string id)
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
    public static Color QualityToColor(ItemQuality quality)
    {
        if (MiscSettings.Instance && quality >= 0 && MiscSettings.Instance.QualityColors != null && (int)quality < MiscSettings.Instance.QualityColors.Count)
            return MiscSettings.Instance.QualityColors[(int)quality];
        else return Color.clear;
    }
}
public static class ItemUsage
{
    public static void UseItem(ItemData itemInfo)
    {
        if (!itemInfo.Model.Usable)
        {
            MessageManager.Instance.New("该物品不可使用");
            return;
        }
        if (itemInfo.Model.IsBox) UseBox(itemInfo);
        else if (itemInfo.Model.IsEquipment) UseEuipment(itemInfo);
        else if (itemInfo.Model.IsBook) UseBook(itemInfo);
        else if (itemInfo.Model.IsBag) UseBag(itemInfo);
        else if (itemInfo.Model.IsForQuest) UseQuest(itemInfo);
    }

    public static void UseBox(ItemData item)
    {
        BoxItem box = item.Model as BoxItem;
        BackpackManager.Instance.LoseItem(item, 1, box.GetItems());
    }

    public static void UseEuipment(ItemData MItemInfo)
    {
        //Equip(MItemInfo);
    }

    public static void UseBook(ItemData item)
    {
        BookItem book = item.Model as BookItem;
        switch (book.BookType)
        {
            case BookType.Building:
                if (BackpackManager.Instance.CanLose(item, 1) && BuildingManager.Instance.Learn(book.BuildingToLearn))
                {
                    BackpackManager.Instance.LoseItem(item, 1);
                }
                break;
            case BookType.Making:
                if (BackpackManager.Instance.CanLose(item, 1) && MakingManager.Instance.Learn(book.ItemToLearn))
                {
                    BackpackManager.Instance.LoseItem(item, 1);
                }
                break;
            case BookType.Skill:
            default: break;
        }
    }

    public static void UseBag(ItemData item)
    {
        BagItem bag = item.Model as BagItem;
        if (BackpackManager.Instance.CanLose(item, 1))
        {
            if (BackpackManager.Instance.ExpandSize(bag.ExpandSize))
                BackpackManager.Instance.LoseItem(item, 1);
        }
    }

    public static void UseQuest(ItemData item)
    {
        if (!BackpackManager.Instance.CanLose(item, 1)) return;
        QuestItem quest = item.Model as QuestItem;
        TriggerManager.Instance.SetTrigger(quest.TriggerName, quest.StateToSet);
        BackpackManager.Instance.LoseItem(item, 1);
    }
}

public sealed class SlotComparer : IComparer<ItemSlotData>
{
    public static SlotComparer Default { get; } = new SlotComparer();

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
        else if (x.Model.ItemType == y.Model.ItemType)
        {

            if (x.Model.Quality < y.Model.Quality)
                return 1;
            else if (x.Model.Quality > y.Model.Quality)
                return -1;
            else return string.Compare(x.ModelID, y.ModelID);
        }
        else
        {
            if (x.Model.ItemType == ItemType.Weapon) return -1;
            else if (y.Model.ItemType == ItemType.Weapon) return 1;
            else if (x.Model.ItemType == ItemType.Armor) return -1;
            else if (y.Model.ItemType == ItemType.Armor) return 1;
            else if (x.Model.ItemType == ItemType.Jewelry) return -1;
            else if (y.Model.ItemType == ItemType.Jewelry) return 1;
            else if (x.Model.ItemType == ItemType.Tool) return -1;
            else if (y.Model.ItemType == ItemType.Tool) return 1;
            else if (x.Model.ItemType == ItemType.Cuisine) return -1;
            else if (y.Model.ItemType == ItemType.Cuisine) return 1;
            else if (x.Model.ItemType == ItemType.Medicine) return -1;
            else if (y.Model.ItemType == ItemType.Medicine) return 1;
            else if (x.Model.ItemType == ItemType.Elixir) return -1;
            else if (y.Model.ItemType == ItemType.Elixir) return 1;
            else if (x.Model.ItemType == ItemType.Box) return -1;
            else if (y.Model.ItemType == ItemType.Box) return 1;
            else if (x.Model.ItemType == ItemType.Book) return -1;
            else if (y.Model.ItemType == ItemType.Book) return 1;
            else if (x.Model.ItemType == ItemType.Valuables) return -1;
            else if (y.Model.ItemType == ItemType.Valuables) return 1;
            else if (x.Model.ItemType == ItemType.Quest) return -1;
            else if (y.Model.ItemType == ItemType.Quest) return 1;
            else if (x.Model.ItemType == ItemType.Material) return -1;
            else if (y.Model.ItemType == ItemType.Material) return 1;
            else if (x.Model.ItemType == ItemType.Seed) return -1;
            else if (y.Model.ItemType == ItemType.Seed) return 1;
            else if (x.Model.ItemType == ItemType.Bag) return -1;
            else if (y.Model.ItemType == ItemType.Bag) return 1;
            else if (x.Model.ItemType == ItemType.Other) return -1;
            else if (y.Model.ItemType == ItemType.Other) return 1;
            else return 0;
        }
    }
}

public sealed class ItemComparer : IComparer<ItemBase>
{
    public static ItemComparer Default { get; } = new ItemComparer();

    public int Compare(ItemBase x, ItemBase y)
    {
        if (x.ItemType == y.ItemType)
        {

            if (x.Quality < y.Quality)
                return 1;
            else if (x.Quality > y.Quality)
                return -1;
            else return string.Compare(x.ID, y.ID);
        }
        else
        {
            if (x.ItemType == ItemType.Weapon) return -1;
            else if (y.ItemType == ItemType.Weapon) return 1;
            else if (x.ItemType == ItemType.Armor) return -1;
            else if (y.ItemType == ItemType.Armor) return 1;
            else if (x.ItemType == ItemType.Jewelry) return -1;
            else if (y.ItemType == ItemType.Jewelry) return 1;
            else if (x.ItemType == ItemType.Tool) return -1;
            else if (y.ItemType == ItemType.Tool) return 1;
            else if (x.ItemType == ItemType.Cuisine) return -1;
            else if (y.ItemType == ItemType.Cuisine) return 1;
            else if (x.ItemType == ItemType.Medicine) return -1;
            else if (y.ItemType == ItemType.Medicine) return 1;
            else if (x.ItemType == ItemType.Elixir) return -1;
            else if (y.ItemType == ItemType.Elixir) return 1;
            else if (x.ItemType == ItemType.Box) return -1;
            else if (y.ItemType == ItemType.Box) return 1;
            else if (x.ItemType == ItemType.Book) return -1;
            else if (y.ItemType == ItemType.Book) return 1;
            else if (x.ItemType == ItemType.Valuables) return -1;
            else if (y.ItemType == ItemType.Valuables) return 1;
            else if (x.ItemType == ItemType.Quest) return -1;
            else if (y.ItemType == ItemType.Quest) return 1;
            else if (x.ItemType == ItemType.Material) return -1;
            else if (y.ItemType == ItemType.Material) return 1;
            else if (x.ItemType == ItemType.Seed) return -1;
            else if (y.ItemType == ItemType.Seed) return 1;
            else if (x.ItemType == ItemType.Bag) return -1;
            else if (y.ItemType == ItemType.Bag) return 1;
            else if (x.ItemType == ItemType.Other) return -1;
            else if (y.ItemType == ItemType.Other) return 1;
            else return 0;
        }
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

    public ItemWithAmount(ItemBase item, int amount)
    {
        source = new ItemData(item, false);
        this.amount = amount;
    }

    public ItemWithAmount(ItemInfoBase info) : this(info.item, info.Amount) { }

    public static ItemWithAmount[] Convert(IEnumerable<ItemInfoBase> infos)
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

    public static explicit operator ItemWithAmount(ItemInfoBase info)
    {
        return new ItemWithAmount(info);
    }
}