using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ItemBase : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    protected string _Name;
    public string Name
    {
        get
        {
            return _Name;
        }
    }

    [SerializeField]
    protected ItemType itemType = ItemType.Other;
    public ItemType ItemType
    {
        get
        {
            return itemType;
        }
        protected set
        {
            itemType = value;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("凡品", "精品", "珍品", "极品", "绝品")]
#endif
    protected ItemQuality quality;
    public ItemQuality Quality
    {
        get
        {
            return quality;
        }
    }

    [SerializeField]
    protected float weight;
    public float Weight
    {
        get
        {
            return weight;
        }
    }

    [SerializeField]
    protected bool sellAble = true;
    public bool SellAble
    {
        get
        {
            return sellAble;
        }
    }

    [SerializeField]
    protected int sellPrice;
    public int SellPrice
    {
        get
        {
            return sellPrice;
        }
    }

    [SerializeField]
    protected int buyPrice;
    public int BuyPrice
    {
        get
        {
            return buyPrice;
        }
    }

    [SerializeField]
    protected Sprite icon;
    public Sprite Icon
    {
        get
        {
            return icon;
        }
    }

    [SerializeField, TextArea]
    protected string description;
    public string Description
    {
        get
        {
            return description;
        }
    }

    [SerializeField]
    protected bool discardAble = true;
    public bool DiscardAble
    {
        get
        {
            return discardAble;
        }
    }

    [SerializeField]
    protected bool inexhaustible;
    public bool Inexhaustible
    {
        get
        {
            return inexhaustible;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("不可制作", "冶炼", "锻造", "裁缝", "烹饪", "炼丹", "制药", "晾晒")]
#endif
    protected ProcessMethod processMethod;
    public ProcessMethod ProcessMethod
    {
        get
        {
            return processMethod;
        }
    }

    [SerializeField]
    protected List<ProcessItemInfo> materials = new List<ProcessItemInfo>();
    public List<ProcessItemInfo> Materials
    {
        get
        {
            return materials;
        }
    }
}

public interface IUsable
{
    void OnUse();
}

public enum ItemType
{
    Other,//其它
    Medicine,//药剂
    DanMedicine,//丹药
    Cuisine,//菜肴
    Weapon,//武器
    Armor,//防具
    Jewelry,//首饰
    Box,//盒子、箱子
    Material,//加工材料
    Valuables,//贵重品
    Quest//任务道具
}

public enum ItemQuality
{
    Normal,//凡品
    Exquisite,//精品
    Precious,//珍品
    Best,//极品
    Peerless,//绝品
}

public enum ProcessMethod
{
    None,//不可制作
    Smelt,//冶炼：材料
    Forging,//锻造：装备、工具
    Tailor,//裁缝：装备
    Cooking,//烹饪：菜肴、Buff
    Alchemy,//炼丹：Buff、恢复剂
    Pharmaceutical,//制药：恢复剂
    Season//晾晒：材料、恢复剂
}
public enum ProcessType
{
    SingleItem,//单种道具
    SameType//同类道具
}

[System.Serializable]
public class ItemInfo
{
    public string ID
    {
        get
        {
            if (Item) return Item.ID;
            else return string.Empty;
        }
    }

    public string Name
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
    }

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
}

[System.Serializable]
public class DropItemInfo : ItemInfo
{
    [SerializeField]
    private float dropRate;
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
}

[System.Serializable]
public class ProcessItemInfo : ItemInfo
{
    [SerializeField]
    private int currentAmount;
    public int CurrentAmount
    {
        get
        {
            return currentAmount;
        }

        set
        {
            if (value < 0) currentAmount = 0;
            currentAmount = value;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("单种材料", "同类材料")]
#endif
    private ProcessType processType;
    public ProcessType ProcessType
    {
        get
        {
            return processType;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("未定义", "矿石", "金属", "植物", "布料", "肉类", "皮毛", "水果")]
#endif
    private MaterialType materialType;
    public MaterialType MaterialType
    {
        get
        {
            return materialType;
        }
    }

}