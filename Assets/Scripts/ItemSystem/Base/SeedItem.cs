using UnityEngine;

public class SeedItem : ItemBase
{
    [SerializeField]
    private CropInformation crop;
    public CropInformation Crop => crop;

    public SeedItem()
    {
        itemType = ItemType.Seed;
    }
}
