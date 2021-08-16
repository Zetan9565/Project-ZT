using UnityEngine;

[CreateAssetMenu(fileName = "currency", menuName = "Zetan Studio/道具/货币")]
public class CurrencyItem : ItemBase
{
    [SerializeField]
    private CurrencyType currencyType;
    public CurrencyType CurrencyType => currencyType;

    [SerializeField]
    private int valueEach = 1;
    public int ValueEach => valueEach;

    public CurrencyItem()
    {
        itemType = ItemType.Currency;
    }
}

public enum CurrencyType
{
    Money,
    EXP,
    SkillPoint,
}