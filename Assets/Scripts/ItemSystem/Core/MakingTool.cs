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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && MakingManager.Instance.CurrentTool != this)
            MakingManager.Instance.CanMake(this);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && MakingManager.Instance.CurrentTool != this)
            MakingManager.Instance.CanMake(this);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && MakingManager.Instance.CurrentTool == this)
            MakingManager.Instance.CannotMake();
    }

    public static string ToolTypeToString(MakingToolType toolType)
    {
        switch (toolType)
        {
            case MakingToolType.Handwork:
                return "手工";
            case MakingToolType.Forging:
                return "锻造炉";
            case MakingToolType.Loom:
                return "织布机";
            case MakingToolType.SewingTable:
                return "缝纫台";
            case MakingToolType.Kitchen:
                return "料理台";
            case MakingToolType.PharmaceuticalTable:
                return "制药台";
            case MakingToolType.AlchemyFurnace:
                return "炼丹炉";
            case MakingToolType.DryingTable:
                return "晾晒台";
            case MakingToolType.PestleAndMortar:
                return "臼和杵";
            case MakingToolType.None:
            default:
                return "未定义";
        }
    }
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