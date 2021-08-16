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
            if (item) return item.name;
            else return "(无效道具)";
        }
    }

    [SerializeField]
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

    [HideInInspector]
    private bool locked;
    public bool Locked
    {
        get => IsValid && locked && item.LockAble;
        set
        {
            locked = IsValid && !item.LockAble && value;
        }
    }

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

public class EquipmentInfo : ItemInfo
{
    #region 装备相关
    [HideInInspector]
    public ScopeInt durability;//耐久度

    public int gemSlotAmount;

    public GemItem gemstone1;

    public GemItem gemstone2;

    public EquipmentInfo(ItemBase item, int amount = 1) : base(item, amount) { }
    #endregion
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
            if (Item) return Item.name;
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

    public static List<ItemInfoBase> Drop(IEnumerable<DropItemInfo> DropItems)
    {
        List<ItemInfoBase> lootItems = new List<ItemInfoBase>();
        foreach (DropItemInfo di in DropItems)
            if (ZetanUtility.Probability(di.DropRate))
                if (!di.OnlyDropForQuest || (di.OnlyDropForQuest && QuestManager.Instance.HasOngoingQuestWithID(di.BindedQuest.ID)))
                    lootItems.Add(new ItemInfo(di.Item, Random.Range(di.MinAmount, di.MaxAmount + 1)));
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
            if (Item) return Item.name;
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