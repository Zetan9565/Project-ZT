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
    private CropType cropType;
    public CropType CropType => cropType;

    [SerializeField]
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

    [SerializeField, NonReorderable]
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
    [InspectorName("全年")]
    All,

    [InspectorName("春季")]
    Spring,
    [InspectorName("春夏")]
    SpringAndSummer,

    [InspectorName("夏季")]
    Summer,
    [InspectorName("夏秋")]
    SummerAndAutumn,

    [InspectorName("秋季")]
    Autumn,
    [InspectorName("秋冬")]
    AutumnAndWinter,

    [InspectorName("冬季")]
    Winter,
    [InspectorName("冬春")]
    WinterAndSpring
}
public enum CropType
{
    [InspectorName("未定义")]
    Undefined,

    [InspectorName("蔬菜")]
    Vegetable,

    [InspectorName("水果")]
    Fruit,

    [InspectorName("材木")]
    Tree
}
[System.Serializable]
public class CropStage
{
    [SerializeField]
    private CropStages stage;
    public CropStages Stage => stage;

    [SerializeField]
    private int lastingDays = 1;
    public int LastingDays => lastingDays;

    public Sprite graph;

    [SerializeField]
    protected GatherType gatherType;
    public GatherType GatherType => gatherType;

    [SerializeField]
    protected float gatherTime;
    public float GatherTime => gatherTime;

    [SerializeField]
    private LootAgent lootPrefab;
    public LootAgent LootPrefab => lootPrefab;

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
    [InspectorName("播种期")]
    Seed,
    /// <summary>
    /// 幼苗期
    /// </summary>
    [InspectorName("幼苗期")]
    Seedling,
    /// <summary>
    /// 成长期
    /// </summary>
    [InspectorName("成长期")]
    Growing,
    /// <summary>
    /// 开花期
    /// </summary>
    [InspectorName("开花期")]
    Flowering,
    /// <summary>
    /// 结果期
    /// </summary>
    [InspectorName("结果期")]
    Bearing,
    /// <summary>
    /// 成熟期
    /// </summary>
    [InspectorName("成熟期")]
    Maturity,
    /// <summary>
    /// 过熟期
    /// </summary>
    [InspectorName("过熟期")]
    OverMature,
    /// <summary>
    /// 收割期
    /// </summary>
    [InspectorName("收割期")]
    Harvested,
    /// <summary>
    /// 枯萎期
    /// </summary>
    [InspectorName("枯萎期")]
    Withered,
    /// <summary>
    /// 腐朽期
    /// </summary>
    [InspectorName("腐朽期")]
    Decay
}