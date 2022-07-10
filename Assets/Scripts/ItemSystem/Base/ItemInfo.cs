using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.Math;

#region 道具信息相关
public interface IItemInfo
{
    public string ItemID { get; }
    public string ItemName { get; }
    public Item Item { get; }
    public int Amount { get; }
    public bool IsValid { get; }
}
[Serializable]
public class ItemInfo : IItemInfo
{
    public string ItemID
    {
        get
        {
            if (Item) return Item.ID;
            else return string.Empty;
        }
    }

    public string ItemName
    {
        get
        {
            if (Item) return Item.Name;
            else return "(无效道具)";
        }
    }

    [SerializeField]
    private Item item;

    [SerializeField]
    protected int amount;
    public int Amount
    {
        get
        {
            return amount;
        }

        set
        {
            if (value < 0) amount = 0;
            else amount = value;
        }
    }

    public bool IsValid => Item;

    public Item Item { get => item; set => item = value; }

    public ItemInfo()
    {
        amount = 1;
    }

    public ItemInfo(Item item, int amount = 1)
    {
        this.Item = item;
        this.amount = amount;
    }

    public static ItemInfo[] Convert(IEnumerable<CountedItem> items)
    {
        List<ItemInfo> results = new List<ItemInfo>();
        foreach (var item in items)
        {
            if (item.IsValid) results.Add(new ItemInfo(item.source.Model, item.amount));
        }
        return results.ToArray();
    }

    public static string GetItemInfoString(IList<ItemInfo> infos)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < infos.Count; i++)
        {
            var ii = infos[i];
            if (ii.IsValid)
            {
                sb.Append('[');
                sb.Append(ItemFactory.GetColorName(ii.Item));
                sb.Append("] ");
                sb.Append(ii.Amount);
            }
            if (i != infos.Count - 1) sb.Append("\n");
        }
        return sb.ToString();
    }

    public static implicit operator bool(ItemInfo self)
    {
        return self != null;
    }
}

[Serializable]
public class DropItemInfo
{
    public string ItemID
    {
        get
        {
            if (item) return item.ID;
            else return string.Empty;
        }
    }

    public string ItemName
    {
        get
        {
            if (item) return item.Name;
            else return string.Empty;
        }
    }

    [SerializeField]
    private Item item;
    public Item Item
    {
        get
        {
            return item;
        }
    }

    public int MinAmount
    {
        get
        {
            return Amount.Range.x;
        }
    }

    public int MaxAmount
    {
        get
        {
            return Amount.Range.y;
        }
    }

    [SerializeField]
    private DistributedIntValue Amount = new DistributedIntValue();

    [SerializeField, ObjectSelector("title", displayNone: true)]
    private Quest onlyDropForQuest;
    public Quest OnlyDropForQuest
    {
        get
        {
            return onlyDropForQuest;
        }
    }

    public bool IsValid => item && Amount.IsValid;

    public bool Definite => MinAmount > 0 && Amount.Distribution.keys.Length > 1 && Amount.Distribution.keys.All(x => x.value >= 1);

    public static List<CountedItem> Drop(IEnumerable<DropItemInfo> DropItems)
    {
        List<CountedItem> lootItems = new List<CountedItem>();
        Dictionary<string, CountedItem> map = new Dictionary<string, CountedItem>();
        foreach (DropItemInfo di in DropItems)
            if (!di.OnlyDropForQuest || QuestManager.HasOngoingQuest(di.OnlyDropForQuest.ID))
            {
                if (di.item.StackAble)
                {
                    if (map.TryGetValue(di.ItemID, out var find))
                        find.amount += di.Amount.RandomValue();
                    else
                    {
                        var iwa = new CountedItem(ItemFactory.MakeItem(di.item), di.Amount.RandomValue());
                        map.Add(di.ItemID, iwa);
                        lootItems.Add(iwa);
                    }
                }
                else
                {
                    lootItems.Add(new CountedItem(ItemFactory.MakeItem(di.item), di.Amount.RandomValue()));
                }
            }
        return lootItems;
    }

