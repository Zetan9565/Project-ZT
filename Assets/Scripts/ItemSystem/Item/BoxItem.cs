using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "item box", menuName = "ZetanStudio/道具/箱子")]
public class BoxItem : ItemBase
{
    [SerializeField]
    private List<ItemInfo> itemsInBox = new List<ItemInfo>();
    public List<ItemInfo> ItemsInBox
    {
        get
        {
            return itemsInBox;
        }
    }

    public BoxItem()
    {
        itemType = ItemType.Box;
    }

    public override float Weight
    {
        get
        {
            float newWeight = weight;
            foreach (ItemInfo info in itemsInBox)
            {
                newWeight += info.Item.Weight * info.Amount;
            }
            return newWeight;
        }
    }

    public float EmptyWeight
    {
        get
        {
            return weight;
        }
    }
}
