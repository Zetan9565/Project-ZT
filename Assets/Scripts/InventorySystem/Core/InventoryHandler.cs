using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;
using ZetanStudio.QuestSystem;

namespace ZetanStudio.InventorySystem
{
    public abstract class InventoryHandler
    {
        protected string _name = "库存";
        public virtual string Name => _name;
        public virtual Inventory Inventory { get; protected set; }

        /// <summary>
        /// 金币更新消息，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的金币数量：<see cref="long"/>])
        /// </summary>
        public abstract string InventoryMoneyChangedMsgKey { get; }
        /// <summary>
        /// 空间上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的空间上限：<see cref="int"/>])
        /// </summary>
        public abstract string InventorySpaceChangedMsgKey { get; }
        /// <summary>
        /// 负重上限更新事件，格式：([发生变化的库存：<see cref="Inventory"/>], [旧的负重上限：<see cref="float"/>])
        /// </summary>
        public abstract string InventoryWeightChangedMsgKey { get; }
        /// <summary>
        /// 道具更新消息，格式：([发生变化的道具：<see cref="ItemData"/>], [旧的数量：<see cref="int"/>], [新的数量：<see cref="int"/>])
        /// </summary>
        public abstract string ItemAmountChangedMsgKey { get; }
        /// <summary>
        /// 道具槽更新消息，格式：([发生变化的道具槽：<see cref="ItemSlotData"/>])
        /// </summary>
        public abstract string SlotStateChangedMsgKey { get; }

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

