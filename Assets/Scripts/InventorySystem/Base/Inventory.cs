using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using ZetanStudio.ItemSystem;
using ZetanStudio.UI;

namespace ZetanStudio.InventorySystem
{
    public class Inventory
    {
        private readonly List<ItemSlotData> slots = new List<ItemSlotData>();
        private readonly HashSet<ItemSlotData> slotsMap = new HashSet<ItemSlotData>();
        /// <summary>
        /// 使用<see cref="Item.ID"/>索引的道具槽
        /// </summary>
        private readonly Dictionary<string, List<ItemSlotData>> keyedSlots = new Dictionary<string, List<ItemSlotData>>();
        /// <summary>
        /// 使用<see cref="ItemData.ID"/>索引的道具数量信息
        /// </summary>
        private readonly Dictionary<string, CountedItem> items = new Dictionary<string, CountedItem>();
        /// <summary>
        /// 使用<see cref="ItemData.ID"/>索引的隐藏道具
        /// </summary>
        private readonly Dictionary<string, ItemData> hiddenItems = new Dictionary<string, ItemData>();

        public ReadOnlyCollection<ItemSlotData> Slots => slots.AsReadOnly();

        public ReadOnlyDictionary<string, CountedItem> Items => new ReadOnlyDictionary<string, CountedItem>(items);

        public ReadOnlyDictionary<string, ItemData> HiddenItems => new ReadOnlyDictionary<string, ItemData>(hiddenItems);

        public float WeightLimit { get; private set; }
        public int SpaceLimit { get; private set; }
        public float WeightCost { get; private set; }
        public int SpaceCost { get; private set; }
        public long Money { get; private set; }
        public int Space => SpaceLimit - SpaceCost;

        /// <summary>
        /// 金币更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的金币数量：<see cref="long"/>])
        /// </summary>
        public event InventoryMoneyListener OnInventoryMoneyChanged;
        /// <summary>
        /// 空间上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的空间上限：<see cref="int"/>])
        /// </summary>
        public event InventorySpaceListener OnInventorySpaceChanged;
        /// <summary>
        /// 负重上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的负重上限：<see cref="float"/>])
        /// </summary>
        public event InventoryWeightListener OnInventoryWeightChanged;
        /// <summary>
        /// 道具更新事件，格式：([发生变化的道具：<see cref="ItemData"/>], [旧的数量：<see cref="int"/>], [新的数量：<see cref="int"/>])
        /// </summary>
        public event ItemAmountListener OnItemAmountChanged;
        /// <summary>
        /// 道具槽更新事件，格式：([发生变化的道具槽：<see cref="ItemSlotData"/>])
        /// </summary>
        public event ItemSlotStateListener OnSlotStateChanged;

        /// <summary>
        /// 自定义道具获取操作，返回值表示操作成功，比<see cref="Inventory"/>的原生获取操作优先级高，不会触发<see cref="OnItemAmountChanged"/>事件
        /// </summary>
        private readonly Func<ItemData, int, bool> customGetAction;
        /// <summary>
        /// 自定义道具获取检查，返回值表示检查成功，比<see cref="Inventory"/>的原生检查优先级低
        /// </summary>
        private readonly Func<ItemData, int, bool> customGetChecker;
        /// <summary>
        /// 自定义道具填充数量检查，返回值表示最大可获取数量，比<see cref="Inventory"/>的原生检查优先级低
        /// </summary>
        private readonly Func<ItemData, int, int> customGetAmountChecker;
        /// <summary>
        /// 自定义道具失去操作，返回值表示操作成功，比<see cref="Inventory"/>的原生失去操作优先级高，不会触发<see cref="OnItemAmountChanged"/>事件
        /// </summary>
        private readonly Func<ItemData, int, bool> customLoseAction;
        /// <summary>
        /// 自定义道具失去检查，返回值表示检查成功，比<see cref="Inventory"/>的原生检查优先级低
        /// </summary>
        private readonly Func<ItemData, int, bool> customLoseChecker;

        private readonly bool ignoreSpace;
        private readonly bool ignoreWeight;
        private bool ignoreLock;

