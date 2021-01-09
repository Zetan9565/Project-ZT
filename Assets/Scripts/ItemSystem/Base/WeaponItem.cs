using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ZetanStudio/道具/武器")]
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

    [SerializeField]
    private int cutATK;
    public int CutATK
    {
        get
        {
            return cutATK;
        }
    }
    [SerializeField]
    private int punATK;
    public int PunATK
    {
        get
        {
            return punATK;
        }
    }
    [SerializeField]
    private int bluATK;
    public int BluATK
    {
        get
        {
            return bluATK;
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
    }

    [SerializeField]
    private int hit;
    public int Hit
    {
        get
        {
            return hit;
        }
    }

    [SerializeField]
    private PowerUp powerup = new PowerUp();
    public PowerUp Powerup
    {
        get
        {
            return powerup;
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