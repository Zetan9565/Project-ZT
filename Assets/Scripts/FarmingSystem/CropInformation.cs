using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "crop info", menuName = "ZetanStudio/农作物信息")]
public class CropInformation : ScriptableObject
{
    [SerializeField]
    private string _ID;
    public string ID
    {
        get
        {
            return _ID;
        }
    }

    [SerializeField]
    private GatheringInformation gatheringInfo;
    public GatheringInformation GatheringInfo
    {
        get
        {
            return gatheringInfo;
        }
    }

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

    [SerializeField]
    private int growthTime = 7;
    public int GrowthTime
    {
        get
        {
            return growthTime;
        }
    }

    [SerializeField]
    private TimeUnit timeUnit = TimeUnit.Day;
    public TimeUnit TimeUnit
    {
        get
        {
            return timeUnit;
        }
    }

    public bool CanRepeat
    {
        get
        {
            return repeatTimes > 1 && stages.Count > 4;
        }
    }

    [SerializeField]
    private int repeatTimes = 2;
    public int RepeatTimes
    {
        get
        {
            return repeatTimes;
        }
    }

    [SerializeField]
    private ScopeInt repeatStage = new ScopeInt(1, 2);
    public ScopeInt RepeatStage
    {
        get
        {
            return repeatStage;
        }
    }

    [SerializeField]
    private List<CropStage> stages = new List<CropStage>()
    {
        new CropStage() { stage = CropStages.Seed },
        new CropStage(2) { stage = CropStages.Growing },
        new CropStage(6) { stage = CropStages.Maturity },
        new CropStage(7) { stage = CropStages.Withered },
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
            return Stages.Count > 0 && GatheringInfo && !Stages.Exists(x => !x.graph);
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

[System.Serializable]
public class CropStage
{
#if UNITY_EDITOR
    [EnumMemberNames("种子期", "幼苗期", "成长期", "开花期", "结果期", "成熟期", "过熟期", "收割期", "枯萎期", "腐朽期")]
#endif
    public CropStages stage;
    [SerializeField]
    private ScopeInt lifespanPer;
    public ScopeFloat LifespanPer
    {
        get
        {
            return lifespanPer * 0.01f;
        }
    }

    public Sprite graph;

    public CropStage(int from = 0, int to = 100)
    {
        lifespanPer = new ScopeInt(from, to);
    }

    public static implicit operator bool(CropStage self)
    {
        return self != null;
    }
}

public enum CropStages
{
    Seed,//种子期
    Seedling,//幼苗期
    Growing,//成长期
    Flowering,//开花期
    Bearing,//结果期
    Maturity,//成熟期
    OverMature,//过熟期
    Harvested,//收割期
    Withered,//枯萎期
    Decay//腐朽期
}