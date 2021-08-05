using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemSelectionManager : WindowHandler<ItemSelectionUI, ItemSelectionManager>
{
    public bool IsSelecting { get; private set; }

    public GameObject PlacementArea => UI ? UI.placementArea : null;

    private readonly List<ItemSlotCopy> itemSlots = new List<ItemSlotCopy>();
    private readonly HashSet<ItemSlot> slotsMap = new HashSet<ItemSlot>();

    public ItemSelectionType SelectionType { get; private set; }

    private Func<ItemInfo, bool> canSelect;
    private Action<IEnumerable<ItemInfo>> onConfirm;
    private Action onCancel;

    private string dialog;

    private bool bagPausingBef;
    private bool bagOpenBef;

    public void StartSelection(ItemSelectionType selectionType, string title, string confirmDialog, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemInfo>> confirm, Action cancel = null)
    {
        if (IsUIOpen) CloseWindow();
        SelectionType = selectionType;
        UI.windowTitle.text = title;
        dialog = confirmDialog;
        if (selectCondition != null) canSelect = selectCondition;
        else canSelect = delegate (ItemInfo _) { return true; };
        onConfirm = confirm;
        onCancel = cancel;
        OpenWindow();
    }

    public void StartSelection(ItemSelectionType selectionType, string title, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemInfo>> confirm, Action cancel = null)
    {
        StartSelection(selectionType, title, string.Empty, selectCondition, confirm, cancel);
    }

    public void StartSelection(ItemSelectionType selectionType, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemInfo>> confirm, Action cancel = null)
    {
        StartSelection(selectionType, "选择物品", selectCondition, confirm, cancel);
    }

    public void Confirm()
    {
        if (itemSlots.Count < 1)
        {
            MessageManager.Instance.New("未选择任何道具");
            return;
        }
        List<ItemInfo> infos = new List<ItemInfo>();
        foreach (var ia in itemSlots)
        {
            infos.Add(ia.current.MItemInfo);
        }
        if (string.IsNullOrEmpty(dialog))
        {
            onConfirm?.Invoke(infos);
            CloseWindow();
        }
        else ConfirmManager.Instance.New(dialog, delegate
        {
            onConfirm?.Invoke(itemSlots.Select(x => x.source.MItemInfo.item.StackAble ? x.current.MItemInfo : x.source.MItemInfo));
            CloseWindow();
        });
    }

    public void Clear()
    {
        foreach (var ia in itemSlots)
        {
            ia.current.Recycle();
            ia.source.Mark(false);
        }
        itemSlots.Clear();
        ZetanUtility.SetActive(UI.tips, true);
        ItemWindowManager.Instance.CloseWindow();
    }

    public bool Place(ItemSlot source)
    {
        if (!source) return false;
        var info = source.MItemInfo;
        if (info == null || info.item == null || info.Amount < 0) return false;
        if (info.item.StackAble && SelectionType == ItemSelectionType.SelectNum)
        {
            if (!canSelect(info)) return false;
            if (itemSlots.Exists(x => x.source.MItemInfo == info || x.source.MItemInfo.item == info.item))
            {
                MessageManager.Instance.New("已选择该道具");
                return false;
            }
            if (info.Amount < 2)
            {
                if (BackpackManager.Instance.TryLoseItem_Boolean(info))
                {
                    MakeSlot(info);
                    return true;
                }
            }
            else
            {
                AmountManager.Instance.New(delegate (long amount)
                {
                    if (BackpackManager.Instance.TryLoseItem_Boolean(info, (int)amount))
                    {
                        ItemSlotCopy ia = itemSlots.Find(x => x.current.MItemInfo.item == info.item);
                        if (ia != null) ia.current.MItemInfo.Amount = (int)amount;
                        else
                        {
                            MakeSlot(new ItemInfo(info.item, (int)amount));
                        }
                        if (itemSlots.Count > 0) ZetanUtility.SetActive(UI.tips, false);
                    }
                }, info.Amount);
                return true;
            }
        }
        else if ((!info.item.StackAble && SelectionType == ItemSelectionType.SelectNum || SelectionType == ItemSelectionType.SelectAll)
            && canSelect(info) && BackpackManager.Instance.TryLoseItem_Boolean(info))
        {
            if (itemSlots.Exists(x => x.source.MItemInfo == info))
            {
                MessageManager.Instance.New("已选择该道具");
                return false;
            }
            MakeSlot(info);
            return true;
        }
        return false;

        void MakeSlot(ItemInfo info)
        {
            ItemSlot ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemSlot>();
            ia.Init(-1, UI.gridScrollRect, GetHandleButtons, delegate (ItemSlot slot) { TakeOut(slot, true); }, OnSlotEndDrag);
            itemSlots.Add(new ItemSlotCopy(ia, source));
            slotsMap.Add(ia);
            ia.SetItem(info);
            if (itemSlots.Count > 0) ZetanUtility.SetActive(UI.tips, false);
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
        if (slot.MItemInfo.Amount > 1 || SelectionType == ItemSelectionType.SelectAll)
            buttons.Add(new ButtonWithTextData("全部取出", delegate
            {
                TakeOut(slot, true);
            }));
        return buttons.ToArray();
    }

    public bool ContainsSlot(ItemSlot slot)
    {
        return slotsMap.Contains(slot);
    }

    private void OnSlotEndDrag(GameObject gameObject, ItemSlot slot)
    {
        ItemSlot target = gameObject.GetComponentInParent<ItemSlot>();
        if (target && target != slot && !ContainsSlot(target))
        {
            TakeOut(slot);
        }
    }

    private void TakeOut(ItemSlot slot, bool all = false)
    {
        var find = itemSlots.Find(x => x.current == slot);
        if (find != null)
        {
            if (all)
                Remove(find);
            else
            {
                AmountManager.Instance.New(delegate (long amount)
                {
                    find.current.MItemInfo.Amount -= (int)amount;
                    if (find.current.MItemInfo.Amount < 1)
                        Remove(find);
                    else find.current.UpdateInfo();
                }, find.current.MItemInfo.Amount, "取出数量");
            }
        }

        void Remove(ItemSlotCopy find)
        {
            find.current.Recycle();
            find.source.Mark(false);
            itemSlots.Remove(find);
            slotsMap.Remove(find.current);
            if (itemSlots.Count < 1) ZetanUtility.SetActive(UI.tips, true);
        }
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        if (ItemWindowManager.Instance.IsUIOpen) ItemWindowManager.Instance.CloseWindow();
        bagPausingBef = BackpackManager.Instance.IsPausing;
        bagOpenBef = BackpackManager.Instance.IsUIOpen;
        if (!BackpackManager.Instance.IsUIOpen)
        {
            if (bagPausingBef)
                BackpackManager.Instance.PauseDisplay(false);
            BackpackManager.Instance.OpenWindow();
        }
        BackpackManager.Instance.PartSelectable(true, canSelect);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        foreach (var ia in itemSlots)
        {
            ia.current.Recycle();
            ia.source.Mark(false);
        }
        itemSlots.Clear();
        ZetanUtility.SetActive(UI.tips, true);
        dialog = string.Empty;
        SelectionType = ItemSelectionType.SelectNum;
        onCancel?.Invoke();
        BackpackManager.Instance.PartSelectable(false);
        if (bagPausingBef)
            BackpackManager.Instance.PauseDisplay(true);
        else if (!bagOpenBef)
        {
            if (BackpackManager.Instance.IsPausing)
                BackpackManager.Instance.PauseDisplay(false);
            BackpackManager.Instance.CloseWindow();
        }
        bagPausingBef = false;
        bagOpenBef = false;
        canSelect = null;
    }
}
public class ItemSlotCopy
{
    public readonly ItemSlot current;
    public readonly ItemSlot source;

    public ItemSlotCopy(ItemSlot current, ItemSlot source)
    {
        this.current = current;
        this.source = source;
    }
}

public enum ItemSelectionType
{
    SelectNum,
    SelectAll
}