using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ItemBase : ScriptableObject
{
    #region 基本信息
    [SerializeField]
    protected string _ID;
    public virtual string ID => _ID;

    [SerializeField]
    protected string _Name;
    public new virtual string name => _Name;

    [SerializeField]
    protected ItemType itemType = ItemType.Other;
    public virtual ItemType ItemType => itemType;

    [SerializeField]
    protected ItemQuality quality;
    public virtual ItemQuality Quality => quality;

    [SerializeField]
    protected float weight;
    public virtual float Weight => weight;

    [SerializeField]
    protected bool sellAble = true;
    public virtual bool SellAble => sellAble;

    [SerializeField]
    protected int sellPrice;
    public virtual int SellPrice => sellPrice;

    [SerializeField]
    protected int buyPrice;
    public virtual int BuyPrice => buyPrice;

    [SerializeField]
    protected Sprite icon;
    public virtual Sprite Icon => icon;

    [SerializeField, TextArea]
    protected string description;
    public virtual string Description => description;

    [SerializeField]
    protected bool stackAble = true;
    public virtual bool StackAble => Inexhaustible ? false : stackAble;

    [SerializeField]
    protected bool discardAble = true;
    public virtual bool DiscardAble => discardAble;

    [SerializeField]
    protected bool lockAble = true;
    public virtual bool LockAble => lockAble;

    [SerializeField]
    protected bool usable = true;
    public virtual bool Usable => usable;

    [SerializeField]
    protected bool inexhaustible;//用之不竭
    /// <summary>
    /// 是否用之不竭
    /// </summary>
    public virtual bool Inexhaustible => inexhaustible;

    [SerializeField]
    private int maxDurability = 100;
    public virtual int MaxDurability => maxDurability;
    #endregion

    #region 制作相关
    [SerializeField]
    protected MakingMethod makingMethod;
    public virtual MakingMethod MakingMethod => makingMethod;

    [SerializeField]
    protected int minYield = 1;
    public virtual int MinYield => minYield;

    [SerializeField]
    protected int maxYield = 1;
    public virtual int MaxYield => maxYield;

    [SerializeField]
    protected MaterialType materialType;
    public MaterialType MaterialType
    {
        get
        {
            return materialType;
        }
    }

    [SerializeField, NonReorderable]
    private List<MaterialInfo> materials = new List<MaterialInfo>();
    public virtual List<MaterialInfo> Materials => materials;

    public MakingToolType MakingTool
    {
        get
        {
            switch (MakingMethod)
            {
                case MakingMethod.Smelt:
                case MakingMethod.Forging:
                    return MakingToolType.Forging;
                case MakingMethod.Weaving:
                    return MakingToolType.Loom;
                case MakingMethod.Tailor:
                    return MakingToolType.SewingTable;
                case MakingMethod.Cooking:
                    return MakingToolType.Kitchen;
                case MakingMethod.Alchemy:
                    return MakingToolType.AlchemyFurnace;
                case MakingMethod.Pharmaceutical:
                    return MakingToolType.PharmaceuticalTable;
                case MakingMethod.Season:
                    return MakingToolType.DryingTable;
                case MakingMethod.Triturate:
                    return MakingToolType.PestleAndMortar;
                case MakingMethod.Handmade:
                    return MakingToolType.Handwork;
                default: return MakingToolType.None;
            }
        }
    }

    public bool DIYAble => MakingTool != MakingToolType.None && !Materials.TrueForAll(x => x.MakingType == MakingType.SingleItem);

    public bool Makable => MakingMethod != MakingMethod.None && Materials != null && Materials.Count > 0 && 
        !Materials.Exists(x => x.MakingType == MakingType.SameType && x.MaterialType == MaterialType.None);
    #endregion

    #region 类型判断相关
    public bool IsWeapon => this is WeaponItem;

    public bool IsBox => this is BoxItem;

    public bool IsBag => this is BagItem;

    public bool IsSeed => this is SeedItem;

    public bool IsMaterial => this is MaterialItem;

    public bool IsGemstone => this is GemItem;

    public bool IsForQuest => this is QuestItem;

    public bool IsBook => this is BookItem;

    public bool IsMedicine => this is MedicineItem;

    public bool IsEquipment => this is WeaponItem || this is ArmorItem;

    /// <summary>
    /// 是消耗品
    /// </summary>
    public bool IsConsumable => this is BoxItem || this is BagItem || this is MedicineItem;
    #endregion

    public static string GetItemTypeString(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Jewelry:
            case ItemType.Tool:
                return "装备";
            case ItemType.Quest:
            case ItemType.Valuables:
                return "特殊";
            case ItemType.Material:
                return "材料";
            case ItemType.Box:
            case ItemType.Bag:
            case ItemType.Medicine:
            case ItemType.Elixir:
            case ItemType.Cuisine:
                return "消耗品";
            default: return "普通";
        }
    }
}

