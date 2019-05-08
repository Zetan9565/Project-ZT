using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "armor", menuName = "ZetanStudio/道具/防具")]
public class ArmorItem : ItemBase
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("盔甲", "头盔", "靴子", "护手", "衣服")]
#endif
    private ArmorType armorType;
    public ArmorType ArmorType
    {
        get
        {
            return armorType;
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
    private int dodge;
    public int Dodge
    {
        get
        {
            return dodge;
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

    public ArmorItem()
    {
        itemType = ItemType.Armor;
        stackAble = false;
    }
}

public enum ArmorType
{
    Armor,
    Helmet,
    Boots,
    Gloves,
    Clothes
}