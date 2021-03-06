﻿using UnityEngine;

[DisallowMultipleComponent]
public class MakingTool : MonoBehaviour
{
    [SerializeField]
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
    [InspectorName("未定义")]
    None,

    /// <summary>
    /// 手工
    /// </summary>
    [InspectorName("手工")]
    Handwork,

    /// <summary>
    /// 锻造炉
    /// </summary>
    [InspectorName("锻造炉")]
    Forging,

    /// <summary>
    /// 织布机
    /// </summary>
    [InspectorName("织布机")]
    Loom,

    /// <summary>
    /// 缝纫台
    /// </summary>
    [InspectorName("缝纫台")]
    SewingTable,

    /// <summary>
    /// 料理台
    /// </summary>
    [InspectorName("料体台")]
    Kitchen,

    /// <summary>
    /// 制药台
    /// </summary>
    [InspectorName("制药台")]
    PharmaceuticalTable,

    /// <summary>
    /// 炼丹炉
    /// </summary>
    [InspectorName("炼丹炉")]
    AlchemyFurnace,

    /// <summary>
    /// 晾晒台
    /// </summary>
    [InspectorName("晾晒台")]
    DryingTable,

    /// <summary>
    /// 臼和杵
    /// </summary>
    [InspectorName("臼和杵")]
    PestleAndMortar
}