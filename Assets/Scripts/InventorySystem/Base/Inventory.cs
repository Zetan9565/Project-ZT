using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Inventory
{
    private readonly List<ItemSlotData> slots = new List<ItemSlotData>();
    private ReadOnlyCollection<ItemSlotData> readOnlySlots;
    private readonly HashSet<ItemSlotData> slotsMap = new HashSet<ItemSlotData>();
    private readonly Dictionary<string, List<ItemSlotData>> keyedSlots = new Dictionary<string, List<ItemSlotData>>();
    private readonly Dictionary<string, ItemWithAmount> items = new Dictionary<string, ItemWithAmount>();
    private ReadOnlyDictionary<string, ItemWithAmount> readOnlyItems;
    private readonly Dictionary<string, ItemData> hiddenItems = new Dictionary<string, ItemData>();
    private ReadOnlyDictionary<string, ItemData> readOnlyHiddenItems;

    public ReadOnlyCollection<ItemSlotData> Slots
    {
        get
        {
            if (readOnlySlots == null) readOnlySlots = slots.AsReadOnly();
            return readOnlySlots;
        }
    }
    public ReadOnlyDictionary<string, ItemWithAmount> Items
    {
        get
        {
            if (readOnlyItems == null) readOnlyItems = new ReadOnlyDictionary<string, ItemWithAmount>(items);
            return readOnlyItems;
        }
    }
    public ReadOnlyDictionary<string, ItemData> HiddenItems
    {
        get
        {
            if (readOnlyHiddenItems == null) readOnlyHiddenItems = new ReadOnlyDictionary<string, ItemData>(hiddenItems);
            return readOnlyHiddenItems;
        }
    }

    public float WeightLimit { get; private set; }
    public int SpaceLimit { get; private set; }
    public float WeightCost { get; private set; }
    public int SpaceCost { get; private set; }
    public long Money { get; private set; }

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
    public bool TryGetData(ItemBase model, out ItemData result, out int amount)
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
    public bool TryGetDatas(string id, out List<ItemWithAmount> results)
    {
        results = items.Values.Where(x => x.source.ModelID == id).ToList();
        return results.Count > 0;
    }
    public bool TryGetDatas(ItemBase model, out List<ItemWithAmount> results)
    {
        results = items.Values.Where(x => x.source.Model_old == model).ToList();
        return results.Count > 0;
    }
    public bool TryGetDatas(Predicate<ItemData> predicate, out List<ItemWithAmount> results)
    {
        results = items.Values.Where(x => predicate(x.source)).ToList();
        return results.Count > 0;
    }

    public int GetAmount(string id)
    {
        var find = items.Values.Where(x => x.source.ModelID == id);
        if (find != null) return find.Sum(x => x.amount);
        else return 0;
    }
    public int GetAmount(ItemBase model)
    {
        if (!model) return 0;
        var find = items.Values.Where(x => x.source.Model_old == model);
        if (find != null) return find.Sum(x => x.amount);
        else return 0;
    }
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
    #endregion

    #region 道具获取相关
    /// <summary>
    /// 窥探性获取而不真正获取
    /// </summary>
    /// <param name="model">道具原型</param>
    /// <param name="amount">数量</param>
    /// <param name="simulLoseItems">同时失去的道具</param>
    /// <returns>实际获取数量</returns>
    public int PeekGet(ItemBase model, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!model || amount <= 0) return 0;
        return PeekGet(model.CreateData(false), amount, simulLoseItems);
    }
    public int PeekGet(ItemData item, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!item || amount <= 0) return 0;
        int finalGet = amount;
        float vacatedWeight = 0;
        int vacatedSpace = 0;
        if (simulLoseItems != null)
            foreach (var sli in simulLoseItems)
            {
                if (sli.IsValid)
                {
                    if (!PeekLose(sli.source, sli.amount, out _))
                        return 0;
                    vacatedWeight += sli.source.Model_old.Weight * sli.amount;
                    vacatedSpace += Mathf.FloorToInt(sli.amount * 1.0f / sli.source.Model_old.StackNum);
                }
            }
        if (!ignoreSpace)
        {
            if (keyedSlots.TryGetValue(item.ModelID, out var find))
            {
                int left = 0;
                foreach (var slot in find)
                {
                    left += item.Model_old.StackNum - slot.amount;
                }
                if (left < amount && Mathf.CeilToInt((amount - left) * 1.0f / item.Model_old.StackNum) > SpaceLimit - SpaceCost + vacatedSpace)
                    finalGet = (SpaceLimit - SpaceCost + vacatedSpace) * item.Model_old.StackNum + left;
            }
            else if (Mathf.CeilToInt(amount * 1.0f / item.Model_old.StackNum) > SpaceLimit - SpaceCost + vacatedSpace)
                finalGet = (SpaceLimit - SpaceCost + vacatedSpace) * item.Model_old.StackNum;
        }
        if (!ignoreWeight && finalGet * item.Model_old.Weight > WeightLimit - WeightCost + vacatedWeight)
            finalGet = Mathf.FloorToInt(WeightLimit - WeightCost + vacatedWeight / item.Model_old.Weight);
        return customGetAmountChecker != null ? customGetAmountChecker(item, finalGet) : finalGet;
    }
    /// <summary>
    /// 窥探性获取而不是真正获取
    /// </summary>
    /// <param name="model">道具原型</param>
    /// <param name="amount">数量</param>
    /// <param name="errorType">错误类型</param>
    /// <param name="simulLoseItems">同时失去的道具</param>
    /// <returns>是否可以获取</returns>
    public bool PeekGet(ItemBase model, int amount, out InventoryError errorType, params ItemWithAmount[] simulLoseItems)
    {
        if (!model || amount <= 0)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        return PeekGet(model.CreateData(false), amount, out errorType, simulLoseItems);
    }
    public bool PeekGet(ItemData item, int amount, out InventoryError errorType, params ItemWithAmount[] simulLoseItems)
    {
        if (!item || !item.Model_old || amount <= 0)
        {
            errorType = InventoryError.Invalid;
            return false;
        }

        float vacatedWeight = 0;
        int vacatedSpace = 0;
        if (simulLoseItems != null)
            foreach (var sli in simulLoseItems)
            {
                if (sli.IsValid)
                {
                    if (!PeekLose(sli.source, sli.amount, out errorType))
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
                    vacatedWeight += sli.source.Model_old.Weight * sli.amount;
                    vacatedSpace += Mathf.FloorToInt(sli.amount * 1.0f / sli.source.Model_old.StackNum);
                }
            }
        if (!ignoreWeight && amount * item.Model_old.Weight > WeightLimit - WeightCost + vacatedWeight)
        {
            errorType = InventoryError.Overload;
            return false;
        }
        else if (!ignoreSpace)
            if (keyedSlots.TryGetValue(item.ModelID, out var find))
            {
                int left = 0;
                foreach (var slot in find)
                {
                    left += item.Model_old.StackNum - slot.amount;
                }
                if (left < amount && Mathf.CeilToInt((amount - left) * 1.0f / item.Model_old.StackNum) > SpaceLimit - SpaceCost + vacatedSpace)
                {
                    errorType = InventoryError.OverSpace;
                    return false;
                }
            }
            else if (Mathf.CeilToInt(amount * 1.0f / item.Model_old.StackNum) > SpaceLimit - SpaceCost + vacatedSpace)
            {
                errorType = InventoryError.OverSpace;
                return false;
            }
        if (customGetChecker != null && !customGetChecker(item, amount))
        {
            errorType = InventoryError.Custom;
            return false;
        }
        errorType = InventoryError.None;
        return true;
    }
    public bool PeekGet(IEnumerable<ItemInfoBase> infos, out InventoryError errorType, params ItemWithAmount[] simulLoseItems)
    {
        if (infos == null)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        return PeekGet(ItemWithAmount.Convert(infos), out errorType, simulLoseItems);
    }
    public bool PeekGet(IEnumerable<ItemWithAmount> items, out InventoryError errorType, params ItemWithAmount[] simulLoseItems)
    {
        if (items == null)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        float vacatedWeight = 0;
        int vacatedSpace = 0;
        if (simulLoseItems != null)
            foreach (var sli in simulLoseItems)
            {
                if (sli.IsValid)
                {
                    if (!PeekLose(sli.source, sli.amount, out errorType))
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
                    vacatedWeight += sli.source.Model_old.Weight * sli.amount;
                    vacatedSpace += Mathf.FloorToInt(sli.amount * 1.0f / sli.source.Model_old.StackNum);
                }
            }
        foreach (var item in items)
        {
            ItemBase model = item.source.Model_old;
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
                int needSpace = Mathf.CeilToInt(amount * 1.0f / model.StackNum);
                if (keyedSlots.TryGetValue(model.ID, out var find))
                {
                    int left = 0;
                    foreach (var slot in find)
                    {
                        left += model.StackNum - slot.amount;
                    }
                    needSpace = Mathf.CeilToInt((amount - left) * 1.0f / model.StackNum);
                    if (left < amount && needSpace > SpaceLimit - SpaceCost + vacatedSpace)
                    {
                        errorType = InventoryError.OverSpace;
                        return false;
                    }
                }
                else if (needSpace > SpaceLimit - SpaceCost + vacatedSpace)
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
    /// <param name="simulLoseItems">同时失去的道具</param>
    public void Get(ItemBase model, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (customGetAction != null && customGetAction(new ItemData(model, false), amount)) return;

        if (!model || amount < 1) return;
        WeightCost += model.Weight * amount;
        if (simulLoseItems != null && simulLoseItems.Length > 0) Lose(simulLoseItems);
        ItemData item = null;
        int oldAmount = 0;
        if (keyedSlots.TryGetValue(model.ID, out var find))
        {
            ItemWithAmount iwa = null;
            foreach (var slot in find)
            {
                oldAmount += slot.amount;
                if (!item)
                {
                    item = slot.item;
                    if (item) items.TryGetValue(item.ID, out iwa);
                }
                int take = slot.Put(amount);
                if (iwa) iwa.amount += take;
                amount -= take;
            }
        }
        else
        {
            find = new List<ItemSlotData>();
            keyedSlots.Add(model.ID, find);
        }
        CostEmptySlot(find);
        OnItemAmountChanged?.Invoke(model, oldAmount, oldAmount + amount);

        void CostEmptySlot(List<ItemSlotData> collection)
        {
            if (amount <= 0 || collection == null) return;
            if (model.StackAble && item == null) item = model.CreateData();//如果物品可叠加但没有在背包中找到，则这个是新物品
            ItemSlotData temp;
            for (int i = 0; i < amount / model.StackNum; i++)
            {
                temp = PutIntoEmptySlot(model.StackAble ? item : model.CreateData(), model.StackNum);
                if (temp) collection.Add(temp);
            }
            if (model.StackAble)
            {
                temp = PutIntoEmptySlot(item, amount % model.StackNum);
                if (temp) collection.Add(temp);
            }
        }
    }
    /// <summary>
    /// 按个例放入道具
    /// </summary>
    /// <param name="item">道具个例</param>
    /// <param name="amount">数量</param>
    /// <param name="simulLoseItems">同时失去的道具</param>
    public void Get(ItemData item, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (customGetAction != null && customGetAction(item, amount)) return;

        if (item.Model_old.StackAble || !item.IsInstance) Get(item.Model_old, amount, simulLoseItems);
        else if (!item.Model_old.StackAble && amount == 1)
        {
            int oldAmount = 0;
            if (simulLoseItems != null && simulLoseItems.Length > 0) Lose(simulLoseItems);
            if (keyedSlots.TryGetValue(item.ModelID, out var find))
            {
                oldAmount = find.Count;
                find.Add(PutIntoEmptySlot(item, 1));
            }
            else keyedSlots.Add(item.ModelID, new List<ItemSlotData>() { PutIntoEmptySlot(item, 1) });
            OnItemAmountChanged?.Invoke(item.Model_old, oldAmount, oldAmount + amount);
        }
    }
    /// <summary>
    /// 按列表放入道具
    /// </summary>
    /// <param name="infos">道具列表</param>
    /// <param name="simulLoseItems">同时失去的道具</param>
    public void Get(IEnumerable<ItemInfoBase> infos, params ItemWithAmount[] simulLoseItems)
    {
        if (infos == null) return;
        if (simulLoseItems != null && simulLoseItems.Length > 0) Lose(simulLoseItems);
        foreach (var info in infos)
        {
            if (info.IsValid) Get(info.item, info.Amount);
        }
    }
    /// <summary>
    /// 按个例列表放入道具
    /// </summary>
    /// <param name="items">道具列表</param>
    /// <param name="simulLoseItems">同时失去的道具</param>
    public void Get(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulLoseItems)
    {
        if (items == null) return;
        if (simulLoseItems != null && simulLoseItems.Length > 0) Lose(simulLoseItems);
        foreach (var item in items)
        {
            if (item.IsValid) Get(item.source, item.amount);
        }
    }
    #endregion

    #region 道具失去相关
    public bool PeekLose(string id, int amount, out InventoryError errorType, params ItemWithAmount[] simulGetItems)
    {
        if (string.IsNullOrEmpty(id) || amount < 1)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        if (TryGetData(id, out var data, out var have))
        {
            if (!ignoreLock && data.isLocked)
            {
                errorType = InventoryError.Locked;
                return false;
            }
            if (simulGetItems != null)
            {
                SimulateLose(data.Model_old, amount);
                if (!PeekGet(simulGetItems, out errorType))
                {
                    UnsimlulateLose(data.Model_old, amount);
                    return false;
                }
                UnsimlulateLose(data.Model_old, amount);
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
            errorType = InventoryError.Lack;
            return false;
        }
    }
    public bool PeekLose(ItemBase model, int amount, out InventoryError errorType, params ItemWithAmount[] simulGetItems)
    {
        if (!model)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        return PeekLose(model.ID, amount, out errorType, simulGetItems);
    }
    public bool PeekLose(ItemData item, int amount, out InventoryError errorType, params ItemWithAmount[] simulGetItems)
    {
        if (item && amount > 0)
            if (!ignoreLock && item.isLocked)
            {
                errorType = InventoryError.Locked;
                return false;
            }
            else if (!item.IsInstance)
                return PeekLose(item.Model_old, amount, out errorType, simulGetItems);
            else if (items.ContainsKey(item.ID))
            {
                if (item.Model_old.StackAble)
                    return PeekLose(item.ModelID, amount, out errorType, simulGetItems);
                errorType = InventoryError.None;
                return true;
            }
        errorType = InventoryError.Invalid;
        return false;
    }
    public bool PeekLose(IEnumerable<ItemWithAmount> items, out InventoryError errorType, params ItemWithAmount[] simulGetItems)
    {
        if (items == null)
        {
            errorType = InventoryError.Invalid;
            return false;
        }
        if (simulGetItems != null)
        {
            foreach (var item in items)
            {
                SimulateLose(item.source.Model_old, item.amount);
            }
            if (!PeekGet(simulGetItems, out errorType))
            {
                foreach (var item in items)
                {
                    UnsimlulateLose(item.source.Model_old, item.amount);
                }
                if (errorType == InventoryError.Overload)
                    errorType = InventoryError.PartialOverload;
                return false;
            }
            foreach (var item in items)
            {
                UnsimlulateLose(item.source.Model_old, item.amount);
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
    /// <param name="simulGetItems">同时获得的道具</param>
    public void Lose(string id, int amount, params ItemWithAmount[] simulGetItems)
    {
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
                    left -= slot.Take(left);
                    if (slot.IsEmpty) find.Remove(slot);
                }
                SpaceCost -= oldCount - find.Count;
                if (!ignoreWeight) WeightCost -= data.Model_old.Weight * amount;
                if (find.Count < 1) items.Remove(data.ID);
                else items[data.ID].amount -= amount;
                OnItemAmountChanged?.Invoke(data.Model_old, oldAmount, oldAmount - amount);
                if (simulGetItems != null && simulGetItems.Length > 0) Get(simulGetItems);
            }
        }
    }
    /// <summary>
    /// 失去道具，可选同时获得道具，无视限制
    /// </summary>
    /// <param name="model">原型</param>
    /// <param name="amount">数量</param>
    /// <param name="simulGetItems">同时获得的道具</param>
    public void Lose(ItemBase model, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!model) return;
        Lose(model.ID, amount, simulGetItems);
    }
    /// <summary>
    /// 失去道具，可选同时获得道具，无视限制
    /// </summary>
    /// <param name="item">道具个例</param>
    /// <param name="amount">数量</param>
    /// <param name="simulGetItems">同时获得的道具</param>
    public void Lose(ItemData item, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (customLoseAction != null && customLoseAction(item, amount)) return;

        if (!item || amount < 1) return;
        if (item.Model_old.StackAble || !item.IsInstance) Lose(item.ModelID, amount, simulGetItems);
        else if (!item.Model_old.StackAble && keyedSlots.TryGetValue(item.ModelID, out var find) && amount == 1)
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
                    if (!ignoreWeight) WeightCost -= item.Model_old.Weight;
                    i--;
                }
            }
            if (simulGetItems != null && simulGetItems.Length > 0) Get(simulGetItems);
            items.Remove(item.ID);
            OnItemAmountChanged?.Invoke(item.Model_old, oldAmount, oldAmount - amount);
        }
    }
    /// <summary>
    /// 失去多个道具，可选同时获得道具，无视限制
    /// </summary>
    /// <param name="items">道具与数量列表</param>
    /// <param name="simulGetItems">同时获得的道具</param>
    public void Lose(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulGetItems)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            if (item.IsValid) Lose(item.source, item.amount);
        }
        if (simulGetItems != null && simulGetItems.Length > 0) Get(simulGetItems);
    }
    #endregion

    #region 其它
    public bool HideItem(ItemData item)
    {
        if (!item || item.Model_old.StackAble) return false;
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
            OnItemAmountChanged(item.Model_old, iwa.amount, 0);
            return true;
        }
        else return false;
    }
    public bool UnhideItem(ItemData item)
    {
        if (!item || item.Model_old.StackAble) return false;
        if (hiddenItems.TryGetValue(item.ID, out var find) && find == item)
        {
            hiddenItems.Remove(item.ID);
            if (!ignoreSpace && SpaceCost + 1 > SpaceLimit) return false;
            PutIntoEmptySlot(item, 1);
            OnItemAmountChanged(item.Model_old, 0, 1);
            return true;
        }
        else return false;
    }
    public bool SwapHiddenItem(ItemData unhide, ItemData hide)
    {
        if (!unhide || !hide || hide.Model_old.StackAble) return false;
        return HideItem(hide) && UnhideItem(unhide);
    }

    private void SimulateLose(ItemBase model, int amount)
    {
        if (!ignoreWeight) WeightCost -= model.Weight * amount;
        if (!ignoreSpace) SpaceCost -= Mathf.FloorToInt(amount * 1.0f / model.StackNum);
    }
    private void UnsimlulateLose(ItemBase model, int amount)
    {
        if (!ignoreWeight) WeightCost += model.Weight * amount;
        if (!ignoreSpace) SpaceCost += Mathf.FloorToInt(amount * 1.0f / model.StackNum);
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
        slots.Sort(SlotComparer.Default);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].index = i;
        }
    }
    private ItemSlotData PutIntoEmptySlot(ItemData item, int amount)
    {
        if (!item || amount <= 0) return null;
        var slot = slots.Find(x => x.IsEmpty);
        if (slot) slot.Put(item, amount);
        else if (ignoreSpace) slot = MakeSlot();
        else throw new InvalidOperationException("往已满的库存中增加新道具");
        SpaceCost++;
        if (items.TryGetValue(item.ID, out var find)) find.amount += amount;
        else items.Add(item.ID, new ItemWithAmount(item, amount));
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

    public delegate void InventoryMoneyListener(Inventory inventory, long oldMoney);
    public delegate void InventorySpaceListener(Inventory inventory, int oldSpaceLimit);
    public delegate void InventoryWeightListener(Inventory inventory, float oldWeightLimit);
    public delegate void ItemAmountListener(ItemBase item, int oldAmount, int newAmount);
    public delegate void ItemSlotStateListener(ItemSlotData slot);
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
public interface IInventoryHolder
{
    public string Name { get; }

    public Inventory Inventory { get; }
}

public static class InventoryUtility
{
    /// <summary>
    /// 丢弃道具
    /// </summary>
    /// <param name="data">道具信息</param>
    public static void DiscardItem(IInventoryHandler handler, ItemData data, Vector3? amountPosition = null)
    {
        if (data == null || !data.Model_old) return;
        if (!handler.ContainsItem(data))
        {
            MessageManager.Instance.New($"该物品已不在{handler.Name}中");
            return;
        }
        if (!data.Model_old.DiscardAble)
        {
            MessageManager.Instance.New("该物品不可丢弃");
            return;
        }
        int amount = handler.GetAmount(data);
        if (amount < 2 && amount > 0)
        {
            ConfirmWindow.StartConfirm($"确定丢弃1个 [{ItemUtility.GetColorName(data.Model_old)}] 吗？",
                delegate
                {
                    if (handler.LoseItem(data, 1))
                        MessageManager.Instance.New($"丢掉了1个 [{data.Name}]");
                });
        }
        else AmountWindow.StartInput(delegate (long amount)
        {
            ConfirmWindow.StartConfirm($"确定丢弃{amount}个 [{ItemUtility.GetColorName(data.Model_old)}] 吗？",
                delegate
                {
                    if (handler.LoseItem(data, (int)amount))
                        MessageManager.Instance.New($"丢掉了{amount}个 [{data.Name}]");
                });
        }, amount, "丢弃数量", amountPosition);
    }
}
#endregion