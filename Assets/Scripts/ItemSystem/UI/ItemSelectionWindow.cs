using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZetanExtends;

[DisallowMultipleComponent]
public class ItemSelectionWindow : Window
{
    public Text windowTitle;
    public GameObject tips;

    public ItemSlot itemCellPrefab;
    public RectTransform itemCellsParent;

    public ScrollRect gridScrollRect;
    public GameObject placementArea;

    public Button confirmButton;
    public Button clearButton;

    public ISlotContainer SourceContainer { get; private set; }
    public IInventoryHandler SourceHandler { get; private set; }

    private bool confirm;

    private SimplePool<ItemSlot> caches;

    protected override void OnAwake()
    {
        confirmButton.onClick.AddListener(Confirm);
        clearButton.onClick.AddListener(Clear);
        var pool = itemCellsParent.FindOrCreate("Caches");
        ZetanUtility.SetActive(pool, false);
        caches = new SimplePool<ItemSlot>(itemCellPrefab, poolRoot: pool);
    }

    public bool IsSelecting { get; private set; }

    public GameObject PlacementArea => placementArea;

    private readonly Dictionary<string, ItemSlot> copySlots = new Dictionary<string, ItemSlot>();
    private readonly HashSet<ItemData> selectedItems = new HashSet<ItemData>();

    public ItemSelectionType SelectionType { get; private set; }

    private int typeLimit;
    private int amountLimit;
    private Predicate<ItemSlotBase> canSelect;
    private Action<List<ItemWithAmount>> onConfirm;
    private Action onCancel;

    private string dialog;

    public static ItemSelectionWindow StartSelection(ItemSelectionType selectionType, ISlotContainer container, IInventoryHandler handler, Action<List<ItemWithAmount>> confirm, string title = null, string confirmDialog = null,
        int? typeLimit = null, int? amountLimit = null, Predicate<ItemSlotBase> selectCondition = null, Action cancel = null)
    {
        return WindowsManager.OpenWindow<ItemSelectionWindow>(selectionType, container, handler, confirm, title, confirmDialog, typeLimit ?? default, amountLimit ?? default, selectCondition, cancel);
    }

    public void Confirm()
    {
        if (copySlots.Count < 1)
        {
            MessageManager.Instance.New("未选择任何物品");
            return;
        }
        List<ItemWithAmount> items = new List<ItemWithAmount>();
        foreach (var kvp in copySlots)
        {
            items.Add(new ItemWithAmount(kvp.Value.Item, kvp.Value.Data.amount));
        }
        confirm = true;
        if (string.IsNullOrEmpty(dialog))
        {
            onConfirm?.Invoke(items);
            Close();
        }
        else ConfirmWindow.StartConfirm(dialog, delegate
        {
            onConfirm?.Invoke(items);
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
        ZetanUtility.SetActive(tips, true);
        WindowsManager.CloseWindow<ItemWindow>();
    }

    public void Place(ItemSlot source)
    {
        if (!source || source.IsEmpty || source.Data.IsEmpty) return;
        var slot = source.Data;
        int have = SourceHandler.GetAmount(slot.item);
        if (slot == null || slot.Model == null || have < 0) return;
        if (slot.Model.StackAble && SelectionType == ItemSelectionType.SelectNum)
        {
            if (!canSelect(source)) return;
            if (typeLimit > 0 && copySlots.Count >= typeLimit)
            {
                MessageManager.Instance.New($"每次最多只能选择{typeLimit}样物品");
                return;
            }
            if (have < 2)
            {
                if (SourceHandler.CanLose(slot.item, 1))
                    MakeSlot(slot.item, 1);
            }
            else if (amountLimit == 1)
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
                        if (copySlots.Count > 0) ZetanUtility.SetActive(tips, false);
                    }
                }, amountLimit > 0 ? amountLimit : have);
            }
        }
        else if ((!slot.Model.StackAble && SelectionType == ItemSelectionType.SelectNum || SelectionType == ItemSelectionType.SelectAll)
            && canSelect(source) && SourceHandler.CanLose(slot.item, have))
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
            ItemSlot copy = caches.Get(itemCellsParent);
            copy.SetScrollRect(gridScrollRect);
            copy.SetCallbacks(GetHandleButtons, (s) => TakeOut(s, true), OnSlotEndDrag);
            copySlots.Add(data.ID, copy);
            selectedItems.Add(data);
            copy.Refresh(new ItemSlotData(data, amount));
            SourceContainer.MarkIf(s => selectedItems.Contains(s.Item));
            if (copySlots.Count > 0) ZetanUtility.SetActive(tips, false);
        }
    }

    private ButtonWithTextData[] GetHandleButtons(ItemSlot slot)
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

    public bool ContainsSlot(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return false;
        return copySlots.ContainsKey(slot.Item.ID);
    }

    private void OnSlotEndDrag(GameObject gameObject, ItemSlot slot)
    {
        ItemSlot target = gameObject.GetComponentInParent<ItemSlot>();
        if (target && target != slot && !ContainsSlot(target))
        {
            TakeOut(slot);
        }
    }

    private void TakeOut(ItemSlot copy, bool all = false)
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

        void RemoveSlot(ItemSlot copy)
        {
            copySlots.Remove(copy.Item.ID);
            caches.Put(copy);
            selectedItems.Remove(copy.Item);
            SourceContainer.MarkIf(s => selectedItems.Contains(s.Item));
            if (copySlots.Count < 1) ZetanUtility.SetActive(tips, true);
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
            var par = (selectionType: (ItemSelectionType)args[0], container: (ISlotContainer)args[1], handler: (IInventoryHandler)args[2], confirm: args[3] as Action<List<ItemWithAmount>>,
                title: args[4] as string, confirmDialog: args[5] as string, typeLimit: (int)args[6], amountLimit: (int)args[7], selectCondition: args[8] as Predicate<ItemSlotBase>, cancel: args[9] as Action);
            SelectionType = par.selectionType;
            SourceContainer = par.container;
            SourceHandler = par.handler;
            windowTitle.text = par.title;
            dialog = par.confirmDialog;
            typeLimit = par.typeLimit;
            amountLimit = par.amountLimit;
            if (par.selectCondition != null) canSelect = par.selectCondition;
            else canSelect = (a) => { return true; };
            SourceContainer.DarkIf(x => !canSelect(x));
            onConfirm = par.confirm;
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
        foreach (var kvp in copySlots)
        {
            caches.Put(kvp.Value);
        }
        copySlots.Clear();
        selectedItems.Clear();
        SourceContainer.DarkIf(null);
        SourceContainer.MarkIf(null);
        SourceContainer = null;
        SourceHandler = null;
        ZetanUtility.SetActive(tips, true);
        dialog = string.Empty;
        SelectionType = ItemSelectionType.SelectNum;
        canSelect = null;
        onConfirm = null;
        if (!confirm) onCancel?.Invoke();
        onCancel = null;
        WindowsManager.CloseWindow<ItemWindow>();
        return true;
    }

    protected override void OnStart()
    {
        //屏蔽基类方法，事件注册见机行事
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