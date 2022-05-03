using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#region 道具信息相关
[Serializable]
public class ItemInfoBase
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
            else return "(无效道具)";
        }
    }

    [SerializeField, ZetanStudio.Item.ItemSelector]
    public ItemBase item;

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

    public bool IsValid => item;

    public ItemInfoBase()
    {
        amount = 1;
    }

    public ItemInfoBase(ItemBase item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public static ItemInfoBase[] Convert(IEnumerable<ItemWithAmount> items)
    {
        List<ItemInfoBase> results = new List<ItemInfoBase>();
        foreach (var item in items)
        {
            if (item.IsValid) results.Add(new ItemInfoBase(item.source.Model_old, item.amount));
        }
        return results.ToArray();
    }

    public static implicit operator bool(ItemInfoBase self)
    {
        return self != null;
    }
}

[Serializable]
public class ItemInfo : ItemInfoBase //在这个类进行拓展，如强化、词缀、附魔
{
    public ItemInfo(ItemBase item, int amount = 1) : base(item, amount) { }

    [HideInInspector]
    public int indexInGrid;

    public new bool IsValid => item && Amount >= 1;

    public ItemInfo Cloned
    {
        get
        {
            return MemberwiseClone() as ItemInfo;
        }
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

    [SerializeField]
    private ItemBase item;
    public ItemBase Item
    {
        get
        {
            return item;
        }

        set
        {
            item = value;
        }
    }
    [SerializeField]
    private ZetanStudio.Item.ItemNew itemNew;

    [SerializeField]
    private int minAmount = 1;
    public int MinAmount
    {
        get
        {
            return minAmount;
        }

        set
        {
            if (value < 0) minAmount = 0;
            else minAmount = value;
        }
    }

    [SerializeField]
    private int maxAmount = 1;
    public int MaxAmount
    {
        get
        {
            return maxAmount;
        }

        set
        {
            if (value < 0) maxAmount = 0;
            else maxAmount = value;
        }
    }

    [SerializeField]
    private float dropRate = 100.0f;
    public float DropRate => dropRate;

    [SerializeField]
    private bool onlyDropForQuest;
    public bool OnlyDropForQuest
    {
        get
        {
            return onlyDropForQuest;
        }
    }

    [SerializeField]
    private Quest bindedQuest;
    public Quest BindedQuest
    {
        get
        {
            return bindedQuest;
        }
    }

    public bool IsValid => item && MinAmount >= 1;

    public static List<ItemWithAmount> Drop(IEnumerable<DropItemInfo> DropItems)
    {
        List<ItemWithAmount> lootItems = new List<ItemWithAmount>();
        Dictionary<string, ItemWithAmount> map = new Dictionary<string, ItemWithAmount>(); 
        foreach (DropItemInfo di in DropItems)
            if (ZetanUtility.Probability(di.DropRate))
                if (!di.OnlyDropForQuest || QuestManager.Instance.HasOngoingQuest(di.BindedQuest.ID))
                {
                    if (di.item.StackAble)
                    {
                        if (map.TryGetValue(di.ItemID, out var find))
                            find.amount += Random.Range(di.MinAmount, di.MaxAmount + 1);
                        else
                        {
                            var iaw = new ItemWithAmount(di.item.CreateData(), Random.Range(di.MinAmount, di.MaxAmount + 1));
                            map.Add(di.ItemID, iaw);
                            lootItems.Add(iaw);
                        }
                    }
                    else
                    {
                        lootItems.Add(new ItemWithAmount(di.item.CreateData(), Random.Range(di.MinAmount, di.MaxAmount + 1)));
                    }
                }
        return lootItems;
    }

    public static implicit operator bool(DropItemInfo self)
    {
        return self != null;
    }
}

[Serializable]
public class MaterialInfo
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

    [SerializeField]
    private ItemBase item;
    public ItemBase Item => item;

    [SerializeField]
    private int amount = 1;
    public int Amount => amount;

    [SerializeField]
    private MakingType makingType;
    public MakingType MakingType => makingType;

    [SerializeField]
    private MaterialType materialType;
    public MaterialType MaterialType => materialType;

    public bool IsValid
    {
        get
        {
            return (makingType == MakingType.SingleItem && item || makingType == MakingType.SameType && materialType != MaterialType.None) && amount > 0;
        }
    }

    public static bool CheckMaterialsDuplicate(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<MaterialInfo> otherMaterials)
    {
        if (itemMaterials == null || itemMaterials.Count() < 1 || otherMaterials == null || otherMaterials.Count() < 1 || itemMaterials.Count() != otherMaterials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.MakingType == MakingType.SingleItem)
                {
                    var find = otherMaterials.FirstOrDefault(x => x.Item == material.Item);
                    if (!find || find.Amount != material.Amount) return false;
                }
            }
        foreach (MaterialType type in Enum.GetValues(typeof(MaterialType)))
        {
            int amout1 = itemMaterials.Where(x => x.MakingType == MakingType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            int amout2 = otherMaterials.Where(x => x.MakingType == MakingType.SameType && x.MaterialType == type).Select(x => x.Amount).Sum();
            if (amout1 != amout2) return false;
        }
        return true;
    }

    /// <summary>
    /// 检查所给材料是否与指定材料匹配，用于“尝试制作”玩法
    /// </summary>
    /// <param name="itemMaterials">目标材料</param>
    /// <param name="givenMaterials">所给材料</param>
    /// <returns>是否匹配</returns>
    public static bool CheckMaterialsMatch(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<ItemInfoBase> givenMaterials)
    {
        if (itemMaterials == null || itemMaterials.Count() < 1 || givenMaterials == null || givenMaterials.Count() < 1 || itemMaterials.Count() != givenMaterials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.MakingType == MakingType.SingleItem)
                {
                    var find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return false;//提供的材料数量不符
                }
                else
                {
                    int amount = givenMaterials.Where(x => x.item.MaterialType == material.MaterialType).Select(x => x.Amount).Sum();
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
    private ItemBase item;
    public ItemBase Item
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