#region 道具种类相关
public enum ItemType
{
    /// <summary>
    /// 其他
    /// </summary>
    Other,

    /// <summary>
    /// 药剂
    /// </summary>
    Medicine,

    /// <summary>
    /// 丹药
    /// </summary>
    Elixir,

    /// <summary>
    /// 菜肴
    /// </summary>
    Cuisine,

    /// <summary>
    /// 武器
    /// </summary>
    Weapon,

    /// <summary>
    /// 防具
    /// </summary>
    Armor,

    /// <summary>
    /// 首饰
    /// </summary>
    Jewelry,

    /// <summary>
    /// 盒子、箱子
    /// </summary>
    Box,

    /// <summary>
    /// 加工材料
    /// </summary>
    Material,

    /// <summary>
    /// 贵重品：用于贸易
    /// </summary>
    Valuables,

    /// <summary>
    /// 任务道具
    /// </summary>
    Quest,

    /// <summary>
    /// 采集工具
    /// </summary>
    Tool,

    /// <summary>
    /// 宝石
    /// </summary>
    Gemstone,

    /// <summary>
    /// 书籍
    /// </summary>
    Book,

    /// <summary>
    /// 袋子
    /// </summary>
    Bag,

    /// <summary>
    /// 种子
    /// </summary>
    Seed
}

public enum ItemQuality
{
    [InspectorName("凡品")]
    Normal,

    [InspectorName("精品")]
    Exquisite,

    [InspectorName("珍品")]
    Precious,

    [InspectorName("极品")]
    Best,

    [InspectorName("绝品")]
    Peerless,
}

public enum MakingMethod
{
    /// <summary>
    /// 不可制作
    /// </summary>
    [InspectorName("不可制作")]
    None,

    /// <summary>
    /// 手工：所有类型
    /// </summary>
    [InspectorName("手工")]
    Handmade,

    /// <summary>
    /// 冶炼：材料
    /// </summary>
    [InspectorName("冶炼")]
    Smelt,

    /// <summary>
    /// 锻造：装备、工具
    /// </summary>
    [InspectorName("锻造")]
    Forging,

    /// <summary>
    /// 织布：材料
    /// </summary>
    [InspectorName("织布")]
    Weaving,

    /// <summary>
    /// 裁缝：装备
    /// </summary>
    [InspectorName("裁缝")]
    Tailor,

    /// <summary>
    /// 烹饪：菜肴、Buff
    /// </summary>
    [InspectorName("烹饪")]
    Cooking,

    /// <summary>
    /// 炼丹：Buff、恢复剂
    /// </summary>
    [InspectorName("炼丹")]
    Alchemy,

    /// <summary>
    /// 制药：恢复剂
    /// </summary>
    [InspectorName("制药")]
    Pharmaceutical,

    /// <summary>
    /// 晾晒：材料、恢复剂
    /// </summary>
    [InspectorName("晾晒")]
    Season,

    /// <summary>
    /// 研磨：材料、恢复剂
    /// </summary>
    [InspectorName("研磨")]
    Triturate
}

public enum MakingType
{
    [InspectorName("单种道具")]
    SingleItem,//单种道具

    [InspectorName("同类道具")]
    SameType//同类道具
}
#endregion

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
            locked = !IsValid ? false : (item.LockAble ? false : value);
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
    private int amount = 1;
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

[Serializable]
public class PowerUp
{
    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("攻击增加")]
