﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZetanStudio.ItemSystem.UI
{
    using Extension;
    using Collections;
    using ZetanStudio.UI;
    using ZetanStudio;
    using ZetanStudio.InventorySystem;

    [DisallowMultipleComponent]
    public class ItemSelectionWindow : Window
    {
        public Text windowTitle;
        public GameObject tips;

        public ItemSlotEx itemCellPrefab;
        public RectTransform itemCellsParent;

        public ScrollRect gridScrollRect;
        public GameObject placementArea;

        public Button confirmButton;
        public Button clearButton;

        public ISlotContainer SourceContainer { get; private set; }
        public InventoryHandler SourceHandler { get; private set; }

        private bool confirm;

        private SimplePool<ItemSlotEx> caches;

        protected override void OnAwake()
        {
            confirmButton.onClick.AddListener(Confirm);
            clearButton.onClick.AddListener(Clear);
            var pool = itemCellsParent.FindOrCreate("Caches");
            Utility.SetActive(pool, false);
            caches = new SimplePool<ItemSlotEx>(itemCellPrefab, poolRoot: pool);
        }

        public bool IsSelecting { get; private set; }

        public GameObject PlacementArea => placementArea;

        private readonly Dictionary<string, ItemSlotEx> copySlots = new Dictionary<string, ItemSlotEx>();
        private readonly HashSet<ItemData> selectedItems = new HashSet<ItemData>();
        public ReadOnlySet<ItemData> SelectedItems => new ReadOnlySet<ItemData>(selectedItems);

        public ItemSelectionType SelectionType { get; private set; }

        private int typeLimit;
        private Func<ItemData, int> amountLimit;
        private Predicate<ItemData> canSelect;
        private Action<List<CountedItem>> onConfirm;
        private Func<List<CountedItem>, bool> onConfirm_check;
        private Action onCancel;

        private string dialog;

        public static ItemSelectionWindow StartSelection(ItemSelectionType selectionType, ISlotContainer container, InventoryHandler handler, Action<List<CountedItem>> confirm, string title = null, string confirmDialog = null,
            int? typeLimit = null, Func<ItemData, int> amountLimit = null, Predicate<ItemData> selectCondition = null, Action cancel = null)
        {
            return WindowsManager.OpenWindow<ItemSelectionWindow>(selectionType, container, handler, confirm, title, confirmDialog, typeLimit ?? default, amountLimit, selectCondition, cancel);
        }
        public static ItemSelectionWindow StartSelection(ItemSelectionType selectionType, ISlotContainer container, InventoryHandler handler, Func<List<CountedItem>, bool> confirm, string title = null, string confirmDialog = null,
            int? typeLimit = null, Func<ItemData, int> amountLimit = null, Predicate<ItemData> selectCondition = null, Action cancel = null)
        {
            return WindowsManager.OpenWindow<ItemSelectionWindow>(selectionType, container, handler, confirm, title, confirmDialog, typeLimit ?? default, amountLimit, selectCondition, cancel);
        }

        public void Confirm()
        {
            if (copySlots.Count < 1)
            {
                MessageManager.Instance.New("未选择任何物品");
                return;
            }
            List<CountedItem> items = new List<CountedItem>();
            foreach (var kvp in copySlots)
            {
                items.Add(new CountedItem(kvp.Value.Item, kvp.Value.Data.amount));
            }
            confirm = true;
            if (string.IsNullOrEmpty(dialog))
            {
                if (onConfirm_check == null)
                {
                    onConfirm?.Invoke(items);
                    Close();
                }
                else if (onConfirm_check(items))
                    Close();
            }
            else ConfirmWindow.StartConfirm(dialog, () =>
            {
                if (onConfirm_check == null)
                {
                    onConfirm?.Invoke(items);
                    Close();
                }
                else if (onConfirm_check(items))
                    Close();
            });
        }

        public void Clear()
        {
            foreach (var kvp in copySlots)
            {
                caches.Put(kvp.Value);
            }
            if (SourceContainer != null) SourceContainer.MarkIf(null);
            copySlots.Clear();
            selectedItems.Clear();
            Utility.SetActive(tips, true);
            WindowsManager.CloseWindow<ItemWindow>();
        }

        public void Place(ItemSlotEx source)
        {
            if (!source || source.IsEmpty || source.Data.IsEmpty) return;
            var slot = source.Data;
            int have = SourceHandler.GetAmount(slot.item);
            if (slot == null || slot.Model == null || have < 0) return;
            var amountLimit = this.amountLimit?.Invoke(source.Item) ?? 0;
            if (slot.Model.StackAble && SelectionType == ItemSelectionType.SelectNum)
            {
                if (!canSelect(source.Item)) return;
                if (typeLimit > 0 && copySlots.Count >= typeLimit)
                {
                    MessageManager.Instance.New($"每次最多只能选择{typeLimit}样物品");
                    return;
                }
                if (have < 2 || amountLimit == 1)
                {
                    if (SourceHandler.CanLose(slot.item, 1))
                        MakeSlot(slot.item, 1);
                }
                else
                {
                    AmountWindow.StartInput(delegate (long amount)
                    {
                        if (SourceHandler.CanLose(slot.item, (int)amount))
                        {
                            if (copySlots.TryGetValue(slot.item.ID, out var copy))
                            {
                                copy.Data.amount = (int)amount;
                                copy.Refresh();
                            }
                            else MakeSlot(slot.item, (int)amount);
                            if (copySlots.Count > 0) Utility.SetActive(tips, false);
                        }
                    }, amountLimit > 0 ? (amountLimit < have ? amountLimit : have) : have);
                }
            }
            else if ((!slot.Model.StackAble && SelectionType == ItemSelectionType.SelectNum || SelectionType == ItemSelectionType.SelectAll)
                && canSelect(source.Item) && SourceHandler.CanLose(slot.item, have))
            {
                if (copySlots.ContainsKey(slot.item.ID))
                {
                    MessageManager.Instance.New("已选择该物品");
                    return;
                }
                if (typeLimit > 0 && copySlots.Count >= typeLimit)
                {
                    MessageManager.Instance.New($"每次最多只能选择{typeLimit}样物品");
                }
                else MakeSlot(slot.item, have);
            }

            void MakeSlot(ItemData data, int amount)
            {
                ItemSlotEx copy = caches.Get(itemCellsParent);
                copy.SetScrollRect(gridScrollRect);
                copy.SetCallbacks(GetHandleButtons, (s) => TakeOut(s, true), OnSlotEndDrag);
                copySlots.Add(data.ID, copy);
                selectedItems.Add(data);
                copy.Refresh(new ItemSlotData(data, amount));
                SourceContainer.MarkIf(s => selectedItems.Contains(s.Item));
                if (copySlots.Count > 0) Utility.SetActive(tips, false);
            }
        }

        private ButtonWithTextData[] GetHandleButtons(ItemSlotEx slot)
        {
            if (!slot || slot.IsEmpty) return null;

            List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
            if (SelectionType == ItemSelectionType.SelectNum)
                buttons.Add(new ButtonWithTextData("取出", delegate
                {
                    TakeOut(slot);
                }));
            if (slot.Data.amount > 1 || SelectionType == ItemSelectionType.SelectAll)
                buttons.Add(new ButtonWithTextData("全部取出", delegate
                {
                    TakeOut(slot, true);
                }));
            return buttons.ToArray();
        }

        public bool ContainsSlot(ItemSlotEx slot)
        {
            if (!slot || slot.IsEmpty) return false;
            return copySlots.ContainsKey(slot.Item.ID);
        }

        private void OnSlotEndDrag(GameObject gameObject, ItemSlotEx slot)
        {
            ItemSlotEx target = gameObject.GetComponentInParent<ItemSlotEx>();
            if (target && target != slot && !ContainsSlot(target))
            {
                TakeOut(slot);
            }
        }

        private void TakeOut(ItemSlotEx copy, bool all = false)
        {
            if (!copy.IsEmpty)
            {
                if (all || copy.Data.amount < 2)
                    RemoveSlot(copy);
                else
                {
                    AmountWindow.StartInput((amount) =>
                    {
                        if (copy.Data.amount < amount)
                            MessageManager.Instance.New("物品数量已改变，请重试");
                        else
                        {
                            copy.Data.amount -= (int)amount;
                            if (copy.Data.amount < 1) RemoveSlot(copy);
                            else copy.Refresh();
                        }
                    }, copy.Data.amount, "取出数量");
                }
            }

            void RemoveSlot(ItemSlotEx copy)
            {
                copySlots.Remove(copy.Item.ID);
                caches.Put(copy);
                selectedItems.Remove(copy.Item);
                SourceContainer.MarkIf(s => selectedItems.Contains(s.Item));
                if (copySlots.Count < 1) Utility.SetActive(tips, true);
            }
        }

        public bool IsSelectFor(ISlotContainer container)
        {
            if (container == null) return false;
            else return container.Equals(SourceContainer);
        }

        protected override bool OnOpen(params object[] args)
        {
            if (IsOpen) return true;
            if (args != null && args.Length > 8)
            {
                WindowsManager.CloseWindow<ItemWindow>();
                var par = (selectionType: (ItemSelectionType)args[0], container: (ISlotContainer)args[1], handler: (InventoryHandler)args[2], confirm: args[3] as Delegate,
                    title: args[4] as string, confirmDialog: args[5] as string, typeLimit: (int)args[6], amountLimit: (Func<ItemData, int>)args[7], selectCondition: args[8] as Predicate<ItemData>, cancel: args[9] as Action);
                SelectionType = par.selectionType;
                SourceContainer = par.container;
                SourceHandler = par.handler;
                windowTitle.text = !string.IsNullOrEmpty(par.title) ? par.title : Tr("选择道具");
                dialog = par.confirmDialog;
                typeLimit = par.typeLimit;
                amountLimit = par.amountLimit;
                if (par.selectCondition != null) canSelect = par.selectCondition;
                else canSelect = (a) => { return true; };
                SourceContainer.DarkIf(x => !canSelect(x.Item));
                onConfirm = par.confirm as Action<List<CountedItem>>;
                onConfirm_check = par.confirm as Func<List<CountedItem>, bool>;
                onCancel = par.cancel;
                confirm = false;
                RegisterNotify();
                return true;
            }
            return false;
        }

        protected override bool OnClose(params object[] args)
        {
            if (!IsOpen) return true;
            caches.Put(copySlots.Values);
            copySlots.Clear();
            selectedItems.Clear();
            SourceContainer.DarkIf(null);
            SourceContainer.MarkIf(null);
            SourceContainer = null;
            SourceHandler = null;
            Utility.SetActive(tips, true);
            dialog = string.Empty;
            SelectionType = ItemSelectionType.SelectNum;
            canSelect = null;
            onConfirm = null;
            if (!confirm) onCancel?.Invoke();
            onCancel = null;
            WindowsManager.CloseWindow<ItemWindow>();
            UnregisterNotify();
            return true;
        }

        protected override void RegisterNotify()
        {
            NotifyCenter.RemoveListener(this);
            if (SourceHandler != null)
            {
                NotifyCenter.AddListener(SourceHandler.ItemAmountChangedMsgKey, OnItemAmountChanged, this);
            }
        }
        private void OnItemAmountChanged(params object[] msg)
        {
            var (item, newAmount) = (msg[0] as ItemData, (int)msg[2]);
            if (item && copySlots.TryGetValue(item.ID, out var find))
            {
                if (newAmount < find.Data.amount)
                {
                    if (newAmount <= 0)
                    {
                        selectedItems.Remove(find.Item);
                        copySlots.Remove(find.Item.ID);
                        find.Vacate();
                        caches.Put(find);
                    }
                    else
                    {
                        find.Data.amount = newAmount;
                        find.Refresh();
                    }
                }
                else if (SelectionType == ItemSelectionType.SelectAll)
                {
                    find.Data.amount = newAmount;
                    find.Refresh();
                }
            }
        }
    }

    public enum ItemSelectionType
    {
        SelectNum,
        SelectAll
    }
}