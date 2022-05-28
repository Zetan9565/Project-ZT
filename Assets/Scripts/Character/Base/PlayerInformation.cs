using UnityEngine;

[CreateAssetMenu(fileName = "player info", menuName = "Zetan Studio/角色/玩家信息")]
public class PlayerInformation : CharacterInformation
{
    protected CharacterSex sex;
    public CharacterSex Sex => sex;

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
        return false;

        //if (toEquip == null || !toEquip.item_old) return false;
        //if (!toEquip.item_old.IsWeapon) return false;
        //WeaponItem weapon = toEquip.item_old as WeaponItem;
        ////ATK += weapon.CutATK + weapon.Powerup.ATK_Add +
        ////    (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.ATK_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.ATK_Add : 0);
        ////DEF += weapon.DEF + weapon.Powerup.DEF_Add +
        ////    (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.DEF_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.DEF_Add : 0);
        ////Hit += weapon.Hit + weapon.Powerup.Hit_Add +
        ////    (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.Hit_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.Hit_Add : 0);

        ////HP.Max += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.HP_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.HP_Add : 0);
        ////MP.Max += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.MP_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.MP_Add : 0);
        ////Dodge += (toEquip.gemstone1 ? toEquip.gemstone1.Powerup.Dodge_Add : 0) + (toEquip.gemstone2 ? toEquip.gemstone2.Powerup.Dodge_Add : 0);

        //if (weapon.IsPrimary) primaryWeapon = toEquip;
        //else secondaryWeapon = toEquip;

        //return true;
    }
    public ItemInfo UnequipWeapon(bool primary)
    {
        return null;
        //if (primaryWeapon == null && secondaryWeapon == null) return null;
        //WeaponItem weapon;
        //ItemInfo info;
        //if (primary)
        //{
        //    info = primaryWeapon;
        //    weapon = primaryWeapon.item_old as WeaponItem;
        //    primaryWeapon = null;
        //}
        //else
        //{
        //    info = secondaryWeapon;
        //    weapon = secondaryWeapon.item_old as WeaponItem;
        //    secondaryWeapon = null;
        //}
        //if (weapon)
        //{
        //    //ATK -= weapon.CutATK + weapon.Powerup.ATK_Add;
        //    //DEF -= weapon.DEF + weapon.Powerup.DEF_Add;
        //    //Hit -= weapon.Hit + weapon.Powerup.Hit_Add;

        //    //HP.Max -= (info.gemstone1 ? info.gemstone1.Powerup.HP_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.HP_Add : 0);
        //    //MP.Max -= (info.gemstone1 ? info.gemstone1.Powerup.MP_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.MP_Add : 0);
        //    //Dodge -= (info.gemstone1 ? info.gemstone1.Powerup.Dodge_Add : 0) + (info.gemstone2 ? info.gemstone2.Powerup.Dodge_Add : 0);
        //}
        //return info;
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
}