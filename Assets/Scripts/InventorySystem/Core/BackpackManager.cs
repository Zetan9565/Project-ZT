using UnityEngine;

namespace ZetanStudio.InventorySystem
{
    using ItemSystem;
    using ItemSystem.Module;
    using PlayerSystem;
    using QuestSystem;
    using SavingSystem;

    public class BackpackManager : InventoryHandler
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

        private static BackpackManager instance;
        public static BackpackManager Instance
        {
            get
            {
                if (!instance) instance = new BackpackManager();
                return instance;
            }
        }

        private BackpackManager()
        {
            _name = PlayerConfig.Instance.BackpackName;
            Inventory = new Inventory(defaultSpaceLimit, defaultWeightLimit, ignoreLock, customGetAction: TryGetCurrency, customLoseChecker: CheckQuest);
            ListenInventoryChange(true);
        }

        private bool CheckQuest(ItemData data, int amount)
        {
            if (QuestManager.HasQuestRequiredItem(data.Model, Inventory.GetAmount(data.ModelID) - amount))
            {
                MessageManager.Instance.New($"部分[{data.ColorName}]已被任务锁定");
                return false;
            }
            return true;
        }
        private bool TryGetCurrency(ItemData data, int amount)
        {
            if (data.Model.TryGetModule<CurrencyModule>(out var currency))
                switch (currency.Type.Name)
                {
                    case "金币":
                        GetMoney(currency.ValueEach * amount);
                        return true;
                    case "经验":
                        //TODO 获得经验
                        Debug.Log("获得经验");
                        return true;
                    case "技能经验":
                        break;
                    case "技能点":
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
            int finallyExpand = maxSpaceLimit > 0 ? Inventory.SpaceLimit + space > maxSpaceLimit ? maxSpaceLimit - Inventory.SpaceLimit : space : space;
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
            float finallyExpand = maxWeightLimit > 0 ? Inventory.WeightLimit + weightLoad > maxWeightLimit ? maxWeightLimit - Inventory.WeightLimit : weightLoad : weightLoad;
            Inventory.ExpandLoad(weightLoad);
            NotifyCenter.PostNotify(BackpackWeightChanged);
            MessageManager.Instance.New(Name + "载重增加了" + finallyExpand.ToString("F2"));
            return true;
        }

        [SaveMethod]
        public static void SaveData(SaveData saveData)
        {
            saveData["backpackData"] = Instance.Inventory.GenerateSaveData();
        }
        [LoadMethod]
        public static void LoadData(SaveData saveData)
        {
            if (saveData.TryReadData("backpackData", out var data))
                Instance.Inventory.LoadSaveData(data);
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
    }
}