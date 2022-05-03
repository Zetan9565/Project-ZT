using UnityEngine;

public class BackpackManager : SingletonInventoryHandler<BackpackManager>
{
    [SerializeField, HideWhenPlaying]
    protected bool ignoreLock = false;
    [SerializeField, HideWhenPlaying]
    protected int defaultSpaceLimit = 30;
    [SerializeField]
    private int maxSpaceLimit = 0;
    [SerializeField, HideWhenPlaying, Tooltip("不大于0表示不限制负重")]
    protected float defaultWeightLimit = 100.0f;
    [SerializeField]
    private float maxWeightLimit = 0;

    private bool CheckQuest(ItemData data, int amount)
    {
        if (QuestManager.Instance.HasQuestRequiredItem(data.Model_old, Inventory.GetAmount(data.ModelID) - amount))
        {
            MessageManager.Instance.New($"部分[{ItemUtility.GetColorName(data.Model_old)}]已被任务锁定");
            return false;
        }
        return true;
    }
    private bool TryGetCurrency(ItemData data, int amount)
    {
        if (data.Model_old is CurrencyItem currency)
            switch (currency.CurrencyType)
            {
                case CurrencyType.Money:
                    GetMoney(currency.ValueEach * amount);
                    Debug.Log("获得钱");
                    return true;
                case CurrencyType.EXP:
                    //TODO 获得经验
                    Debug.Log("获得经验");
                    return true;
                case CurrencyType.SkillPoint:
                    break;
                case CurrencyType.SkillEXP:
                    break;
                default:
                    break;
            }
        return false;
    }

    /// <summary>
    /// 扩展容量
    /// </summary>
    /// <param name="space">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool ExpandSpace(int space)
    {
        if (space < 1) return false;
        if (maxSpaceLimit > 0 && Inventory.SpaceLimit >= maxSpaceLimit)
        {
            MessageManager.Instance.New(Name + "已经达到最大容量了");
            return false;
        }
        int finallyExpand = maxSpaceLimit > 0 ? (Inventory.SpaceLimit + space > maxSpaceLimit ? maxSpaceLimit - Inventory.SpaceLimit : space) : space;
        Inventory.ExpandSpace(finallyExpand);
        NotifyCenter.PostNotify(BackpackSpaceChanged);
        MessageManager.Instance.New(Name + "空间增加了" + finallyExpand);
        return true;
    }

    /// <summary>
    /// 扩展负重
    /// </summary>
    /// <param name="weightLoad">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool ExpandLoad(float weightLoad)
    {
        if (weightLoad < 0.01f) return false;
        if (maxWeightLimit > 0 && Inventory.WeightLimit >= maxWeightLimit)
        {
            MessageManager.Instance.New(Name + "已经达到最大扩展载重了");
            return false;
        }
        float finallyExpand = maxWeightLimit > 0 ? (Inventory.WeightLimit + weightLoad > maxWeightLimit ? maxWeightLimit - Inventory.WeightLimit : weightLoad) : weightLoad;
        Inventory.ExpandLoad(weightLoad);
        NotifyCenter.PostNotify(BackpackWeightChanged);
        MessageManager.Instance.New(Name + "载重增加了" + finallyExpand.ToString("F2"));
        return true;
    }

    protected override void OnAwake()
    {
        Inventory = new Inventory(defaultSpaceLimit, defaultWeightLimit, ignoreLock, customGetAction: TryGetCurrency, customLoseChecker: CheckQuest);
        ListenInventoryChange(true);
    }

    #region 消息定义
    public const string BackpackMoneyChanged = "BackpackMoneyChanged";
    public const string BackpackSpaceChanged = "BackpackSpaceChanged";
    public const string BackpackWeightChanged = "BackpackWeightChanged";
    public const string BackpackItemAmountChanged = "BackpackItemAmountChanged";
    public const string BackpackSlotStateChanged = "BackpackSlotStateChanged";
    public const string BackpackUseItem = "BackpackUseItem";

    public override string InventoryMoneyChangedMsgKey => BackpackMoneyChanged;
    public override string InventorySpaceChangedMsgKey => BackpackSpaceChanged;
    public override string InventoryWeightChangedMsgKey => BackpackWeightChanged;
    public override string ItemAmountChangedMsgKey => BackpackItemAmountChanged;
    public override string SlotStateChangedMsgKey => BackpackSlotStateChanged;
    #endregion

    #region 道具使用相关
    public void UseItem(ItemData item)
    {
        if (!item.Model_old.Usable)
        {
            MessageManager.Instance.New("该物品不可使用");
            return;
        }
        bool used = false;
        if (item.Model_old.IsBox) used = UseBox(item);
        else if (item.Model_old.IsEquipment) used = UseEuipment(item);
        else if (item.Model_old.IsBook) used = UseBook(item);
        else if (item.Model_old.IsBag) used = UseBag(item);
        else if (item.Model_old.IsForQuest) used = UseQuest(item);
        if (used) NotifyCenter.PostNotify(BackpackUseItem, item);
    }

    public bool UseBox(ItemData item)
    {
        BoxItem box = item.Model_old as BoxItem;
        return LoseItem(item, 1, box.GetItems());
    }

    public bool UseEuipment(ItemData MItemInfo)
    {
        //Equip(MItemInfo);
        return false;
    }

    public bool UseBook(ItemData item)
    {
        BookItem book = item.Model_old as BookItem;
        switch (book.BookType)
        {
            case BookType.Building:
                if (CheckQuest(item, 1) && StructureManager.Instance.Learn(book.BuildingToLearn))
                    return LoseItem(item, 1);
                break;
            case BookType.Making:
                if (CheckQuest(item, 1) && MakingManager.Instance.Learn(book.ItemToLearn))
                    return LoseItem(item, 1);
                break;
            case BookType.Skill:
            default: break;
        }
        return false;
    }

    public bool UseBag(ItemData item)
    {
        BagItem bag = item.Model_old as BagItem;
        if (CheckQuest(item, 1))
        {
            if (ExpandSpace(bag.ExpandSize))
                return LoseItem(item, 1);
        }
        return false;
    }

    public bool UseQuest(ItemData item)
    {
        if (!CheckQuest(item, 1)) return false;
        QuestItem quest = item.Model_old as QuestItem;
        TriggerManager.Instance.SetTrigger(quest.TriggerName, quest.StateToSet);
        return LoseItem(item, 1);
    }
    #endregion
}