#endif
    private int _ATK_Add;
    public int ATK_Add
    {
        get
        {
            return _ATK_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("防御增加")]
#endif
    private int _DEF_Add;
    public int DEF_Add
    {
        get
        {
            return _DEF_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("命中增加")]
#endif
    private int hit_Add;
    public int Hit_Add
    {
        get
        {
            return hit_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("闪避增加")]
#endif
    private int dodge_Add;
    public int Dodge_Add
    {
        get
        {
            return dodge_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("暴击增加")]
#endif
    private int crit_Add;
    public int Crit_Add
    {
        get
        {
            return crit_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("体力增加")]
#endif
    private int _HP_Add;
    public int HP_Add
    {
        get
        {
            return _HP_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("魔力增加")]
#endif
    private int _MP_Add;
    public int MP_Add
    {
        get
        {
            return _MP_Add;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("负重能力增加")]
#endif
    private float weightLoadAdd;
    public float WeightLoadAdd
    {
        get
        {
            return weightLoadAdd;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("经验获取量增加%")]
#endif
    private float _EXP_AddPer;//经验获取量增加百分比
    public float EXP_AddPer
    {
        get
        {
            return _EXP_AddPer;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("生活经验获取量增加%")]
#endif
    private float liveEXP_AddPer;//生活经验获取量增加百分比
    public float LiveEXP_AddPer
    {
        get
        {
            return liveEXP_AddPer;
        }
    }

    public bool IsEffective
    {
        get
        {
            return _ATK_Add > 0 || _DEF_Add > 0 || hit_Add > 0 || dodge_Add > 0 || crit_Add > 0 ||
                _HP_Add > 0 || _MP_Add > 0 || weightLoadAdd > 0 || _EXP_AddPer > 0 || liveEXP_AddPer > 0;
        }
    }

    public override string ToString()
    {
        string result = (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty) +
            (DEF_Add > 0 ? "防御力增加" + DEF_Add + "\n" : string.Empty) +
            (Hit_Add > 0 ? "命中增加" + Hit_Add + "\n" : string.Empty) +
            (Dodge_Add > 0 ? "闪避增加" + Dodge_Add + "\n" : string.Empty) +
            (Crit_Add > 0 ? "暴击增加" + Crit_Add + "\n" : string.Empty) +
            (HP_Add > 0 ? "体力增加" + HP_Add + "\n" : string.Empty) +
            (MP_Add > 0 ? "真气增加" + MP_Add + "\n" : string.Empty) +
            (WeightLoadAdd > 0 ? "负重能力增加" + WeightLoadAdd.ToString("F2") + "\n" : string.Empty) +
            (EXP_AddPer > 0 ? "经验获取量增加" + EXP_AddPer + "%\n" : string.Empty) +
            (liveEXP_AddPer > 0 ? "生活经验获取量增加" + liveEXP_AddPer + "%\n" : string.Empty)/* +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty) +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty) +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty)*/
            ;
        return result.Substring(0, result.Length - 1);
    }

    public string ToString(string format)
    {
        if (format != "/" || format != "-") return ToString();
        string result = (ATK_Add > 0 ? "攻击力增加" + ATK_Add + format : string.Empty) +
            (DEF_Add > 0 ? "防御力增加" + DEF_Add + format : string.Empty) +
            (Hit_Add > 0 ? "命中增加" + Hit_Add + format : string.Empty) +
            (Dodge_Add > 0 ? "闪避增加" + Dodge_Add + format : string.Empty) +
            (Crit_Add > 0 ? "暴击增加" + Crit_Add + format : string.Empty) +
            (HP_Add > 0 ? "体力增加" + HP_Add + format : string.Empty) +
            (MP_Add > 0 ? "真气增加" + MP_Add + format : string.Empty) +
            (WeightLoadAdd > 0 ? "负重能力增加" + WeightLoadAdd.ToString("F2") + format : string.Empty) +
            (EXP_AddPer > 0 ? "经验获取量增加" + EXP_AddPer + "%" + format : string.Empty) +
            (liveEXP_AddPer > 0 ? "生活经验获取量增加" + liveEXP_AddPer + "%" + format : string.Empty)/* +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty) +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty) +
            (ATK_Add > 0 ? "攻击力增加" + ATK_Add + "\n" : string.Empty)*/
            ;
        return result.Substring(0, result.Length - 1);
    }

    public static implicit operator bool(PowerUp self)
    {
        return self != null;
    }
}

public class BUFF
{
    private PowerUp buff;
    public PowerUp Buff
    {
        get
        {
            return buff;
        }
    }

    private float duration;
    public float Duration
    {
        get
        {
            return duration;
        }
    }

    public static implicit operator bool(BUFF self)
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