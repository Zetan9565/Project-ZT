using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gathering info", menuName = "ZetanStudio/采集物信息")]
public class GatheringInformation : ScriptableObject
{
    [SerializeField]
    protected string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    protected string _name;
    public new string name
    {
        get
        {
            return _name;
        }
    }

    [SerializeField]
    protected GatherType gatherType;
    public GatherType GatherType
    {
        get
        {
            return gatherType;
        }
    }

    [SerializeField]
    protected float gatherTime;
    public float GatherTime
    {
        get
        {
            return gatherTime;
        }
    }

    [SerializeField]
    protected float refreshTime;
    public float RefreshTime
    {
        get
        {
            return refreshTime;
        }
    }

    [SerializeField, NonReorderable]
    protected List<DropItemInfo> productItems = new List<DropItemInfo>();
    public List<DropItemInfo> ProductItems
    {
        get
        {
            return productItems;
        }
    }

    [SerializeField]
    private GameObject lootPrefab;
    public GameObject LootPrefab
    {
        get
        {
            return lootPrefab;
        }
    }
}
public enum GatherType
{
    /// <summary>
    /// 手采
    /// </summary>
    [InspectorName("手摘")]
    Hands,
    /// <summary>
    /// 斧子
    /// </summary>
    [InspectorName("用斧子砍")]
    Axe,
    /// <summary>
    /// 镐子
    /// </summary>
    [InspectorName("用稿子敲")]
    Shovel,
    /// <summary>
    /// 铲子
    /// </summary>
    [InspectorName("用铲子挖")]
    Spade,
    /// <summary>
    /// 锄头
    /// </summary>
    [InspectorName("用锄头翻")]
    Hoe
}