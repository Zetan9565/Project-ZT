using System;
using UnityEngine;

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