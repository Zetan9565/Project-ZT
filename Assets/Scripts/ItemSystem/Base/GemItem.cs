using UnityEngine;

[CreateAssetMenu(fileName = "gemstone", menuName = "ZetanStudio/道具/宝石")]
public class GemItem : ItemBase
{
    [SerializeField]
    private RoleAttributeGroup powerup = new RoleAttributeGroup();
    public RoleAttributeGroup Powerup
    {
        get
        {
            return powerup;
        }
    }

    public GemItem()
    {
        itemType = ItemType.Gemstone;
    }
}