        /// <summary>
        /// <see cref="Inventory"/>的构造函数
        /// </summary>
        /// <param name="spaceLimit">空间上限，不填表示不设上限</param>
        /// <param name="weightLimit">负重上限，不填表示不设上限</param>
        /// <param name="ignoreLock">是否忽略锁定道具</param>
        /// <param name="customGetAction">自定义道具获取操作，返回值表示操作成功，比<see cref="Inventory"/>的原生获取操作优先级高，不会触发<see cref="OnItemAmountChanged"/>事件</param>
        /// <param name="customGetChecker">自定义道具获取检查，返回值表示检查成功，比<see cref="Inventory"/>的原生检查优先级低</param>
        /// <param name="customGetAmountChecker">自定义道具填充数量检查，返回值表示最大可获取数量，比<see cref="Inventory"/>的原生检查优先级低</param>
        /// <param name="customLoseAction">自定义道具失去操作，返回值表示操作成功，比<see cref="Inventory"/>的原生失去操作优先级高，不会触发<see cref="OnItemAmountChanged"/>事件</param>
        /// <param name="customLoseChecker">自定义道具失去检查，返回值表示检查成功，比<see cref="Inventory"/>的原生检查优先级低</param>
        public Inventory(int? spaceLimit = null,
                         float? weightLimit = null,
                         bool ignoreLock = false,
                         Func<ItemData, int, bool> customGetAction = null,
                         Func<ItemData, int, bool> customGetChecker = null,
                         Func<ItemData, int, int> customGetAmountChecker = null,
                         Func<ItemData, int, bool> customLoseAction = null,
                         Func<ItemData, int, bool> customLoseChecker = null)
        {
            SpaceLimit = spaceLimit ?? -1;
            WeightLimit = weightLimit ?? -1;
            this.ignoreLock = ignoreLock;
            this.customGetAction = customGetAction;
            this.customGetChecker = customGetChecker;
            this.customGetAmountChecker = customGetAmountChecker;
            this.customLoseAction = customLoseAction;
            this.customLoseChecker = customLoseChecker;
            while (slots.Count < spaceLimit)
            {
                MakeSlot();
            }
            ignoreSpace = SpaceLimit <= 0;
            ignoreWeight = WeightLimit <= 0;
        }

        #region 获取道具数据
        public bool TryGetData(string id, out ItemData result, out int amount)
        {
            result = null;
            amount = 0;
            var find = items.Values.FirstOrDefault(x => x.source.ModelID == id);
            if (find)
            {
                result = find.source;
                amount = find.amount;
            }
            return amount > 0;
        }
        public bool TryGetData(Item model, out ItemData result, out int amount)
        {
            result = null;
            amount = 0;
            if (!model) return false;
            return TryGetData(model.ID, out result, out amount);
        }
        public bool TryGetData(Predicate<ItemData> predicate, out ItemData result, out int amount)
        {
            result = null;
            amount = 0;
            if (predicate == null) return false;
            var find = items.Values.FirstOrDefault(x => predicate(x.source));
            if (find)
            {
                result = find.source;
                amount = find.amount;
            }
            return amount > 0;
        }
        public bool TryGetDatas(string id, out List<CountedItem> results)
        {
            results = items.Values.Where(x => x.source.ModelID == id).ToList();
            return results.Count > 0;
        }
        public bool TryGetDatas(Item model, out List<CountedItem> results)
        {
            results = items.Values.Where(x => x.source.Model == model).ToList();
            return results.Count > 0;
        }
        public bool TryGetDatas(Predicate<ItemData> predicate, out List<CountedItem> results)
        {
            results = items.Values.Where(x => predicate(x.source)).ToList();
            return results.Count > 0;
        }

        /// <summary>
        /// 按原型ID获取数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns>所有使用此原型的道具总数量</returns>
        public int GetAmount(string id)
        {
            var find = items.Values.Where(x => x.source.ModelID == id);
            if (find != null) return find.Sum(x => x.amount);
            else return 0;
        }
        /// <summary>
        /// 按原型获取数量
        /// </summary>
        /// <param name="model"></param>
        /// <returns>所有使用此原型的道具总数量</returns>
        public int GetAmount(Item model)
        {
            if (!model) return 0;
            var find = items.Values.Where(x => x.source.Model == model);
            if (find != null) return find.Sum(x => x.amount);
            else return 0;
        }
        /// <summary>
        /// 按实例获取数量
        /// </summary>
        /// <param name="item"></param>
        /// <returns>该实例在背包中的数量</returns>
        public int GetAmount(ItemData item)
        {
            if (!item || !item.IsInstance) return 0;
            if (items.TryGetValue(item.ID, out var find) && find != null)
                return find.amount;
            return 0;
        }
        public int GetAmount(Predicate<ItemData> predicate)
        {
            return items.Values.Where(x => predicate(x.source)).Sum(x => x.amount);
        }
        public bool ContainsItem(ItemData item)
        {
            return items.ContainsKey(item.ID) || hiddenItems.ContainsKey(item.ID);
        }
        #endregion

