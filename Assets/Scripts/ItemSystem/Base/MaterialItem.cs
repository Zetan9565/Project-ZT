using UnityEngine;

[CreateAssetMenu(fileName = "material", menuName = "ZetanStudio/道具/材料")]
public class MaterialItem : ItemBase
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("未定义", "矿石", "金属", "植物", "布料", "肉类", "皮毛", "水果", "图纸", "调料")]
#endif
    private MaterialType materialType;
    public MaterialType MaterialType
    {
        get
        {
            return materialType;
        }
    }

    public MaterialItem()
    {
        itemType = ItemType.Material;
        usable = false;
    }
}

public enum MaterialType
{
    Other,//未定义：所有途径
    Ore,//矿石：采集、挖宝
    Metal,//金属：加工、挖宝、掉落
    Plant,//植物：采集、农场生产
    Cloth,//布料：加工、挖宝、掉落
    Meat,//肉类：采集、农场生产
    Fur,//皮毛：采集、农场生产
    Fruit,//水果：采集、农场生产
    Blueprint,//图纸：购买、掉落、挖宝
    Liquid,//液体：采集、农场生产、掉落
    Condiment//调料：加工、采集
}
