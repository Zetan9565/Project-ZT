using UnityEngine;

[CreateAssetMenu(fileName = "material", menuName = "Zetan Studio/道具/材料")]
public class MaterialItem : ItemBase
{
    public MaterialItem()
    {
        itemType = ItemType.Material;
        usable = false;
    }

    public static string GetMaterialTypeString(MaterialType materialType)
    {
        switch (materialType)
        {
            case MaterialType.None:
                return "未定义道具";
            case MaterialType.Ore:
                return "矿石";
            case MaterialType.Metal:
                return "金属";
            case MaterialType.Plant:
                return "植物";
            case MaterialType.Cloth:
                return "布料";
            case MaterialType.Meat:
                return "肉类";
            case MaterialType.Fur:
                return "皮毛";
            case MaterialType.Fruit:
                return "水果";
            case MaterialType.Blueprint:
                return "图纸";
            case MaterialType.Liquid:
                return "液体";
            case MaterialType.Condiment:
                return "调料";
            default:
                return "任意道具";
        }
    }
}

public enum MaterialType
{
    [InspectorName("未定义")]
    None,//未定义：所有途径

    [InspectorName("矿石")]
    Ore,//矿石：采集、挖宝

    [InspectorName("金属")]
    Metal,//金属：加工、挖宝、掉落

    [InspectorName("植物")]
    Plant,//植物：采集、农场生产

    [InspectorName("布料")]
    Cloth,//布料：加工、挖宝、掉落

    [InspectorName("肉类")]
    Meat,//肉类：采集、农场生产

    [InspectorName("皮毛")]
    Fur,//皮毛：采集、农场生产

    [InspectorName("水果")]
    Fruit,//水果：采集、农场生产

    [InspectorName("图纸")]
    Blueprint,//图纸：购买、掉落、挖宝

    [InspectorName("液体")]
    Liquid,//液体：采集、农场生产、掉落

    [InspectorName("调料")]
    Condiment,//调料：加工、采集

    [InspectorName("任意")]
    Any//任意
}