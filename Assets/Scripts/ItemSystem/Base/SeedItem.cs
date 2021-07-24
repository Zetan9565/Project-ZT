using UnityEngine;

[CreateAssetMenu(fileName ="seed", menuName ="ZetanStudio/道具/种子")]
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
