using System;
using System.Collections.Generic;
public class ItemSlotData
{
    public static ItemSlotData Empty = new ItemSlotData();

    public string ItemID => item ? item.ID : string.Empty;

    public string ModelID => item ? item.ModelID : string.Empty;

    public ItemBase Model => item ? item.Model : null;

    public bool IsEmpty => item == null;

    public bool IsFull => item && item.Model.StackNum <= amount;

    public ItemData item;

    public int index;

    public int amount;

    public event Action<ItemSlotData> OnSlotStateChanged;
    public event Action<ItemSlotData, ItemSlotData> OnSlotSwap;

    public ItemSlotData()
    {
        index = -1;
    }

    public ItemSlotData(int index)
    {
        this.index = index;
    }

    public ItemSlotData(ItemData item, int amount = 1) : this()
    {
        this.item = item;
        this.amount = amount;
    }

    /// <summary>
    /// 替换道具
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="amount">放入数量</param>
    public void Put(ItemData item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
        OnSlotStateChanged?.Invoke(this);
    }

    /// <summary>
    /// 放入道具
    /// </summary>
    /// <param name="amount">放入数量</param>
    /// <returns>实际放入数量</returns>
    public int Put(int amount = 1)
    {
        if (!item) return 0;
        int left = item.Model.StackNum - this.amount;
        if (left >= amount)
        {
            this.amount += amount;
            OnSlotStateChanged?.Invoke(this);
            return amount;
        }
        else
        {
            this.amount += left;
            OnSlotStateChanged?.Invoke(this);
            return left;
        }
    }

    /// <summary>
    /// 取走
    /// </summary>
    /// <param name="amount">取走数量</param>
    /// <returns>实际取走数量</returns>
    public int Take(int amount = 1)
    {
        if (IsEmpty || amount <= 0) return 0;
        int left = this.amount - amount;
        this.amount = left;
        if (this.amount <= 0)
        {
            this.amount = 0;
            item = null;
        }
        OnSlotStateChanged?.Invoke(this);
        return left < 0 ? amount + left : amount;
    }

    public void TakeAll()
    {
        Vacate();
        OnSlotStateChanged?.Invoke(this);
    }

    public void Vacate()
    {
        amount = 0;
        item = null;
    }

    public void Swap(ItemSlotData slot)
    {
        if (!slot) return;
        (item, slot.item) = (slot.item, item);
        amount += slot.amount;
        slot.amount = amount - slot.amount;
        amount -= slot.amount;
        OnSlotStateChanged?.Invoke(this);
        slot.OnSlotStateChanged?.Invoke(slot);
        OnSlotSwap?.Invoke(this, slot);
    }

    public static implicit operator bool(ItemSlotData self)
    {
        return self != null;
    }

    public static List<ItemSlotData> Convert(IEnumerable<ItemInfoBase> infos, int? fixedSlotCount = null)
    {
        List<ItemSlotData> slots = new List<ItemSlotData>();
        foreach (var info in infos)
        {
            if (!fixedSlotCount.HasValue || slots.Count < fixedSlotCount)
                if (info.item.StackAble)
                    slots.Add(new ItemSlotData(new ItemData(info.item, false), info.Amount));
                else for (int i = 0; i < info.Amount; i++)
                    {
                        slots.Add(new ItemSlotData(new ItemData(info.item, false), 1));
                    }
        }
        if (fixedSlotCount.HasValue)
            while (slots.Count < fixedSlotCount)
            {
                slots.Add(new ItemSlotData());
            }
        return slots;
    }
    public static List<ItemSlotData> Convert(IEnumerable<ItemWithAmount> items, int? fixedSlotCount = null)
    {
        List<ItemSlotData> slots = new List<ItemSlotData>();
        foreach (var item in items)
        {
            if (!fixedSlotCount.HasValue || slots.Count < fixedSlotCount)
                if (item.source.Model.StackAble)
                    slots.Add(new ItemSlotData(item.source, item.amount));
                else for (int i = 0; i < item.amount; i++)
                    {
                        slots.Add(new ItemSlotData(item.source, 1));
                    }
        }
        if (fixedSlotCount.HasValue)
            while (slots.Count < fixedSlotCount)
            {
                slots.Add(new ItemSlotData());
            }
        return slots;
    }
}