    public static string GetDropInfoString(IList<DropItemInfo> products)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < products.Count; i++)
        {
            var di = products[i];
            if (di.IsValid)
            {
                sb.Append('[');
                sb.Append(ItemFactory.GetColorName(di.item));
                sb.Append("] ");
                sb.Append(di.MinAmount);
                if (di.MinAmount != di.MaxAmount)
                {
                    sb.Append("~");
                    sb.Append(di.MaxAmount);
                }
            }
            if (i != products.Count - 1) sb.Append("\n");
        }
        return sb.ToString();
    }

    public static implicit operator bool(DropItemInfo self)
    {
        return self != null;
    }
}
namespace ZetanStudio.ItemSystem
{
    public enum MaterialCostType
    {
        [InspectorName("单种道具")]
        SingleItem,//单种道具

        [InspectorName("同类道具")]
        SameType//同类道具
    }
}
[Serializable]
public class MaterialInfo : IItemInfo
{
    public string ItemID
    {
        get
        {
            if (Item) return Item.ID;
            else return string.Empty;
        }
    }

    public string ItemName
    {
        get
        {
            if (Item) return Item.Name;
            else return string.Empty;
        }
    }

    [SerializeField, ItemFilter(typeof(MaterialModule))]
    private Item item;
    public Item Item => item;

    [SerializeField, Min(1)]
    private int amount = 1;
    public int Amount => amount;

    [SerializeField]
    private MaterialCostType costType;
    public MaterialCostType CostType => costType;

    [SerializeField, Enum(typeof(MaterialType))]
    private int materialType;
    public MaterialType MaterialType => MaterialTypeEnum.Instance[materialType];

    public virtual bool IsValid
    {
        get
        {
            return (costType == MaterialCostType.SingleItem && item || costType == MaterialCostType.SameType && materialType > -1) && amount > 0;
        }
    }

    public static bool CheckMaterialsDuplicate(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<MaterialInfo> otherMaterials)
    {
        if (itemMaterials == null || !itemMaterials.Any() || otherMaterials == null || !otherMaterials.Any() || itemMaterials.Count() != otherMaterials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    var find = otherMaterials.FirstOrDefault(x => x.Item == material.Item);
                    if (!find || find.Amount != material.Amount) return false;
                }
            }
        foreach (var type in MaterialTypeEnum.Instance.Enum)
        {
            int amout1 = itemMaterials.Where(x => x.CostType == MaterialCostType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            int amout2 = otherMaterials.Where(x => x.CostType == MaterialCostType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            if (amout1 != amout2) return false;
        }
        return true;
    }

    /// <summary>
    /// 检查所给材料是否与指定材料匹配，用于“放入材料”玩法
    /// </summary>
    /// <param name="itemMaterials">目标材料</param>
    /// <param name="givenMaterials">所给材料</param>
    /// <returns>是否匹配</returns>
    public static bool CheckMaterialsMatch(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<ItemInfo> givenMaterials)
    {
        if (itemMaterials == null || !itemMaterials.Any() || givenMaterials == null || !givenMaterials.Any() || itemMaterials.Count() != givenMaterials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    var find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return false;//提供的材料数量不符
                }
                else
                {
                    int amount = givenMaterials.Where(x => MaterialModule.SameType(material.MaterialType, x.Item)).Select(x => x.Amount).Sum();
                    if (amount != material.Amount) return false;//提供的材料数量不符
                }
            }
        return true;
    }

    public static implicit operator bool(MaterialInfo self)
    {
        return self != null;
    }
}
#endregion

#region 喜厌道具信息相关
[Serializable]
public class AffectiveItemInfo
{
    [SerializeField]
    private Item item;
    public Item Item
    {
        get
        {
            return item;
        }
    }

    [SerializeField]
    private int intimacyValue;
    public int IntimacyValue => intimacyValue;

    public static implicit operator bool(AffectiveItemInfo self)
    {
        return self != null;
    }
}

public enum EmotionalLevel
{
    [InspectorName("稍微喜欢")]
    Like,//稍微喜欢+10

    [InspectorName("喜欢")]
    Fond,//喜欢+30

    [InspectorName("很喜欢")]
    Fascinated,//着迷+100

    [InspectorName("着迷")]
    Crazy,//狂热的+300

    [InspectorName("不喜欢")]
    Dislike,//不喜欢-10

    [InspectorName("稍微讨厌")]
    LittleHate,//稍微讨厌-30

    [InspectorName("讨厌")]
    Hate,//讨厌-100

    [InspectorName("很讨厌")]
    Detes,//深恶痛绝-300
}
#endregion