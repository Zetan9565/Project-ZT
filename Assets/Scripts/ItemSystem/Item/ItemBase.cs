using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public abstract class ItemBase : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public virtual string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    protected string _Name;
    public virtual string Name
    {
        get
        {
            return _Name;
        }
    }

    [SerializeField]
    protected ItemType itemType = ItemType.Other;
    public virtual ItemType ItemType
    {
        get
        {
            return itemType;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("凡品", "精品", "珍品", "极品", "绝品")]
#endif
    protected ItemQuality quality;
    public virtual ItemQuality Quality
    {
        get
        {
            return quality;
        }
    }

    [SerializeField]
    protected float weight;
    public virtual float Weight
    {
        get
        {
            return weight;
        }
    }

    [SerializeField]
    protected bool sellAble = true;
    public virtual bool SellAble
    {
        get
        {
            return sellAble;
        }
    }

    [SerializeField]
    protected int sellPrice;
    public virtual int SellPrice
    {
        get
        {
            return sellPrice;
        }
    }

    [SerializeField]
    protected int buyPrice;
    public virtual int BuyPrice
    {
        get
        {
            return buyPrice;
        }
    }

    [SerializeField]
    protected Sprite icon;
    public virtual Sprite Icon
    {
        get
        {
            return icon;
        }
    }

    [SerializeField, TextArea]
    protected string description;
    public virtual string Description
    {
        get
        {
            return description;
        }
    }

    [SerializeField]
    protected bool stackAble = true;
    public virtual bool StackAble
    {
        get
        {
            return stackAble;
        }
    }

    [SerializeField]
    protected bool discardAble = true;
    public virtual bool DiscardAble
    {
        get
        {
            return discardAble;
        }
    }

    [SerializeField]
    protected bool useable = true;
    public virtual bool Useable
    {
        get
        {
            return useable;
        }
    }

    [SerializeField]
    protected bool inexhaustible;//用之不竭
    public virtual bool Inexhaustible
    {
        get
        {
            return inexhaustible;
        }
    }

    [SerializeField]
    private int maxDurability = 100;
    public virtual int MaxDurability
    {
        get
        {
            return maxDurability;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("不可制作", "冶炼", "锻造", "裁缝", "烹饪", "炼丹", "制药", "晾晒")]
#endif
    protected ProcessMethod processMethod;
    public virtual ProcessMethod ProcessMethod
    {
        get
        {
            return processMethod;
        }
    }

    [SerializeField]
    protected List<ProcessItemInfo> materials = new List<ProcessItemInfo>();
    public virtual List<ProcessItemInfo> Materials
    {
        get
        {
            return materials;
        }
    }

    public bool IsWeapon
    {
        get
        {
            return this is WeaponItem;
        }
    }

    public bool IsBox
    {
        get
        {
            return this is BoxItem;
        }
    }

    public bool IsMaterial
    {
        get
        {
            return this is MaterialItem;
        }
    }

    public bool IsGemstone
    {
        get
        {
            return this is GemItem;
        }
    }

    public bool IsForQuest
    {
        get
        {
            return this is QuestItem;
        }
    }

    public bool IsEquipment
    {
        get
        {
            return this is WeaponItem;
        }
    }

    public bool IsConsumable
    {
        get
        {
            return this is BoxItem;
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
    DanMedicine,

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
}

public enum ItemQuality
{
    /// <summary>
    /// 凡品
    /// </summary>
    Normal,

    /// <summary>
    /// 精品
    /// </summary>
    Exquisite,

    /// <summary>
    /// 珍品
    /// </summary>
    Precious,

    /// <summary>
    /// 极品
    /// </summary>
    Best,

    /// <summary>
    /// 绝品
    /// </summary>
    Peerless,
}

public enum ProcessMethod
{
    /// <summary>
    /// 不可制作
    /// </summary>
    None,

    /// <summary>
    /// 冶炼：材料
    /// </summary>
    Smelt,

    /// <summary>
    /// 锻造：装备、工具
    /// </summary>
    Forging,

    /// <summary>
    /// 裁缝：装备
    /// </summary>
    Tailor,

    /// <summary>
    /// 烹饪：菜肴、Buff
    /// </summary>
    Cooking,

    /// <summary>
    /// 炼丹：Buff、恢复剂
    /// </summary>
    Alchemy,

    /// <summary>
    /// 制药：恢复剂
    /// </summary>
    Pharmaceutical,

    /// <summary>
    /// 晾晒：材料、恢复剂
    /// </summary>
    Season
}
public enum ProcessType
{
    SingleItem,//单种道具
    SameType//同类道具
}
#endregion

#region 道具信息相关
[Serializable]
public class ItemInfo : ICloneable//在这个类进行拓展，如强化、词缀、附魔
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

    public ItemInfo()
    {
        amount = 1;
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

    public ItemInfo(ItemBase item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public ItemInfo(ItemInfo info)
    {
        item = info.Item;
        amount = info.Amount;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public void UseItem()
    {
        if (!item) return;
        if (!item.Useable)
        {
            MessageManager.Instance.NewMessage("该物品不可使用");
            return;
        }
        if (item.IsBox) UseBox();
    }

    void UseBox()
    {
        BoxItem box = item as BoxItem;
        if (BackpackManager.Instance.LoseItem(this))
        {
            foreach (ItemInfo info in box.ItemsInBox)
            {
                if (!BackpackManager.Instance.TryGetItem_Boolean(info))
                {
                    BackpackManager.Instance.GetItem(this);
                    return;
                }
            }
            foreach (ItemInfo info in box.ItemsInBox)
            {
                BackpackManager.Instance.GetItem(info);
            }
        }
    }

    #region 装备相关
    [HideInInspector]
    public ScopeInt durability;//耐久度

    public int gemSlotAmount;
    #endregion
}

[Serializable]
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

[Serializable]
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
#if UNITY_EDITOR
    [EnumMemberNames("稍微喜欢", "喜欢", "着迷", "狂热")]
#endif
    private FavoriteLevel favoriteLevel = FavoriteLevel.Little;
    public FavoriteLevel FavoriteLevel
    {
        get
        {
            return favoriteLevel;
        }
    }
}

public enum FavoriteLevel
{
    Little = 10,//稍微喜欢+10
    Fond = 30,//喜欢+30
    Fascinated = 100,//着迷+100
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
#if UNITY_EDITOR
    [EnumMemberNames("不喜欢", "稍微讨厌", "讨厌", "深恶痛绝")]
#endif
    private HateLevel hateLevel = HateLevel.Dislike;
    public HateLevel HateLevel
    {
        get
        {
            return hateLevel;
        }
    }
}

public enum HateLevel
{
    Dislike = 10,//不喜欢-10
    Little = 30,//稍微讨厌-30
    Hate = 100,//讨厌-100
    Detest = 300//深恶痛绝-300
}
#endregion