using UnityEngine;

[CreateAssetMenu(fileName = "weapon", menuName = "Zetan Studio/道具/武器")]
[System.Serializable]
public class WeaponItem : EquipmentItem
{
    [SerializeField]
    private WeaponType weaponType;
    public WeaponType WeaponType
    {
        get
        {
            return weaponType;
        }
    }

    public bool IsPrimary
    {
        get
        {
            return weaponType != WeaponType.SortBow;
        }
    }


    public WeaponItem()
    {
        itemType = ItemType.Weapon;
        stackAble = false;
    }

}

public enum WeaponType
{
    Sword,
    Blade,
    Spear,
    SortBow
}