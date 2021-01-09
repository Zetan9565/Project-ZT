using UnityEngine;

[CreateAssetMenu(fileName = "player info", menuName = "ZetanStudio/角色/玩家信息")]
public class PlayerInformation : CharacterInformation
{
    public Backpack backpack;

    #region 装备相关
    public ItemInfo primaryWeapon;

    public ItemInfo secondaryWeapon;

    public ItemInfo armor;

    public ItemInfo helmet;

    public ItemInfo boots;

    public ItemInfo gloves;

    public ItemInfo clothes;

    public ItemInfo ring1;

    public ItemInfo ring2;

    public bool EquipWeapon(ItemInfo toEquip)
    {
        if (toEquip == null || !toEquip.item) return false;
        if (!toEquip.item.IsWeapon) return false;
        WeaponItem weapon = toEquip.item as WeaponItem;
        ATK += weapon.CutATK + weapon.Powerup.ATK_Add +
            (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.ATK_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.ATK_Add : 0);
        DEF += weapon.DEF + weapon.Powerup.DEF_Add +
            (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.DEF_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.DEF_Add : 0);
        Hit += weapon.Hit + weapon.Powerup.Hit_Add +
            (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.Hit_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.Hit_Add : 0);

        HP.Max += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.HP_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.HP_Add : 0);
        MP.Max += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.MP_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.MP_Add : 0);
        Dodge += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.Dodge_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.Dodge_Add : 0);

        if (weapon.IsPrimary) primaryWeapon = toEquip;
        else secondaryWeapon = toEquip;

        return true;
    }
    public ItemInfo UnequipWeapon(bool primary)
    {
        if (primaryWeapon == null && secondaryWeapon == null) return null;
        WeaponItem weapon;
        ItemInfo info;
        if (primary)
        {
            info = primaryWeapon;
            weapon = primaryWeapon.item as WeaponItem;
            primaryWeapon = null;
        }
        else
        {
            info = secondaryWeapon;
            weapon = secondaryWeapon.item as WeaponItem;
            secondaryWeapon = null;
        }
        if (weapon)
        {
            ATK -= weapon.CutATK + weapon.Powerup.ATK_Add;
            DEF -= weapon.DEF + weapon.Powerup.DEF_Add;
            Hit -= weapon.Hit + weapon.Powerup.Hit_Add;

            HP.Max -= (info.gemstone1 ? info.gemstone1.Powerup.HP_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.HP_Add : 0);
            MP.Max -= (info.gemstone1 ? info.gemstone1.Powerup.MP_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.MP_Add : 0);
            Dodge -= (info.gemstone1 ? info.gemstone1.Powerup.Dodge_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.Dodge_Add : 0);
        }
        return info;
    }

    public bool HasPrimaryWeapon
    {
        get
        {
            return primaryWeapon != null && primaryWeapon.item;
        }
    }

    public bool HasSecondaryWeapon
    {
        get
        {
            return secondaryWeapon != null && secondaryWeapon.item;
        }
    }
    #endregion

    #region 能力相关
    [SerializeField]
    private RoleAttributeGroup attribute;
    public RoleAttributeGroup Attribute => attribute;

    [SerializeField]
    private ScopeInt _HP = new ScopeInt(150) { Current = 150 };
    public ScopeInt HP
    {
        get
        {
            return _HP;
        }
    }

    [SerializeField]
    private ScopeInt _MP = new ScopeInt(50) { Current = 50 };
    public ScopeInt MP
    {
        get
        {
            return _MP;
        }
    }

    [SerializeField]
    private int _ATK;
    public int ATK
    {
        get
        {
            return _ATK;
        }

        private set
        {
            _ATK = value;
        }
    }

    [SerializeField]
    private int _DEF;
    public int DEF
    {
        get
        {
            return _DEF;
        }

        private set
        {
            _DEF = value;
        }
    }

    [SerializeField]
    private int hit;
    public int Hit
    {
        get
        {
            return hit;
        }

        private set
        {
            hit = value;
        }
    }

    [SerializeField]
    private int dodge;
    public int Dodge
    {
        get
        {
            return dodge;
        }

        private set
        {
            dodge = value;
        }
    }

    #endregion

    public int level;
}