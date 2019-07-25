using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gathering info", menuName = "ZetanStudio/采集物信息")]
public class GatheringInformation : ScriptableObject
{
    [SerializeField]
    private string _name;
    public new string name
    {
        get
        {
            return _name;
        }
    }

    [SerializeField]
    private GatherType gatherType;
    public GatherType GatherType
    {
        get
        {
            return gatherType;
        }
    }

    [SerializeField]
    private float gatherTime;
    public float GatherTime
    {
        get
        {
            return gatherTime;
        }
    }

    [SerializeField]
    private float refreshTime;
    public float RefreshTime
    {
        get
        {
            return refreshTime;
        }
    }

    [SerializeField]
    private List<DropItemInfo> productItems = new List<DropItemInfo>();
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
    Ore,
    Plant
}