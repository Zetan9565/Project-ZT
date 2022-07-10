using System.Collections.Generic;
using System.Linq;
using ZetanStudio.ItemSystem;

public sealed class CountedItem
{
    public readonly ItemData source;
    public int amount;

    public bool IsValid => source && amount > 0 && source.Model;

    public CountedItem(ItemData source, int amount)
    {
        this.source = source;
        this.amount = amount;
    }

    public CountedItem(Item item, int amount)
    {
        source = ItemData.Empty(item);
        this.amount = amount;
    }

    public CountedItem(IItemInfo info) : this(info.Item, info.Amount) { }

    public static CountedItem[] Convert(IEnumerable<IItemInfo> infos)
    {
        List<CountedItem> results = new List<CountedItem>();
        foreach (var info in infos)
        {
            results.Add(new CountedItem(info));
        }
        return results.ToArray();
    }

    public static int GetAmount(IEnumerable<CountedItem> items, Item model)
    {
        if (model.StackAble) return items.FirstOrDefault(x => x.source.Model == model)?.amount ?? 0;
        else return items.Count(x => x.source.Model == model);
    }

    public static implicit operator bool(CountedItem self)
    {
        return self != null;
    }

    public static explicit operator CountedItem(ItemInfo info)
    {
        return new CountedItem(info);
    }
}