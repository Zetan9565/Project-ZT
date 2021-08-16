using UnityEngine;

[CreateAssetMenu(fileName = "bag", menuName = "Zetan Studio/道具/袋子")]
public class BagItem : ItemBase
{
    [SerializeField]
    private int expandSize = 1;
    public int ExpandSize
    {
        get
        {
            return expandSize;
        }
    }

    public BagItem()
    {
        itemType = ItemType.Bag;
        stackAble = false;
    }
}