        #region 道具获取相关
        /// <summary>
        /// 窥探性获取而不真正获取
        /// </summary>
        /// <param name="model">道具原型</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        /// <returns>实际获取数量</returns>
        public int PeekGet(Item model, int amount, params CountedItem[] itemsToLose)
        {
            if (!model || amount <= 0) return 0;
            return PeekGet(ItemData.Empty(model), amount, itemsToLose);
        }
        public int PeekGet(ItemData item, int amount, params CountedItem[] itemsToLose)
        {
            if (!item || amount <= 0) return 0;
            int finalGet = amount;
            float vacatedWeight = 0;
            int vacatedSpace = 0;
            if (itemsToLose != null)
                foreach (var itl in itemsToLose)
                {
                    if (itl.IsValid)
                    {
                        if (!PeekLose(itl.source, itl.amount, out _)) return 0;
                        vacatedWeight += itl.source.Model.Weight * itl.amount;
                        if (itl.source.Model.InfiniteStack) vacatedSpace += itl.amount >= GetAmount(itl.source) ? 1 : 0;
                        else vacatedSpace += Mathf.FloorToInt(itl.amount * 1.0f / itl.source.Model.StackLimit);
                    }
                }
            if (!ignoreSpace)
            {
                if (keyedSlots.TryGetValue(item.ModelID, out var find))
                {
                    if (item.Model.InfiniteStack) finalGet = amount;
                    else
                    {
                        int left = 0;
                        foreach (var slot in find)
                        {
                            left += item.Model.StackLimit - slot.amount;
                        }
                        if (left < amount && Mathf.CeilToInt((amount - left) * 1.0f / item.Model.StackLimit) > Space + vacatedSpace)
                            finalGet = (Space + vacatedSpace) * item.Model.StackLimit + left;
                    }
                }
                else if (item.Model.InfiniteStack || Mathf.CeilToInt(amount * 1.0f / item.Model.StackLimit) > Space + vacatedSpace)
                    finalGet = (Space + vacatedSpace) * item.Model.StackLimit;
            }
            if (!ignoreWeight && finalGet * item.Model.Weight > WeightLimit - WeightCost + vacatedWeight)
                finalGet = Mathf.FloorToInt(WeightLimit - WeightCost + vacatedWeight / item.Model.Weight);
            return customGetAmountChecker != null ? customGetAmountChecker(item, finalGet) : finalGet;
        }
        /// <summary>
        /// 窥探性获取而不是真正获取
        /// </summary>
        /// <param name="model">道具原型</param>
        /// <param name="amount">数量</param>
        /// <param name="errorType">错误类型</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        /// <returns>是否可以获取</returns>
        public bool PeekGet(Item model, int amount, out InventoryError errorType, params CountedItem[] itemsToLose)
        {
            if (!model || amount <= 0)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            return PeekGet(ItemData.Empty(model), amount, out errorType, itemsToLose);
        }
        public bool PeekGet(ItemData item, int amount, out InventoryError errorType, params CountedItem[] itemsToLose)
        {
            if (!item || !item.Model || amount <= 0)
            {
                errorType = InventoryError.Invalid;
                return false;
            }

            errorType = InventoryError.None;
            float vacatedWeight = 0;
            int vacatedSpace = 0;
            if (itemsToLose != null)
                foreach (var itl in itemsToLose)
                {
                    if (itl.IsValid)
                    {
                        if (!PeekLose(itl.source, itl.amount, out errorType))
                        {
                            switch (errorType)
                            {
                                case InventoryError.Lack:
                                    errorType = InventoryError.PartialLack;
                                    break;
                                case InventoryError.Locked:
                                    errorType = InventoryError.PartialLocked;
                                    break;
                            }
                            return false;
                        }
                        vacatedWeight += itl.source.Model.Weight * itl.amount;
                        if (itl.source.Model.InfiniteStack) vacatedSpace += itl.amount >= GetAmount(itl.source) ? 1 : 0;
                        else vacatedSpace += Mathf.FloorToInt(itl.amount * 1.0f / itl.source.Model.StackLimit);
                    }
                }
            if (!ignoreWeight && amount * item.Model.Weight > WeightLimit - WeightCost + vacatedWeight)
            {
                errorType = InventoryError.Overload;
                return false;
            }
            else if (!ignoreSpace)
                if (keyedSlots.TryGetValue(item.ModelID, out var find))
                {
                    if (item.Model.InfiniteStack) return true;
                    int left = 0;
                    foreach (var slot in find)
                    {
                        left += item.Model.StackLimit - slot.amount;
                    }
                    if (left < amount && Mathf.CeilToInt((amount - left) * 1.0f / item.Model.StackLimit) > Space + vacatedSpace)
                    {
                        errorType = InventoryError.OverSpace;
                        return false;
                    }
                }
                else if (item.Model.InfiniteStack && Space + vacatedSpace < 1 || !item.Model.InfiniteStack && Mathf.CeilToInt(amount * 1.0f / item.Model.StackLimit) > Space + vacatedSpace)
                {
                    errorType = InventoryError.OverSpace;
                    return false;
                }
            if (customGetChecker != null && !customGetChecker(item, amount))
            {
                errorType = InventoryError.Custom;
                return false;
            }
            return true;
        }
        public bool PeekGet(IEnumerable<IItemInfo> infos, out InventoryError errorType, params CountedItem[] itemsToLose)
        {
            if (infos == null)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            return PeekGet(CountedItem.Convert(infos), out errorType, itemsToLose);
        }
        public bool PeekGet(IEnumerable<CountedItem> items, out InventoryError errorType, params CountedItem[] itemsToLose)
        {
            if (items == null)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            float vacatedWeight = 0;
            int vacatedSpace = 0;
            if (itemsToLose != null)
                foreach (var itl in itemsToLose)
                {
                    if (itl.IsValid)
                    {
                        if (!PeekLose(itl.source, itl.amount, out errorType))
                        {
                            switch (errorType)
                            {
                                case InventoryError.Lack:
                                    errorType = InventoryError.PartialLack;
                                    break;
                                case InventoryError.Locked:
                                    errorType = InventoryError.PartialLocked;
                                    break;
                            }
                            return false;
                        }
                        vacatedWeight += itl.source.Model.Weight * itl.amount;
                        if (itl.source.Model.InfiniteStack) vacatedSpace += itl.amount >= GetAmount(itl.source) ? 1 : 0;
                        else vacatedSpace += Mathf.FloorToInt(itl.amount * 1.0f / itl.source.Model.StackLimit);
                    }
                }
            foreach (var item in items)
            {
                Item model = item.source.Model;
                int amount = item.amount;
                float needWeight = amount * model.Weight;
                if (!ignoreWeight)
                {
                    if (needWeight > WeightLimit - WeightCost + vacatedWeight)
                    {
                        errorType = InventoryError.Overload;
                        return false;
                    }
                    vacatedWeight -= model.Weight * amount;
                }
                if (!ignoreSpace)
                {
                    int needSpace = model.InfiniteStack ? 1 : Mathf.CeilToInt(amount * 1.0f / model.StackLimit);
                    if (keyedSlots.TryGetValue(model.ID, out var find))
                    {
                        if (model.InfiniteStack) needSpace = 0;
                        else
                        {
                            int left = 0;
                            foreach (var slot in find)
                            {
                                left += model.StackLimit - slot.amount;
                            }
                            needSpace = Mathf.CeilToInt((amount - left) * 1.0f / model.StackLimit);
                            if (left < amount && needSpace > Space + vacatedSpace)
                            {
                                errorType = InventoryError.OverSpace;
                                return false;
                            }
                        }
                    }
                    else if (needSpace > Space + vacatedSpace)
                    {
                        errorType = InventoryError.OverSpace;
                        return false;
                    }
                    vacatedSpace -= needSpace;
                }
                if (customGetChecker != null && !customGetChecker(item.source, item.amount))
                {
                    errorType = InventoryError.Custom;
                    return false;
                }
            }
            errorType = InventoryError.None;
            return true;
        }

