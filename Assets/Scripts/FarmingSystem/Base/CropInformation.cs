using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "crop info", menuName = "Zetan Studio/种植/农作物信息")]
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
    private int temperature = 20;
    public int Temperature => temperature;

    [SerializeField]
    private int humidity = 50;
    public int Humidity => humidity;

    [SerializeField]
    private int size = 1;
    public int Size => size;

    [SerializeField]
    private Crop prefab;
    public Crop Prefab => prefab;

    [SerializeField]
    private CropPreview previewPrefab;
    public CropPreview PreviewPrefab => previewPrefab;

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
                value += Mathf.Abs(stages[i].LastingDays);
            }
            return value;
        }
    }

    [SerializeField, NonReorderable]
    private List<CropStage> stages = new List<CropStage>()
    {
        new CropStage(1, CropStageType.Seed ),
        new CropStage(3, CropStageType.Growing),
        new CropStage(2, CropStageType.Maturity),
        new CropStage(1, CropStageType.Withered),
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
            return Stages.Count > 0 && !Stages.Exists(x => !x.Graph);
        }
    }

    public static string CropSeasonString(CropSeason season)
    {
        switch (season)
        {
            case CropSeason.Spring:
                return "春季";
            case CropSeason.SpringAndSummer:
                return "春夏";
            case CropSeason.Summer:
                return "夏季";
            case CropSeason.SummerAndAutumn:
                return "夏秋";
            case CropSeason.Autumn:
                return "秋季";
            case CropSeason.AutumnAndWinter:
                return "秋冬";
            case CropSeason.Winter:
                return "冬季";
            case CropSeason.WinterAndSpring:
                return "冬春";
            case CropSeason.All:
                return "全年";
            default:
                return "全年";
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

public delegate void CropStageListner(CropStage current);
[System.Serializable]
public class CropStage
{
    [SerializeField]
    private CropStageType stage;
    public CropStageType Stage => stage;

    [SerializeField]
    private int lastingDays = 1;
    public int LastingDays => lastingDays;

    [SerializeField]
    private Sprite graph;
    public Sprite Graph => graph;

    public bool HarvestAble => gatherInfo && gatherInfo.ProductItems.Count > 0 && repeatTimes != 0;
    public bool RepeatAble
    {
        get
        {
            return repeatTimes < 0 || repeatTimes > 1;
        }
    }

    [SerializeField]
    private int repeatTimes = 1;
    public int RepeatTimes => repeatTimes;

    [SerializeField]
    private int indexToReturn;
    public int IndexToReturn => indexToReturn;

    [SerializeField]
    private GatheringInformation gatherInfo;
    public GatheringInformation GatherInfo => gatherInfo;


    public CropStage(int lastingDays, CropStageType stage)
    {
        this.lastingDays = lastingDays;
        this.stage = stage;
    }

    public static implicit operator bool(CropStage self)
    {
        return self != null;
    }

    public static string CropStageName(CropStageType stage)
    {
        switch (stage)
        {
            case CropStageType.Seed:
                return "播种期";
            case CropStageType.Seedling:
                return "幼苗期";
            case CropStageType.Growing:
                return "成长期";
            case CropStageType.Flowering:
                return "开花期";
            case CropStageType.Bearing:
                return "结果期";
            case CropStageType.Maturity:
                return "成熟期";
            case CropStageType.OverMature:
                return "过熟期";
            case CropStageType.Harvested:
                return "收割期";
            case CropStageType.Withered:
                return "枯萎期";
            case CropStageType.Decay:
                return "腐朽期";
            default:
                return "未知";
        }
    }
}
public enum CropStageType
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