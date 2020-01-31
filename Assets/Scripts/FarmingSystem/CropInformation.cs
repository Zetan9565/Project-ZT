using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "crop info", menuName = "ZetanStudio/农作物信息")]
public class CropInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID => _ID;

    [SerializeField]
    private string _name;
    public new string name => _name;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("未定义", "蔬菜", "水果", "材木")]
#endif
    private CropType cropType;
    public CropType CropType => cropType;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("全年", "春季", "春夏", "夏季", "夏秋", "秋季", "秋冬", "冬季", "冬春")]
#endif
    private CropSeason plantSeason;
    public CropSeason PlantSeason
    {
        get
        {
            return plantSeason;
        }
    }

    public int Lifespan
    {
        get
        {
            int value = 0;
            for (int i = 0; i < stages.Count; i++)
            {
                value += stages[i].LastingDays;
            }
            return value;
        }
    }

    [SerializeField]
    private List<CropStage> stages = new List<CropStage>()
    {
        new CropStage(1, CropStages.Seed ),
        new CropStage(3, CropStages.Growing),
        new CropStage(2, CropStages.Maturity),
        new CropStage(1, CropStages.Withered),
    };
    public List<CropStage> Stages
    {
        get
        {
            return stages;
        }
    }

    public bool IsValid
    {
        get
        {
            return Stages.Count > 0 && !Stages.Exists(x => !x.graph);
        }
    }
}
public enum CropSeason
{
    All,
    Spring,
    SpringAndSummer,
    Summer,
    SummerAndAutumn,
    Autumn,
    AutumnAndWinter,
    Winter,
    WinterAndSpring
}
public enum CropType
{
    Undefined,
    Vegetable,
    Fruit,
    Tree
}
[System.Serializable]
public class CropStage
{
#if UNITY_EDITOR
    [EnumMemberNames("种子期", "幼苗期", "成长期", "开花期", "结果期", "成熟期", "过熟期", "收割期", "枯萎期", "腐朽期")]
#endif
    [SerializeField]
    private CropStages stage;
    public CropStages Stage => stage;

    [SerializeField]
    private int lastingDays = 1;
    public int LastingDays => lastingDays;

    public Sprite graph;

    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("手动", "斧子", "镐子", "铲子", "锄头")]
#endif
    protected GatherType gatherType;
    public GatherType GatherType => gatherType;

    [SerializeField]
    protected float gatherTime;
    public float GatherTime => gatherTime;

    [SerializeField]
    private GameObject lootPrefab;
    public GameObject LootPrefab => lootPrefab;

    public bool HarvestAble => productItems.Count > 0 && repeatTimes > 0;
    public bool RepeatAble
    {
        get
        {
            return repeatTimes > 1;
        }
    }

    [SerializeField]
    private int repeatTimes = 1;
    public int RepeatTimes => repeatTimes;

    [SerializeField]
    private int indexToReturn;
    public int IndexToReturn => indexToReturn;

    [SerializeField]
    protected List<DropItemInfo> productItems = new List<DropItemInfo>();
    public List<DropItemInfo> ProductItems => productItems;

    public CropStage(int lastingDays, CropStages stage)
    {
        this.lastingDays = lastingDays;
        this.stage = stage;
    }

    public static implicit operator bool(CropStage self)
    {
        return self != null;
    }
}
public enum CropStages
{
    /// <summary>
    /// 种子期
    /// </summary>
    Seed,
    /// <summary>
    /// 幼苗期
    /// </summary>
    Seedling,
    /// <summary>
    /// 成长期
    /// </summary>
    Growing,
    /// <summary>
    /// 开花期
    /// </summary>
    Flowering,
    /// <summary>
    /// 结果期
    /// </summary>
    Bearing,
    /// <summary>
    /// 成熟期
    /// </summary>
    Maturity,
    /// <summary>
    /// 过熟期
    /// </summary>
    OverMature,
    /// <summary>
    /// 收割期
    /// </summary>
    Harvested,
    /// <summary>
    /// 枯萎期
    /// </summary>
    Withered,
    /// <summary>
    /// 腐朽期
    /// </summary>
    Decay
}