        /// <summary>
        /// 按原型放入道具
        /// </summary>
        /// <param name="model">道具原型</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        public void Get(Item model, int amount, params CountedItem[] itemsToLose)
        {
            if (customGetAction != null && customGetAction(ItemData.Empty(model), amount)) return;

            if (itemsToLose != null && itemsToLose.Length > 0) Lose(itemsToLose);
            int oldAmount;
            if (model.StackAble) oldAmount = PutStackableItem(ItemFactory.MakeItem(model), amount);
            else
            {
                oldAmount = PutUnstackableItem(ItemFactory.MakeItem(model));
                for (int i = 0; i < amount - 1; i++)
                {
                    PutUnstackableItem(ItemFactory.MakeItem(model));
                }
            }
            OnItemAmountChanged?.Invoke(model, oldAmount, oldAmount + amount);
        }
        /// <summary>
        /// 按个例放入道具
        /// </summary>
        /// <param name="item">道具个例</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        public void Get(ItemData item, int amount, params CountedItem[] itemsToLose)
        {
            if (customGetAction != null && customGetAction(item, amount)) return;

            if (!item.IsInstance) Get(item.Model, amount, itemsToLose);
            else if (item.Model.StackAble)
            {
                if (itemsToLose != null && itemsToLose.Length > 0) Lose(itemsToLose);
                int oldAmount = PutStackableItem(item, amount);
                OnItemAmountChanged?.Invoke(item.Model, oldAmount, oldAmount + amount);
            }
            else
            {
                if (itemsToLose != null && itemsToLose.Length > 0) Lose(itemsToLose);
                int oldAmount = PutUnstackableItem(item);
                OnItemAmountChanged?.Invoke(item.Model, oldAmount, oldAmount + 1);
            }
        }
        /// <summary>
        /// 按列表放入道具
        /// </summary>
        /// <param name="infos">道具列表</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        public void Get(IEnumerable<IItemInfo> infos, params CountedItem[] itemsToLose)
        {
            if (infos == null) return;
            if (itemsToLose != null && itemsToLose.Length > 0) Lose(itemsToLose);
            foreach (var info in infos)
            {
                if (info.IsValid) Get(info.Item, info.Amount);
            }
        }
        /// <summary>
        /// 按个例列表放入道具
        /// </summary>
        /// <param name="items">道具列表</param>
        /// <param name="itemsToLose">同时失去的道具</param>
        public void Get(IEnumerable<CountedItem> items, params CountedItem[] itemsToLose)
        {
            if (items == null) return;
            if (itemsToLose != null && itemsToLose.Length > 0) Lose(itemsToLose);
            foreach (var item in items)
            {
                if (item.IsValid) Get(item.source, item.amount);
            }
        }
        #endregion

