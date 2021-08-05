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

    public bool IsEquipment => this is EquipmentItem;

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