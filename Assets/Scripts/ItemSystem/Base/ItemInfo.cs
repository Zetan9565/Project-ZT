using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#region 道具信息相关
[Serializable]
public class ItemInfo //在这个类进行拓展，如强化、词缀、附魔
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

    public ItemInfo()
    {
        amount = 1;
    }

    [SerializeField]
    public ItemBase item;

    [SerializeField]
    private int amount;
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

    public bool IsValid => item && Amount >= 1;

    public ItemInfo(ItemBase item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public ItemInfo Cloned
    {
        get
        {
            return MemberwiseClone() as ItemInfo;
        }
    }

    #region 装备相关
    [HideInInspector]
    public ScopeInt durability;//耐久度

    public int gemSlotAmount;

    public GemItem gemstone1;

    public GemItem gemstone2;
    #endregion

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
    public float DropRate
    {
        get
        {
            return dropRate;
        }
        set
        {
            if (value < 0) dropRate = 0;
            else dropRate = value;
        }
    }

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

    public static List<ItemInfo> Drop(IEnumerable<DropItemInfo> DropItems)
    {
        List<ItemInfo> lootItems = new List<ItemInfo>();
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

    public bool IsInvalid
    {
        get
        {
            return !(makingType == MakingType.SingleItem && Item || makingType == MakingType.SameType);
        }
    }

    public ItemInfo ItemInfo => new ItemInfo(item, amount);

    public static implicit operator bool(MaterialInfo self)
    {
        return self != null;
    }
}

#endregion

#region 喜厌道具信息相关
[Serializable]
public class FavoriteItemInfo
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
    private FavoriteLevel favoriteLevel = FavoriteLevel.Little;
    public FavoriteLevel FavoriteLevel
    {
        get
        {
            return favoriteLevel;
        }
    }

    public static implicit operator bool(FavoriteItemInfo self)
    {
        return self != null;
    }
}

public enum FavoriteLevel
{
    [InspectorName("稍微喜欢")]
    Little = 10,//稍微喜欢+10

    [InspectorName("喜欢")]
    Fond = 30,//喜欢+30

    [InspectorName("很喜欢")]
    Fascinated = 100,//着迷+100

    [InspectorName("着迷")]
    Crazy = 300//狂热的+300
}

[Serializable]
public class HateItemInfo
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
    private HateLevel hateLevel = HateLevel.Dislike;
    public HateLevel HateLevel
    {
        get
        {
            return hateLevel;
        }
    }

    public static implicit operator bool(HateItemInfo self)
    {
        return self != null;
    }
}

public enum HateLevel
{
    [InspectorName("不喜欢")]
    Dislike = 10,//不喜欢-10

    [InspectorName("稍微讨厌")]
    Little = 30,//稍微讨厌-30

    [InspectorName("讨厌")]
    Hate = 100,//讨厌-100

    [InspectorName("很讨厌")]
    Detest = 300//深恶痛绝-300
}
#endregion