        #region 道具失去相关
        public bool PeekLose(string id, int amount, out InventoryError errorType, params CountedItem[] itemsToGet)
        {
            if (string.IsNullOrEmpty(id) && amount < 0)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            if (TryGetData(id, out var data, out var have))
            {
                if (amount == 0)
                {
                    if (have > 0)
                    {
                        errorType = InventoryError.None;
                        return true;
                    }
                    else
                    {
                        errorType = InventoryError.Lack;
                        return false;
                    }
                }
                else if (!ignoreLock && data.IsLocked)
                {
                    errorType = InventoryError.Locked;
                    return false;
                }
                if (itemsToGet != null)
                {
                    SimulateLose(data.Model, amount);
                    if (!PeekGet(itemsToGet, out errorType))
                    {
                        UnsimlulateLose(data.Model, amount);
                        return false;
                    }
                    UnsimlulateLose(data.Model, amount);
                }
                errorType = have >= amount ? InventoryError.None : InventoryError.Lack;
                if (have < amount) return false;
                else if (customLoseChecker != null && !customLoseChecker(data, amount))
                {
                    errorType = InventoryError.Custom;
                    return false;
                }
                else return true;
            }
            else
            {
                Debug.Log(id);
                errorType = InventoryError.Lack;
                return false;
            }
        }
        public bool PeekLose(Item model, int amount, out InventoryError errorType, params CountedItem[] itemsToGet)
        {
            if (!model)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            return PeekLose(model.ID, amount, out errorType, itemsToGet);
        }
        public bool PeekLose(ItemData item, int amount, out InventoryError errorType, params CountedItem[] itemsToGet)
        {
            if (item && amount > -1)
                if (amount == 0)
                {
                    if (ContainsItem(item))
                    {
                        errorType = InventoryError.None;
                        return true;
                    }
                    else
                    {
                        errorType = InventoryError.Lack;
                        return false;
                    }
                }
                else if (!ignoreLock && item.IsLocked)
                {
                    errorType = InventoryError.Locked;
                    return false;
                }
                else if (!item.IsInstance) return PeekLose(item.Model, amount, out errorType, itemsToGet);
                else if (ContainsItem(item))
                {
                    if (item.Model.StackAble) return PeekLose(item.ModelID, amount, out errorType, itemsToGet);
                    errorType = InventoryError.None;
                    return true;
                }
                else
                {
                    errorType = InventoryError.Lack;
                    return false;
                }
            errorType = InventoryError.Invalid;
            return false;
        }
        public bool PeekLose(IEnumerable<CountedItem> items, out InventoryError errorType, params CountedItem[] itemsToGet)
        {
            if (items == null)
            {
                errorType = InventoryError.Invalid;
                return false;
            }
            if (itemsToGet != null)
            {
                foreach (var item in items)
                {
                    SimulateLose(item.source.Model, item.amount);
                }
                if (!PeekGet(itemsToGet, out errorType))
                {
                    foreach (var item in items)
                    {
                        UnsimlulateLose(item.source.Model, item.amount);
                    }
                    if (errorType == InventoryError.Overload)
                        errorType = InventoryError.PartialOverload;
                    return false;
                }
                foreach (var item in items)
                {
                    UnsimlulateLose(item.source.Model, item.amount);
                }
            }
            foreach (var item in items)
            {
                if (item.IsValid && !PeekLose(item.source, item.amount, out errorType))
                {
                    switch (errorType)
                    {
                        case InventoryError.Lack:
                            errorType = InventoryError.PartialLack;
                            break;
                        case InventoryError.Locked:
                            errorType = InventoryError.PartialLocked;
                            break;
                    }
                    return false;
                }
                if (customLoseChecker != null && !customLoseChecker(item.source, item.amount))
                {
                    errorType = InventoryError.Custom;
                    return false;
                }
            }
            errorType = InventoryError.None;
            return true;
        }

