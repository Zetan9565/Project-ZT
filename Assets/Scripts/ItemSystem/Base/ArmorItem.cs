using UnityEngine;

[CreateAssetMenu(fileName = "armor", menuName = "Zetan Studio/道具/防具")]
public class ArmorItem : EquipmentItem
{
    [SerializeField]
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
    private int flee;
    public int Flee
    {
        get
        {
            return flee;
        }
    }

    public ArmorItem()
    {
        itemType = ItemType.Armor;
        stackNum = 1;
    }
}

public enum ArmorType
{
    [InspectorName("盔甲")]
    Armor,

    [InspectorName("头盔")]
    Helmet,

    [InspectorName("靴子")]
    Boots,

    [InspectorName("护手")]
    Gloves,

    [InspectorName("衣服")]
    Clothes
}