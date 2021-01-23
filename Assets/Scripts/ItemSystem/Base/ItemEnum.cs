using UnityEngine;

public enum ItemType
{
    /// <summary>
    /// 其他
    /// </summary>
    Other,

    /// <summary>
    /// 药剂
    /// </summary>
    Medicine,

    /// <summary>
    /// 丹药
    /// </summary>
    Elixir,

    /// <summary>
    /// 菜肴
    /// </summary>
    Cuisine,

    /// <summary>
    /// 武器
    /// </summary>
    Weapon,

    /// <summary>
    /// 防具
    /// </summary>
    Armor,

    /// <summary>
    /// 首饰
    /// </summary>
    Jewelry,

    /// <summary>
    /// 盒子、箱子
    /// </summary>
    Box,

    /// <summary>
    /// 加工材料
    /// </summary>
    Material,

    /// <summary>
    /// 贵重品：用于贸易
    /// </summary>
    Valuables,

    /// <summary>
    /// 任务道具
    /// </summary>
    Quest,

    /// <summary>
    /// 采集工具
    /// </summary>
    Tool,

    /// <summary>
    /// 宝石
    /// </summary>
    Gemstone,

    /// <summary>
    /// 书籍
    /// </summary>
    Book,

    /// <summary>
    /// 袋子
    /// </summary>
    Bag,

    /// <summary>
    /// 种子
    /// </summary>
    Seed
}

public enum ItemQuality
{
    [InspectorName("凡品")]
    Normal,

    [InspectorName("精品")]
    Exquisite,

    [InspectorName("珍品")]
    Precious,

    [InspectorName("极品")]
    Best,

    [InspectorName("绝品")]
    Peerless,
}

public enum MakingMethod
{
    /// <summary>
    /// 不可制作
    /// </summary>
    [InspectorName("不可制作")]
    None,

    /// <summary>
    /// 手工：所有类型
    /// </summary>
    [InspectorName("手工")]
    Handmade,

    /// <summary>
    /// 冶炼：材料
    /// </summary>
    [InspectorName("冶炼")]
    Smelt,

    /// <summary>
    /// 锻造：装备、工具
    /// </summary>
    [InspectorName("锻造")]
    Forging,

    /// <summary>
    /// 织布：材料
    /// </summary>
    [InspectorName("织布")]
    Weaving,

    /// <summary>
    /// 裁缝：装备
    /// </summary>
    [InspectorName("裁缝")]
    Tailor,

    /// <summary>
    /// 烹饪：菜肴、Buff
    /// </summary>
    [InspectorName("烹饪")]
    Cooking,

    /// <summary>
    /// 炼丹：Buff、恢复剂
    /// </summary>
    [InspectorName("炼丹")]
    Alchemy,

    /// <summary>
    /// 制药：恢复剂
    /// </summary>
    [InspectorName("制药")]
    Pharmaceutical,

    /// <summary>
    /// 晾晒：材料、恢复剂
    /// </summary>
    [InspectorName("晾晒")]
    Season,

    /// <summary>
    /// 研磨：材料、恢复剂
    /// </summary>
    [InspectorName("研磨")]
    Triturate
}

public enum MakingType
{
    [InspectorName("单种道具")]
    SingleItem,//单种道具

    [InspectorName("同类道具")]
    SameType//同类道具
}