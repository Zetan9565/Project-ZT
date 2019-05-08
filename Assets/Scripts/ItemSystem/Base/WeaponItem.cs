using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ZetanStudio/道具/武器")]
[System.Serializable]
public class WeaponItem : ItemBase
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
    private int _ATK;
    public int ATK
    {
        get
        {
            return _ATK;
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
    [Range(0, 2)]
    private int gemSlotAmount = 0;
    public int GemSlotAmout
    {
        get
        {
            return gemSlotAmount;
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