        /// <summary>
        /// 失去道具，可选同时获得道具，无视限制
        /// </summary>
        /// <param name="id">原型ID</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToGet">同时获得的道具</param>
        public void Lose(string id, int amount, params CountedItem[] itemsToGet)
        {
            if (string.IsNullOrEmpty(id) || amount < 1) return;
            if (keyedSlots.TryGetValue(id, out var find) && find.Count > 0)
            {
                ItemData data = find[0].item;
                if (data)
                {
                    if (customLoseAction != null && customLoseAction(data, amount)) return;

                    int left = amount, oldAmount = 0, oldCount = find.Count;
                    for (int i = find.Count - 1; i >= 0; i--)
                    {
                        var slot = find[i];
                        oldAmount += slot.amount;
                        var item = slot.item;
                        left -= slot.Take(left);
                        if (slot.IsEmpty)
                        {
                            find.Remove(slot);
                            if (!data.StackAble) items.Remove(item.ID);
                        }
                    }
                    SpaceCost -= oldCount - find.Count;
                    WeightCost -= data.Model.Weight * amount;
                    if (data.StackAble)
                        if (find.Count < 0) items.Remove(data.ID);
                        else items[data.ID].amount -= amount;
                    OnItemAmountChanged?.Invoke(data.Model, oldAmount, oldAmount - amount);
                    if (itemsToGet != null && itemsToGet.Length > 0) Get(itemsToGet);
                }
            }
        }
        /// <summary>
        /// 失去道具，可选同时获得道具，无视限制
        /// </summary>
        /// <param name="model">原型</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToGet">同时获得的道具</param>
        public void Lose(Item model, int amount, params CountedItem[] itemsToGet)
        {
            if (model) Lose(model.ID, amount, itemsToGet);
        }
        /// <summary>
        /// 失去道具，可选同时获得道具，无视限制
        /// </summary>
        /// <param name="item">道具个例</param>
        /// <param name="amount">数量</param>
        /// <param name="itemsToGet">同时获得的道具</param>
        public void Lose(ItemData item, int amount, params CountedItem[] itemsToGet)
        {
            if (customLoseAction != null && customLoseAction(item, amount)) return;

            if (!item || amount < 1) return;
            if (item.Model.StackAble || !item.IsInstance) Lose(item.ModelID, amount, itemsToGet);
            else if (!item.Model.StackAble && keyedSlots.TryGetValue(item.ModelID, out var find) && amount == 1)
            {
                int oldAmount = find.Count;
                for (int i = 0; i < find.Count; i++)
                {
                    var slot = find[i];
                    if (slot.item == item)
                    {
                        slot.TakeAll();
                        find.RemoveAt(i);
                        SpaceCost--;
                        WeightCost -= item.Model.Weight;
                        i--;
                    }
                }
                if (itemsToGet != null && itemsToGet.Length > 0) Get(itemsToGet);
                items.Remove(item.ID);
                OnItemAmountChanged?.Invoke(item.Model, oldAmount, oldAmount - amount);
            }
        }
        /// <summary>
        /// 失去多个道具，可选同时获得道具，无视限制
        /// </summary>
        /// <param name="items">道具与数量列表</param>
        /// <param name="itemsToGet">同时获得的道具</param>
        public void Lose(IEnumerable<CountedItem> items, params CountedItem[] itemsToGet)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (item.IsValid) Lose(item.source, item.amount);
            }
            if (itemsToGet != null && itemsToGet.Length > 0) Get(itemsToGet);
        }
        #endregion

        #region 其它
        public bool CanUnhide()
        {
            return ignoreSpace || SpaceCost + 1 <= SpaceLimit;
        }
        public bool HideItem(ItemData item)
        {
            if (!item || item.Model.StackAble) return false;
            foreach (var slot in slots)
            {
                if (slot.item == item)
                {
                    slot.Vacate();
                    SpaceCost--;
                }
            }
            if (keyedSlots.TryGetValue(item.ID, out var find)) find.Clear();
            if (items.TryGetValue(item.ID, out var iwa))
            {
                items.Remove(item.ID);
                hiddenItems.Add(item.ID, item);
                OnItemAmountChanged(item.Model, iwa.amount, 0);
                return true;
            }
            else return false;
        }
        public bool UnhideItem(ItemData item)
        {
            return UnhideItem(item, null);
        }
        private bool UnhideItem(ItemData item, ItemSlotData slot)
        {
            if (!item || item.Model.StackAble) return false;
            if (hiddenItems.TryGetValue(item.ID, out var find) && find == item)
            {
                hiddenItems.Remove(item.ID);
                if (!ignoreSpace && SpaceCost + 1 > SpaceLimit) return false;
                if (slot) PutIntoSlot(item, 1, slot);
                else PutIntoEmptySlot(item, 1);
                OnItemAmountChanged(item.Model, 0, 1);
                return true;
            }
            else return false;
        }
        public bool SwapHiddenItem(ItemData unhide, ItemData hide)
        {
            if (!hide || hide.Model.StackAble) return false;
            ItemSlotData slot = slots.Find(x => x.item == hide);
            return HideItem(hide) && (!unhide || UnhideItem(unhide, slot));
        }

        private void SimulateLose(Item model, int amount)
        {
            if (!ignoreWeight) WeightCost -= model.Weight * amount;
            if (!ignoreSpace)
            {
                if (model.InfiniteStack)
                {
                    int have = GetAmount(model);
                    SpaceCost -= have > amount ? 0 : 1;
                }
                else SpaceCost -= Mathf.FloorToInt(amount * 1.0f / model.StackLimit);
            }
        }
        private void UnsimlulateLose(Item model, int amount)
        {
            if (!ignoreWeight) WeightCost += model.Weight * amount;
            if (!ignoreSpace)
            {
                if (model.InfiniteStack)
                {
                    int have = GetAmount(model);
                    SpaceCost += have > amount ? 0 : 1;
                }
                else SpaceCost += Mathf.FloorToInt(amount * 1.0f / model.StackLimit);
            }
        }
        private void OnSlotSwap(ItemSlotData origin, ItemSlotData target)
        {
            if (!origin || !target || !ContainsSlot(target)) return;
            if (origin.ModelID == target.ModelID) return;
            if (keyedSlots.TryGetValue(target.ModelID, out var find))//因为已经换过了，所以ID反着用
            {
                find.Remove(origin);
                if (!target.IsEmpty) find.Add(target);
            }
            if (keyedSlots.TryGetValue(origin.ModelID, out find))
            {
                find.Remove(target);
                if (!origin.IsEmpty) find.Add(origin);
            }
        }
        public bool PeekTransferItem(Inventory other, ItemData item, int amount, out InventoryError error)
        {
            if (!item || amount < 1 || other == null)
            {
                error = InventoryError.Invalid;
                return false;
            }
            var ilBf = ignoreLock;
            ignoreLock = true;
            if (!PeekLose(item, amount, out error))
            {
                ignoreLock = ilBf;
                return false;
            }
            if (!other.PeekGet(item, amount, out error))
            {
                ignoreLock = ilBf;
                return false;
            }
            return true;
        }
        public void TransferItem(Inventory other, ItemData item, int amount)
        {
            Lose(item, amount);
            other.Get(item, amount);
        }
        public void Arrange()
        {
            slots.Sort(ItemSlotData.Comparer.Default);
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].index = i;
            }
        }
        private int PutUnstackableItem(ItemData item)
        {
            int oldAmount = 0;
            WeightCost += item.Weight;
            if (keyedSlots.TryGetValue(item.ModelID, out var find))
            {
                oldAmount = find.Count;
                find.Add(PutIntoEmptySlot(item, 1));
            }
            else keyedSlots.Add(item.ModelID, new List<ItemSlotData>() { PutIntoEmptySlot(item, 1) });
            return oldAmount;
        }
        private int PutStackableItem(ItemData item, int amount)
        {
            WeightCost += item.Weight * amount;
            ItemData exist = null;
            int oldAmount = 0;
            if (keyedSlots.TryGetValue(item.ModelID, out var find))
            {
                CountedItem iwa = null;
                foreach (var slot in find)
                {
                    oldAmount += slot.amount;
                    if (!exist)
                    {
                        exist = slot.item;
                        if (exist) items.TryGetValue(exist.ID, out iwa);
                    }
                    int take = slot.Put(amount);
                    if (iwa) iwa.amount += take;
                    amount -= take;
                }
            }
            else
            {
                find = new List<ItemSlotData>();
                keyedSlots.Add(item.ModelID, find);
            }
            //尝试处理剩下的数量
            costEmptySlot(find);
            return oldAmount;

            void costEmptySlot(List<ItemSlotData> collection)
            {
                if (amount <= 0 || collection == null) return;
                if (exist == null) exist = item;//如果物品可叠加但没有在背包中找到，则这个是新物品
                ItemSlotData temp;
                if (item.InfiniteStack)
                {
                    temp = PutIntoEmptySlot(exist, amount);
                    if (temp) collection.Add(temp);
                }
                else
                {
                    //尝试以叠加上限依次填充空槽，直到下次无法填满
                    for (int i = 0; i < amount / item.StackLimit; i++)
                    {
                        temp = PutIntoEmptySlot(exist, item.StackLimit);
                        if (temp) collection.Add(temp);
                    }
                    //尝试处理剩余的部分
                    temp = PutIntoEmptySlot(exist, amount % item.StackLimit);
                    if (temp) collection.Add(temp);
                }
            }
        }
        private ItemSlotData PutIntoEmptySlot(ItemData item, int amount)
        {
            if (!item || amount <= 0) return null;
            return PutIntoSlot(item, amount, slots.Find(x => x.IsEmpty));
        }
        private ItemSlotData PutIntoSlot(ItemData item, int amount, ItemSlotData slot)
        {
            if (!item || amount <= 0) return null;
            if (!slot)
                if (ignoreSpace) slot = MakeSlot();
                else throw new InvalidOperationException("往已满的库存中增加新道具");
            slot.Put(item, amount);
            SpaceCost++;
            if (items.TryGetValue(item.ID, out var find)) find.amount += amount;
            else items.Add(item.ID, new CountedItem(item, amount));
            return slot;
        }
        private ItemSlotData MakeSlot()
        {
            var slot = new ItemSlotData(slots.Count);
            slot.OnSlotStateChanged += (s) => OnSlotStateChanged?.Invoke(s);
            slot.OnSlotSwap += OnSlotSwap;
            slots.Add(slot);
            slotsMap.Add(slot);
            return slot;
        }
        public bool ContainsSlot(ItemSlotData slot)
        {
            return slotsMap.Contains(slot);
        }
        public void GetMoney(long amount)
        {
            if (amount < 0) return;
            else
            {
                var old = Money;
                Money += amount;
                OnInventoryMoneyChanged(this, old);
            }
        }
        public void LoseMoney(long amount)
        {
            if (amount > Money) return;
            else
            {
                var old = Money;
                Money -= amount;
                OnInventoryMoneyChanged(this, old);
            }
        }
        public void ExpandSpace(int size)
        {
            int oldLimit = SpaceLimit;
            SpaceLimit += size;
            while (slots.Count < SpaceLimit)
            {
                MakeSlot();
            }
            OnInventorySpaceChanged?.Invoke(this, oldLimit);
        }
        public void ExpandLoad(float weight)
        {
            float oldLimit = WeightLimit;
            WeightLimit += weight;
            OnInventoryWeightChanged?.Invoke(this, oldLimit);
        }
        #endregion

        public static implicit operator bool(Inventory self)
        {
            return self != null;
        }

        public GenericData GetSaveData()
        {
            var save = new GenericData();
            save["money"] = Money.ToString();
            save["spaceCost"] = SpaceCost;
            save["spaceLimit"] = SpaceLimit;
            save["weightCost"] = WeightCost;
            save["weightLimit"] = WeightLimit;
            var items = new GenericData();
            save["items"] = items;
            var slots = new GenericData();
            save["slots"] = slots;
            foreach (var slot in this.slots)
            {
                var ss = new GenericData();
                ss["index"] = slot.index;
                ss["amount"] = slot.amount;
                ss["item"] = slot.ItemID;
                slots.Write(ss);
            }
            foreach (var item in this.items.Values)
            {
                items[item.source.ID] = item.amount;
            }
            var hidden = new GenericData();
            save["hiddenItems"] = hidden;
            foreach (var item in hiddenItems.Values)
            {
                hidden.Write(item.ID);
            }
            return save;
        }
        public void LoadSaveData(GenericData save)
        {
            if (save.TryReadString("money", out var money) && long.TryParse(money, out var mv)) Money = mv;
            if (save.TryReadInt("spaceCost", out var space)) SpaceCost = space;
            if (save.TryReadInt("spaceLimit", out var spaceLmt)) SpaceLimit = spaceLmt;
            if (save.TryReadFloat("weightCost", out var weight)) WeightCost = weight;
            if (save.TryReadFloat("weightLimit", out var weightLmt)) WeightLimit = weightLmt;
            this.items.Clear();
            if (save.TryReadData("items", out var items))
            {
                foreach (var amount in items.ReadIntDict())
                {
                    if (ItemFactory.GetItem(amount.Key) is ItemData loadedItem)
                        this.items[amount.Key] = new CountedItem(loadedItem, amount.Value);
                }
            }
            this.slots.Clear();
            slotsMap.Clear();
            keyedSlots.Clear();

            if (save.TryReadData("slots", out var slots))
            {
                foreach (var sd in slots.ReadDataList())
                {
                    var slot = new ItemSlotData(this.slots.Count);
                    if (sd.TryReadInt("index", out var index)) slot.index = index;
                    if (sd.TryReadInt("amount", out var amount)) slot.amount = amount;
                    if (sd.TryReadString("item", out var itemID) && this.items.TryGetValue(itemID, out var item))
                    {
                        slot.item = item.source;
                        if (keyedSlots.TryGetValue(slot.ModelID, out var list)) list.Add(slot);
                        else keyedSlots[slot.ModelID] = new List<ItemSlotData>() { slot };
                    }
                    this.slots.Add(slot);
                    slotsMap.Add(slot);
                }
            }
            hiddenItems.Clear();
            if (save.TryReadData("hiddenItems", out var hiddens))
            {
                foreach (var hidden in hiddens.ReadStringList())
                {
                    if (this.items.TryGetValue(hidden, out var item))
                        hiddenItems[hidden] = item.source;
                }
            }
        }

        #region 委托
        public delegate void InventoryMoneyListener(Inventory inventory, long oldMoney);
        public delegate void InventorySpaceListener(Inventory inventory, int oldSpaceLimit);
        public delegate void InventoryWeightListener(Inventory inventory, float oldWeightLimit);
        public delegate void ItemAmountListener(Item item, int oldAmount, int newAmount);
        public delegate void ItemSlotStateListener(ItemSlotData slot);
        #endregion
    }

    public enum InventoryError
    {
        None,
        Invalid,
        Overload,
        OverSpace,
        Lack,
        Locked,
        PartialOverload,
        PartialLack,
        PartialLocked,
        Custom,
    }

    #region 各种工具类
    public static class InventoryUtility
    {
        /// <summary>
        /// 丢弃道具
        /// </summary>
        /// <param name="data">道具信息</param>
        public static void OpenDiscardItemPanel(InventoryHandler handler, ItemData data, Vector3? amountPosition = null)
        {
            if (data == null || !data.Model) return;
            if (!handler.ContainsItem(data))
            {
                MessageManager.Instance.New($"该物品已不在{handler.Name}中");
                return;
            }
            if (!data.Model.Discardable)
            {
                MessageManager.Instance.New("该物品不可丢弃");
                return;
            }
            int amount = handler.GetAmount(data);
            if (amount < 2 && amount > 0)
            {
                ConfirmWindow.StartConfirm($"确定丢弃1个 [{data.ColorName}] 吗？",
                    delegate
                    {
                        if (handler.Lose(data, 1))
                            MessageManager.Instance.New($"丢掉了1个 [{data.Name}]");
                    });
            }
            else AmountWindow.StartInput(delegate (long amount)
            {
                ConfirmWindow.StartConfirm($"确定丢弃{amount}个 [{data.ColorName}] 吗？",
                    delegate
                    {
                        if (handler.Lose(data, (int)amount))
                            MessageManager.Instance.New($"丢掉了{amount}个 [{data.Name}]");
                    });
            }, amount, "丢弃数量", amountPosition);
        }
    }
    #endregion
}