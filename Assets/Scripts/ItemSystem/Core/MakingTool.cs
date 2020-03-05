using UnityEngine;

[DisallowMultipleComponent]
public class MakingTool : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [EnumMemberNames("未定义(在此组件无效)", "手工", "锻造炉", "织布机", "料理台", "制药台", "炼丹炉", "晾晒台", "臼和杵")]
#endif
    private MakingToolType toolType;
    public MakingToolType ToolType
    {
        get
        {
            return toolType;
        }

        private set
        {
            toolType = value;
        }
    }

    [SerializeField]
#if UNITY_EDITOR
    [DisplayName("制作耗时")]
#endif
    private float makingTime = 5f;
    public float MakingTime => makingTime;
}

public enum MakingToolType
{
    /// <summary>
    /// 未定义
    /// </summary>
    None,

    /// <summary>
    /// 手工
    /// </summary>
    Handwork,

    /// <summary>
    /// 锻造炉
    /// </summary>
    Forging,

    /// <summary>
    /// 织布机
    /// </summary>
    Loom,

    /// <summary>
    /// 缝纫台
    /// </summary>
    SewingTable,

    /// <summary>
    /// 料理台
    /// </summary>
    Kitchen,

    /// <summary>
    /// 制药台
    /// </summary>
    PharmaceuticalTable,

    /// <summary>
    /// 炼丹炉
    /// </summary>
    AlchemyFurnace,

    /// <summary>
    /// 晾晒台
    /// </summary>
    DryingTable,

    /// <summary>
    /// 臼和杵
    /// </summary>
    PestleAndMortar
}