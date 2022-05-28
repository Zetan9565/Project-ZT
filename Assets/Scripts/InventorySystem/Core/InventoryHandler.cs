using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZetanStudio.Item;
using ZetanStudio.Item.Craft;
using ZetanStudio.Item.Module;

public abstract class InventoryHandler : MonoBehaviour, IInventoryHandler
{
    [SerializeField]
    private string _name = "库存";
    public virtual string Name => _name;
    public virtual Inventory Inventory { get; protected set; }

    public abstract string InventoryMoneyChangedMsgKey { get; }
    public abstract string InventorySpaceChangedMsgKey { get; }
    public abstract string InventoryWeightChangedMsgKey { get; }
    public abstract string ItemAmountChangedMsgKey { get; }
    public abstract string SlotStateChangedMsgKey { get; }

    private void Awake()
    {
        OnAwake();
    }
    protected virtual void OnAwake() { }

    public int GetAmount(string id)
    {
        return Inventory.GetAmount(id);
    }
    public int GetAmount(Item model)
    {
        return Inventory.GetAmount(model);
    }
    public int GetAmount(ItemData item)
    {
        return Inventory.GetAmount(item);
    }

    public bool GetItem(Item item, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!CanGet(item, amount, simulLoseItems))
            return false;
        Inventory.Get(item, amount, simulLoseItems);
        return true;
    }
    public bool GetItem(ItemData data, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!data) return false;
        if (!CanGet(data.Model, amount, simulLoseItems))
            return false;
        Inventory.Get(data, amount, simulLoseItems);
        return true;
    }
    public bool GetItem(IEnumerable<ItemInfo> infos, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!CanGet(infos, simulLoseItems))
            return false;
        Inventory.Get(infos, simulLoseItems);
        return true;
    }
    public bool GetItem(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!CanGet(items, simulLoseItems))
            return false;
        Inventory.Get(items, simulLoseItems);
        return true;
    }

    public bool LoseItem(string id, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!CanLose(id, amount, simulGetItems))
            return false;
        Inventory.Lose(id, amount, simulGetItems);
        return true;
    }
    public bool LoseItem(Item model, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!CanLose(model, amount, simulGetItems))
            return false;
        Inventory.Lose(model, amount, simulGetItems);
        return true;
    }
    public bool LoseItem(ItemData data, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!CanLose(data, amount, simulGetItems))
            return false;
        Inventory.Lose(data, amount, simulGetItems);
        return true;
    }
    public bool LoseItem(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (items == null) return false;
        if (!CanLose(items, simulGetItems))
            return false;
        Inventory.Lose(items, simulGetItems);
        return true;
    }

    public bool CanLose(string id, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekLose(id, amount, out var error, simulGetItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanLose(Item model, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekLose(model, amount, out var error, simulGetItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanLose(ItemData data, int amount, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekLose(data, amount, out var error, simulGetItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanLose(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulGetItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekLose(items, out var error, simulGetItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }

    public bool CanGet(Item item, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekGet(item, amount, out var error, simulLoseItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanGet(ItemData item, int amount, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekGet(item, amount, out var error, simulLoseItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanGet(IEnumerable<ItemInfo> infos, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekGet(infos, out var error, simulLoseItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }
    public bool CanGet(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulLoseItems)
    {
        if (!Inventory) return false;

        if (!Inventory.PeekGet(items, out var error, simulLoseItems))
        {
            SayError(error);
            return false;
        }
        return true;
    }

    public bool HasItemWithID(string id)
    {
        return Inventory?.GetAmount(id) > 0;
    }
    public bool HasItem(Item item)
    {
        return Inventory?.GetAmount(item) > 0;
    }

    public bool GetItemData(string id, out ItemData item, out int amount)
    {
        if (!Inventory)
        {
            item = null;
            amount = 0;
            return false;
        }

        return Inventory.TryGetData(id, out item, out amount);
    }
    public bool GetItemData(Item model, out ItemData item, out int amount)
    {
        if (!Inventory)
        {
            item = null;
            amount = 0;
            return false;
        }

        return Inventory.TryGetData(model, out item, out amount);
    }
    public bool GetItemDatas(string id, out List<ItemWithAmount> results)
    {
        if (!Inventory)
        {
            results = null;
            return false;
        }

        return Inventory.TryGetDatas(id, out results);
    }
    public bool GetItemDatas(Item model, out List<ItemWithAmount> results)
    {
        if (!Inventory)
        {
            results = null;
            return false;
        }

        return Inventory.TryGetDatas(model, out results);
    }

    public void GetMoney(long money)
    {
        Inventory?.GetMoney(money);
    }
    public bool CanLoseMoney(long money)
    {
        if (!Inventory) return false;

        if (Inventory?.Money < money)
        {
            MessageManager.Instance.New($"{Name}中没有这么多钱");
            return false;
        }
        else return true;
    }
    public bool LoseMoney(long money)
    {
        if (!Inventory) return false;

        if (CanLoseMoney(money))
        {
            Inventory.LoseMoney(money);
            return true;
        }
        return false;
    }

    public bool ContainsItem(ItemData item)
    {
        return Inventory?.ContainsItem(item) ?? false;
    }

    protected virtual void SayError(InventoryError error)
    {
        switch (error)
        {
            case InventoryError.Locked:
                MessageManager.Instance.New("已锁定");
                break;
            case InventoryError.PartialLocked:
                MessageManager.Instance.New("部分物品已锁定");
                break;
            case InventoryError.Overload:
                MessageManager.Instance.New("太重了");
                break;
            case InventoryError.PartialOverload:
                MessageManager.Instance.New("这些物品太重了");
                break;
            case InventoryError.OverSpace:
                MessageManager.Instance.New("空间不足");
                break;
            case InventoryError.Lack:
                MessageManager.Instance.New("数量不足");
                break;
            case InventoryError.PartialLack:
                MessageManager.Instance.New("部分道具数量不足");
                break;
            case InventoryError.Invalid:
                Debug.Log("参数无效");
                break;
        }
    }

    #region 材料相关
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> materials)
    {
        if (!Inventory || materials == null) return false;
        if (materials.Count() < 1) return true;
        var materialEnum = materials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (materialEnum.Current.MakingType == CraftType.SingleItem)
            {
                if (GetAmount(materialEnum.Current.Item) < materialEnum.Current.Amount) return false;
            }
            else
            {

                int amount = Inventory.GetAmount(x => MaterialModule.Compare(x.Model, materialEnum.Current.MaterialType));
                if (amount < materialEnum.Current.Amount) return false;
            }
        }
        return true;
    }
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials)
    {
        if (!Inventory || targetMaterials == null || targetMaterials.Count() < 1 || givenMaterials == null || givenMaterials.Count() < 1 || targetMaterials.Count() != givenMaterials.Count()) return false;
        foreach (var material in targetMaterials)
        {
            if (material.MakingType == CraftType.SingleItem)
            {
                ItemInfo find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                if (!find) return false;//所提供的材料中没有这种材料
                if (find.Amount != material.Amount) return false;//若材料数量不符合，则无法制作
                else if (GetAmount(find.ItemID) < material.Amount) return false;//背包中材料数量不足
            }
            else
            {
                var finds = givenMaterials.Where(x => MaterialModule.Compare(x.item, material.MaterialType));//找到种类相同的道具
                if (finds.Count() > 0)
                {
                    if (finds.Select(x => x.Amount).Sum() != material.Amount) return false;//若材料总数不符合，则无法制作
                    foreach (var find in finds)
                    {
                        if (GetAmount(find.item) < find.Amount || QuestManager.Instance.HasQuestRequiredItem(find.item, GetAmount(find.item) - find.Amount))
                        {
                            return false;
                        }//若任意一个相应数量的材料无法失去（包括数量不足），则会导致总数量不符合，所以无法制作
                    }
                }
                else return false;//材料不足
            }
        }
        return true;
    }

    public List<ItemWithAmount> GetMaterialsFromInventory(IEnumerable<MaterialInfo> targetMaterials)
    {
        if (!Inventory || targetMaterials == null) return null;

        List<ItemWithAmount> items = new List<ItemWithAmount>();
        HashSet<string> itemsToken = new HashSet<string>();
        if (targetMaterials.Count() < 1) return items;

        var materialEnum = targetMaterials.GetEnumerator();
        while (materialEnum.MoveNext())
        {
            if (materialEnum.Current.MakingType == CraftType.SingleItem)
            {
                if (materialEnum.Current.Item.StackAble)
                {
                    if (Inventory.TryGetData(materialEnum.Current.Item, out var item, out var amount))
                    {
                        int need = materialEnum.Current.Amount;
                        int takeAmount = 0;
                        if (itemsToken.Contains(item.ID))//被选取过了
                        {
                            var find = items.Find(x => x.source == item);
                            int left = amount - find.amount;
                            left = left > need ? need : left;
                            takeAmount = left;
                        }
                        else
                        {
                            takeAmount = amount > need ? need : amount;
                        }
                        TakeItem(item, takeAmount);
                    }
                }
                else
                {
                    int need = materialEnum.Current.Amount;
                    Inventory.TryGetDatas(materialEnum.Current.Item, out var finds);
                    foreach (var find in finds)
                    {
                        if (need > 0)
                        {
                            if (!itemsToken.Contains(find.source.ID))
                            {
                                TakeItem(find.source, 1);
                                need--;
                            }
                        }
                        else break;
                    }
                }
            }
            else
            {
                Inventory.TryGetDatas(x => MaterialModule.Compare(x.Model, materialEnum.Current.MaterialType), out var finds);
                if (finds.Count > 0)
                {
                    int need = materialEnum.Current.Amount;
                    foreach (var find in finds)
                    {
                        int takeAmount = 0;
                        int leftAmount = find.amount;
                        if (itemsToken.Contains(find.source.ID))
                        {
                            if (!find.source.Model.StackAble) continue;//不可叠加且选取过了，则跳过选取
                            else
                            {
                                ItemWithAmount find2 = items.Find(x => x.source == find);
                                leftAmount = find.amount - find2.amount;
                            }
                        }
                        if (leftAmount < need)
                        {
                            takeAmount = leftAmount;
                            need -= takeAmount;
                        }
                        else
                        {
                            takeAmount = need;
                            need = 0;
                        }
                        TakeItem(find.source, takeAmount);
                    }
                }
            }
        }
        return items;

        void TakeItem(ItemData item, int amount)
        {
            if (itemsToken.Contains(item.ID))
            {
                if (item.Model.StackAble)
                {
                    var find = items.Find(x => x.source == item);
                    find.amount += amount;
                }
            }
            else
            {
                items.Add(new ItemWithAmount(item, amount));
                itemsToken.Add(item.ID);
            }
        }
    }

    public List<string> GetMaterialsInfoString(IEnumerable<MaterialInfo> materials)
    {
        List<string> info = new List<string>();
        if (!Inventory) return info;
        using (var materialEnum = materials.GetEnumerator())
            while (materialEnum.MoveNext())
                if (materialEnum.Current.MakingType == CraftType.SingleItem)
                    info.Add(string.Format("{0}\t[{1}/{2}]", materialEnum.Current.ItemName, GetAmount(materialEnum.Current.Item), materialEnum.Current.Amount));
                else
                {
                    Inventory.TryGetDatas(x => MaterialModule.Compare(x.Model, materialEnum.Current.MaterialType), out var finds);
                    int amount = 0;
                    foreach (var item in finds)
                    {
                        amount += item.amount;
                    }
                    info.Add(string.Format("{0}\t[{1}/{2}]", materialEnum.Current.MaterialType.Name, amount, materialEnum.Current.Amount));
                }
        return info;
    }

    public int GetAmountCanMake(IEnumerable<MaterialInfo> materials)
    {
        if (!Inventory) return 0;
        if (materials.Count() < 1) return 1;
        List<int> amounts = new List<int>();
        using (var materialEnum = materials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                if (materialEnum.Current.MakingType == CraftType.SingleItem)
                    amounts.Add(GetAmount(materialEnum.Current.Item) / materialEnum.Current.Amount);
                else
                {
                    Inventory.TryGetDatas(x => MaterialModule.Compare(x.Model, materialEnum.Current.MaterialType), out var finds);
                    int amount = 0;
                    foreach (var item in finds)
                    {
                        amount += item.amount;
                    }
                    amounts.Add(amount / materialEnum.Current.Amount);
                }
            }
        return amounts.Min();
    }
    public int GetAmountCanMake(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials)
    {
        if (!Inventory || givenMaterials == null || givenMaterials.Count() < 1 || targetMaterials == null || targetMaterials.Count() < 1 || targetMaterials.Count() != givenMaterials.Count()) return 0;
        List<int> amounts = new List<int>();
        foreach (var material in targetMaterials)
        {
            if (material.MakingType == CraftType.SingleItem)
            {
                ItemInfo find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                if (!find) return 0;//所提供的材料中没有这种材料
                if (find.Amount != material.Amount) return 0;//若材料数量不符合，则无法制作
                amounts.Add(GetAmount(find.ItemID) / material.Amount);
            }
            else
            {
                var finds = givenMaterials.Where(x => MaterialModule.Compare(x.item, material.MaterialType));//找到种类相同的道具
                if (finds.Count() > 0)
                {
                    if (finds.Select(x => x.Amount).Sum() != material.Amount) return 0;//若材料总数不符合，则无法制作
                    foreach (var find in finds)
                    {
                        int amount = GetAmount(find.ItemID);
                        if (QuestManager.Instance.HasQuestRequiredItem(find.item, GetAmount(find.item) - find.Amount))
                            return 0;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                        amounts.Add(amount / find.Amount);
                    }
                }
                else return 0;//材料不足
            }
        }

        return amounts.Min();
    }
    #endregion

    #region 事件相关
    protected void ListenInventoryChange(bool listen)
    {
        if (Inventory)
            if (listen)
            {
                Inventory.OnInventoryMoneyChanged += OnInventoryMoneyChanged;
                Inventory.OnInventorySpaceChanged += OnInventorySpaceChanged;
                Inventory.OnInventoryWeightChanged += OnInventoryWeightChanged;
                Inventory.OnItemAmountChanged += OnItemAmountChanged;
                Inventory.OnSlotStateChanged += OnSlotStateChanged;
            }
            else
            {
                Inventory.OnInventoryMoneyChanged -= OnInventoryMoneyChanged;
                Inventory.OnInventorySpaceChanged -= OnInventorySpaceChanged;
                Inventory.OnInventoryWeightChanged -= OnInventoryWeightChanged;
                Inventory.OnItemAmountChanged -= OnItemAmountChanged;
                Inventory.OnSlotStateChanged -= OnSlotStateChanged;
            }
    }
    protected virtual void OnInventoryMoneyChanged(Inventory inventory, long oldMoney)
    {
        if (inventory == Inventory) NotifyCenter.PostNotify(InventoryMoneyChangedMsgKey, inventory, oldMoney);
    }
    protected virtual void OnInventorySpaceChanged(Inventory inventory, int oldSpaceLimit)
    {
        if (inventory == Inventory) NotifyCenter.PostNotify(InventoryMoneyChangedMsgKey, inventory, oldSpaceLimit);
    }
    protected virtual void OnInventoryWeightChanged(Inventory inventory, float oldWeightLimit)
    {
        if (inventory == Inventory) NotifyCenter.PostNotify(InventoryMoneyChangedMsgKey, inventory, oldWeightLimit);
    }
    protected virtual void OnItemAmountChanged(Item model, int oldAmount, int newAmount)
    {
        NotifyCenter.PostNotify(ItemAmountChangedMsgKey, model, oldAmount, newAmount);
    }
    protected virtual void OnSlotStateChanged(ItemSlotData slot)
    {
        NotifyCenter.PostNotify(SlotStateChangedMsgKey, slot);
    }
    #endregion
}

public abstract class SingletonInventoryHandler<T> : InventoryHandler where T : InventoryHandler
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }
}

public interface IInventoryHandler : IInventoryHolder
{
    /// <summary>
    /// 金币更新消息，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的金币数量：<see cref="long"/>])
    /// </summary>
    public string InventoryMoneyChangedMsgKey { get; }
    /// <summary>
    /// 空间上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的空间上限：<see cref="int"/>])
    /// </summary>
    public string InventorySpaceChangedMsgKey { get; }
    /// <summary>
    /// 负重上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的负重上限：<see cref="float"/>])
    /// </summary>
    public string InventoryWeightChangedMsgKey { get; }
    /// <summary>
    /// 道具更新消息，格式：([发生变化的道具：<see cref="ItemData"/>], [旧的数量：<see cref="int"/>], [新的数量：<see cref="int"/>])
    /// </summary>
    public string ItemAmountChangedMsgKey { get; }
    /// <summary>
    /// 道具槽更新消息，格式：([发生变化的道具槽：<see cref="ItemSlotData"/>])
    /// </summary>
    public string SlotStateChangedMsgKey { get; }

    public int GetAmount(string id);
    public int GetAmount(Item model);
    public int GetAmount(ItemData item);

    public bool GetItem(Item item, int amount, params ItemWithAmount[] simulLoseItems);
    public bool GetItem(ItemData data, int amount, params ItemWithAmount[] simulLoseItems);
    public bool GetItem(IEnumerable<ItemInfo> infos, params ItemWithAmount[] simulLoseItems);
    public bool GetItem(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulLoseItems);

    public bool LoseItem(string id, int amount, params ItemWithAmount[] simulGetItems);
    public bool LoseItem(Item model, int amount, params ItemWithAmount[] simulGetItems);
    public bool LoseItem(ItemData data, int amount, params ItemWithAmount[] simulGetItems);
    public bool LoseItem(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulGetItems);

    public bool CanLose(string id, int amount, params ItemWithAmount[] simulGetItems);
    public bool CanLose(Item model, int amount, params ItemWithAmount[] simulGetItems);
    public bool CanLose(ItemData data, int amount, params ItemWithAmount[] simulGetItems);
    public bool CanLose(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulGetItems);

    public bool CanGet(Item item, int amount, params ItemWithAmount[] simulLoseItems);
    public bool CanGet(ItemData item, int amount, params ItemWithAmount[] simulLoseItems);
    public bool CanGet(IEnumerable<ItemInfo> infos, params ItemWithAmount[] simulLoseItems);
    public bool CanGet(IEnumerable<ItemWithAmount> items, params ItemWithAmount[] simulLoseItems);

    public bool HasItemWithID(string id);
    public bool HasItem(Item item);

    public bool GetItemData(string id, out ItemData item, out int amount);
    public bool GetItemData(Item model, out ItemData item, out int amount);
    public bool GetItemDatas(string id, out List<ItemWithAmount> results);
    public bool GetItemDatas(Item model, out List<ItemWithAmount> results);

    public void GetMoney(long money);
    public bool CanLoseMoney(long money);
    public bool LoseMoney(long money);

    public bool ContainsItem(ItemData item);

    #region 材料相关
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> materials);
    public bool IsMaterialsEnough(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials);
    public List<ItemWithAmount> GetMaterialsFromInventory(IEnumerable<MaterialInfo> targetMaterials);
    public List<string> GetMaterialsInfoString(IEnumerable<MaterialInfo> materials);
    public int GetAmountCanMake(IEnumerable<MaterialInfo> materials);
    public int GetAmountCanMake(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials);
    #endregion
}