        public bool Get(Item item, int amount, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!CanGet(item, amount, itemsToLose))
                return false;
            Inventory.Get(item, amount, itemsToLose);
            return true;
        }
        public bool Get(ItemData data, int amount, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!data) return false;
            if (!CanGet(data.Model, amount, itemsToLose))
                return false;
            Inventory.Get(data, amount, itemsToLose);
            return true;
        }
        public bool Get(IEnumerable<ItemInfo> infos, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!CanGet(infos, itemsToLose))
                return false;
            Inventory.Get(infos, itemsToLose);
            return true;
        }
        public bool Get(IEnumerable<CountedItem> items, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!CanGet(items, itemsToLose))
                return false;
            Inventory.Get(items, itemsToLose);
            return true;
        }

        public bool Lose(string id, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!CanLose(id, amount, itemsToGet))
                return false;
            Inventory.Lose(id, amount, itemsToGet);
            return true;
        }
        public bool Lose(Item model, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!CanLose(model, amount, itemsToGet))
                return false;
            Inventory.Lose(model, amount, itemsToGet);
            return true;
        }
        public bool Lose(ItemData data, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!CanLose(data, amount, itemsToGet))
                return false;
            Inventory.Lose(data, amount, itemsToGet);
            return true;
        }
        public bool Lose(IEnumerable<CountedItem> items, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (items == null) return false;
            if (!CanLose(items, itemsToGet))
                return false;
            Inventory.Lose(items, itemsToGet);
            return true;
        }

        public bool CanLose(string id, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekLose(id, amount, out var error, itemsToGet))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanLose(Item model, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekLose(model, amount, out var error, itemsToGet))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanLose(ItemData data, int amount, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekLose(data, amount, out var error, itemsToGet))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanLose(IEnumerable<CountedItem> items, params CountedItem[] itemsToGet)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekLose(items, out var error, itemsToGet))
            {
                SayError(error);
                return false;
            }
            return true;
        }

        public bool CanGet(Item item, int amount, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekGet(item, amount, out var error, itemsToLose))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanGet(ItemData item, int amount, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekGet(item, amount, out var error, itemsToLose))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanGet(IEnumerable<ItemInfo> infos, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekGet(infos, out var error, itemsToLose))
            {
                SayError(error);
                return false;
            }
            return true;
        }
        public bool CanGet(IEnumerable<CountedItem> items, params CountedItem[] itemsToLose)
        {
            if (!Inventory) return false;

            if (!Inventory.PeekGet(items, out var error, itemsToLose))
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
        public bool GetItemDatas(string id, out List<CountedItem> results)
        {
            if (!Inventory)
            {
                results = null;
                return false;
            }

            return Inventory.TryGetDatas(id, out results);
        }
        public bool GetItemDatas(Item model, out List<CountedItem> results)
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
            if (!materials.Any()) return true;
            var materialEnum = materials.GetEnumerator();
            while (materialEnum.MoveNext())
            {
                if (materialEnum.Current.CostType == MaterialCostType.SingleItem)
                {
                    if (GetAmount(materialEnum.Current.Item) < materialEnum.Current.Amount) return false;
                }
                else
                {
                    int amount = Inventory.GetAmount(x => MaterialModule.SameType(materialEnum.Current.MaterialType, x.Model));
                    if (amount < materialEnum.Current.Amount) return false;
                }
            }
            return true;
        }
        public bool IsMaterialsEnough(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials)
        {
            if (!Inventory || targetMaterials == null || !targetMaterials.Any() || givenMaterials == null || !givenMaterials.Any() || targetMaterials.Count() != givenMaterials.Count()) return false;
            foreach (var material in targetMaterials)
            {
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    ItemInfo find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return false;//若材料数量不符合，则无法制作
                    else if (GetAmount(find.ItemID) < material.Amount) return false;//背包中材料数量不足
                }
                else
                {
                    var finds = givenMaterials.Where(x => MaterialModule.SameType(material.MaterialType, x.Item));//找到种类相同的道具
                    if (finds.Any())
                    {
                        if (finds.Select(x => x.Amount).Sum() != material.Amount) return false;//若材料总数不符合，则无法制作
                        foreach (var find in finds)
                        {
                            if (GetAmount(find.Item) < find.Amount || QuestManager.HasQuestRequiredItem(find.Item, GetAmount(find.Item) - find.Amount))
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

        public List<CountedItem> GetMaterials(IEnumerable<MaterialInfo> targetMaterials)
        {
            if (!Inventory || targetMaterials == null) return null;

            List<CountedItem> items = new List<CountedItem>();
            HashSet<string> itemsToken = new HashSet<string>();
            if (!targetMaterials.Any()) return items;

            var materialEnum = targetMaterials.GetEnumerator();
            while (materialEnum.MoveNext())
            {
                if (materialEnum.Current.CostType == MaterialCostType.SingleItem)
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
                    Inventory.TryGetDatas(x => MaterialModule.SameType(materialEnum.Current.MaterialType, x.Model), out var finds);
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
                                    CountedItem find2 = items.Find(x => x.source == find);
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
                    items.Add(new CountedItem(item, amount));
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
                    if (materialEnum.Current.CostType == MaterialCostType.SingleItem)
                        info.Add(string.Format("{0}\t[{1}/{2}]", materialEnum.Current.ItemName, GetAmount(materialEnum.Current.Item), materialEnum.Current.Amount));
                    else
                    {
                        Inventory.TryGetDatas(x => MaterialModule.SameType(materialEnum.Current.MaterialType, x.Model), out var finds);
                        int amount = 0;
                        foreach (var item in finds)
                        {
                            amount += item.amount;
                        }
                        info.Add(string.Format("{0}\t[{1}/{2}]", materialEnum.Current.MaterialType.Name, amount, materialEnum.Current.Amount));
                    }
            return info;
        }
        public string GetMaterialsAmountString(MaterialInfo material)
        {
            if (material.CostType == MaterialCostType.SingleItem) return $"{GetAmount(material.Item)}/{material.Amount}";
            else
            {
                Inventory.TryGetDatas(x => MaterialModule.SameType(material.MaterialType, x.Model), out var finds);
                int amount = 0;
                foreach (var item in finds)
                {
                    amount += item.amount;
                }
                return $"{amount}/{material.Amount}";
            }
        }
        public int GetMaterialsAmount(MaterialInfo material)
        {
            if (material.CostType == MaterialCostType.SingleItem) return GetAmount(material.Item);
            else
            {
                Inventory.TryGetDatas(x => MaterialModule.SameType(material.MaterialType, x.Model), out var finds);
                int amount = 0;
                foreach (var item in finds)
                {
                    amount += item.amount;
                }
                return amount;
            }
        }

        public int GetAmountCanCraft(IEnumerable<MaterialInfo> materials)
        {
            if (!Inventory) return 0;
            if (!materials.Any()) return 1;
            List<int> amounts = new List<int>();
            using (var materialEnum = materials.GetEnumerator())
                while (materialEnum.MoveNext())
                {
                    if (materialEnum.Current.CostType == MaterialCostType.SingleItem)
                        amounts.Add(GetAmount(materialEnum.Current.Item) / materialEnum.Current.Amount);
                    else
                    {
                        Inventory.TryGetDatas(x => MaterialModule.SameType(materialEnum.Current.MaterialType, x.Model), out var finds);
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
        public int GetAmountCanCraft(IEnumerable<MaterialInfo> targetMaterials, IEnumerable<ItemInfo> givenMaterials)
        {
            if (!Inventory || givenMaterials == null || !givenMaterials.Any() || targetMaterials == null || !targetMaterials.Any() || targetMaterials.Count() != givenMaterials.Count()) return 0;
            List<int> amounts = new List<int>();
            foreach (var material in targetMaterials)
            {
                if (material.CostType == MaterialCostType.SingleItem)
                {
                    ItemInfo find = givenMaterials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return 0;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return 0;//若材料数量不符合，则无法制作
                    amounts.Add(GetAmount(find.ItemID) / material.Amount);
                }
                else
                {
                    var finds = givenMaterials.Where(x => MaterialModule.SameType(material.MaterialType, x.Item));//找到种类相同的道具
                    if (finds.Any())
                    {
                        if (finds.Select(x => x.Amount).Sum() != material.Amount) return 0;//若材料总数不符合，则无法制作
                        foreach (var find in finds)
                        {
                            int amount = GetAmount(find.ItemID);
                            if (QuestManager.HasQuestRequiredItem(find.Item, GetAmount(find.Item) - find.Amount))
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

        public static implicit operator bool(InventoryHandler self)
        {
            return self != null;
        }
    }
}