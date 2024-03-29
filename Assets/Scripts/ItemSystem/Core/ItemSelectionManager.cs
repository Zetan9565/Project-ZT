﻿//using System;
//using System.Collections.Generic;
//using UnityEngine;
////using UnityEngine.UI;

//[DisallowMultipleComponent]
//public class ItemSelectionManager : WindowHandler<ItemSelectionUI, ItemSelectionManager>
//{
//    public bool IsSelecting { get; private set; }

//    public GameObject PlacementArea => UI ? UI.placementArea : null;

//    private readonly List<ItemSlotCopy> itemSlots = new List<ItemSlotCopy>();
//    private readonly HashSet<ItemSlotAgent> slotsMap = new HashSet<ItemSlotAgent>();

//    public ItemSelectionType SelectionType { get; private set; }

//    private int typeLimit;
//    private int amountLimit;
//    private Func<ItemInfo, bool> canSelect;
//    private Action<IEnumerable<ItemSelectionData>> onConfirm;
//    private Action onCancel;

//    private string dialog;

//    private bool bagPausingBef;
//    private bool bagOpenBef;

//    public void StartSelection(ItemSelectionType selectionType, string title, string confirmDialog, int typeLimit, int amountLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        if (IsUIOpen) CloseWindow();
//        SelectionType = selectionType;
//        UI.windowTitle.text = title;
//        dialog = confirmDialog;
//        this.typeLimit = typeLimit;
//        this.amountLimit = amountLimit;
//        if (selectCondition != null) canSelect = selectCondition;
//        else canSelect = delegate (ItemInfo _) { return true; };
//        onConfirm = confirm;
//        onCancel = cancel;
//        OpenWindow();
//    }
//    public void StartSelection(ItemSelectionType selectionType, string title, string confirmDialog, int typeLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, title, confirmDialog, typeLimit, -1, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, string title, string confirmDialog, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, title, confirmDialog, -1, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, string title, int typeLimit, int amountLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, title, string.Empty, typeLimit, amountLimit, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, string title, int typeLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, title, string.Empty, typeLimit, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, string title, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, title, string.Empty, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, int typeLimit, int amountLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, "选择物品", typeLimit, amountLimit, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, int typeLimit, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, "选择物品", typeLimit, selectCondition, confirm, cancel);
//    }
//    public void StartSelection(ItemSelectionType selectionType, Func<ItemInfo, bool> selectCondition, Action<IEnumerable<ItemSelectionData>> confirm, Action cancel = null)
//    {
//        StartSelection(selectionType, "选择物品", selectCondition, confirm, cancel);
//    }

//    public void Confirm()
//    {
//        if (itemSlots.Count < 1)
//        {
//            MessageManager.Instance.New("未选择任何道具");
//            return;
//        }
//        List<ItemSelectionData> infos = new List<ItemSelectionData>();
//        foreach (var ia in itemSlots)
//        {
//            infos.Add(new ItemSelectionData(ia.source.Info, ia.current.Info.Amount));
//        }
//        if (string.IsNullOrEmpty(dialog))
//        {
//            onConfirm?.Invoke(infos);
//            CloseWindow();
//        }
//        else ConfirmWindow.StartConfirm(dialog, delegate
//        {
//            onConfirm?.Invoke(infos);
//            CloseWindow();
//        });
//    }

//    public void Clear()
//    {
//        foreach (var ia in itemSlots)
//        {
//            ia.current.Recycle();
//            ia.source.Mark(false);
//        }
//        itemSlots.Clear();
//        ZetanUtility.SetActive(UI.tips, true);
//        NewWindowsManager.CloseWindow<ItemWindow>();
//    }

//    public void Place(ItemSlotAgent source)
//    {
//        if (!source) return;
//        var info = source.Info;
//        if (info == null || info.item == null || info.Amount < 0) return;
//        if (info.item.StackAble && SelectionType == ItemSelectionType.SelectNum)
//        {
//            if (!canSelect(info)) return;
//            if (itemSlots.Exists(x => x.source.Info == info || x.source.Info.item == info.item))
//            {
//                MessageManager.Instance.New("已选择该道具");
//                return;
//            }
//            if (typeLimit > 0 && itemSlots.Count >= typeLimit)
//            {
//                MessageManager.Instance.New($"每次最多只能选择{typeLimit}样道具");
//                return;
//            }
//            if (info.Amount < 2)
//            {
//                if (BackpackManager.Instance.TryLoseItem_Boolean(info))
//                    MakeSlot(info);
//            }
//            else if (amountLimit == 1)
//            {
//                if (BackpackManager.Instance.TryLoseItem_Boolean(info, 1))
//                    MakeSlot(new ItemInfo(info.item));
//            }
//            else
//            {
//                AmountWindow.StartInput(delegate (long amount)
//                {
//                    if (BackpackManager.Instance.TryLoseItem_Boolean(info, (int)amount))
//                    {
//                        ItemSlotCopy ia = itemSlots.Find(x => x.current.Info.item == info.item);
//                        if (ia != null) ia.current.Info.Amount = (int)amount;
//                        else
//                        {
//                            MakeSlot(new ItemInfo(info.item, (int)amount));
//                        }
//                        if (itemSlots.Count > 0) ZetanUtility.SetActive(UI.tips, false);
//                    }
//                }, amountLimit > 0 ? amountLimit : info.Amount);
//            }
//        }
//        else if ((!info.item.StackAble && SelectionType == ItemSelectionType.SelectNum || SelectionType == ItemSelectionType.SelectAll)
//            && canSelect(info) && BackpackManager.Instance.TryLoseItem_Boolean(info))
//        {
//            if (itemSlots.Exists(x => x.source.Info == info))
//            {
//                MessageManager.Instance.New("已选择该道具");
//                return;
//            }
//            if (typeLimit > 0 && itemSlots.Count >= typeLimit)
//            {
//                MessageManager.Instance.New($"每次最多只能选择{typeLimit}样道具");
//            }
//            else MakeSlot(info);
//        }

//        void MakeSlot(ItemInfo info)
//        {
//            ItemSlotAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemSlotAgent>();
//            ia.Init(-1, UI.gridScrollRect, GetHandleButtons, delegate (ItemSlotAgent slot) { TakeOut(slot, true); }, OnSlotEndDrag);
//            itemSlots.Add(new ItemSlotCopy(ia, source));
//            slotsMap.Add(ia);
//            ia.SetItem(info);
//            source.Mark();
//            if (itemSlots.Count > 0) ZetanUtility.SetActive(UI.tips, false);
//        }
//    }

//    private ButtonWithTextData[] GetHandleButtons(ItemSlotAgent slot)
//    {
//        if (!slot || slot.IsEmpty) return null;

//        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>();
//        if (SelectionType == ItemSelectionType.SelectNum)
//            buttons.Add(new ButtonWithTextData("取出", delegate
//            {
//                TakeOut(slot);
//            }));
//        if (slot.Info.Amount > 1 || SelectionType == ItemSelectionType.SelectAll)
//            buttons.Add(new ButtonWithTextData("全部取出", delegate
//            {
//                TakeOut(slot, true);
//            }));
//        return buttons.ToArray();
//    }

//    public bool ContainsSlot(ItemSlotAgent slot)
//    {
//        return slotsMap.Contains(slot);
//    }

//    private void OnSlotEndDrag(GameObject gameObject, ItemSlotAgent slot)
//    {
//        ItemSlotAgent target = gameObject.GetComponentInParent<ItemSlotAgent>();
//        if (target && target != slot && !ContainsSlot(target))
//        {
//            TakeOut(slot);
//        }
//    }

//    private void TakeOut(ItemSlotAgent slot, bool all = false)
//    {
//        var find = itemSlots.Find(x => x.current == slot);
//        if (find != null)
//        {
//            if (all || slot.Info.Amount < 2)
//                Remove(find);
//            else
//            {
//                AmountWindow.StartInput(delegate (long amount)
//                {
//                    find.current.Info.Amount -= (int)amount;
//                    if (find.current.Info.Amount < 1)
//                        Remove(find);
//                    else find.current.UpdateInfo();
//                }, find.current.Info.Amount, "取出数量");
//            }
//        }

//        void Remove(ItemSlotCopy find)
//        {
//            find.current.Recycle();
//            find.source.Mark(false);
//            itemSlots.Remove(find);
//            slotsMap.Remove(find.current);
//            if (itemSlots.Count < 1) ZetanUtility.SetActive(UI.tips, true);
//        }
//    }

//    public override void OpenWindow()
//    {
//        base.OpenWindow();
//        if (!IsUIOpen) return;
//        NewWindowsManager.CloseWindow<ItemWindow>();
//        bagPausingBef = BackpackManager.Instance.IsPausing;
//        if (bagPausingBef) BackpackManager.Instance.PauseDisplay(false);
//        bagOpenBef = BackpackManager.Instance.IsUIOpen;
//        if (!BackpackManager.Instance.IsUIOpen) BackpackManager.Instance.OpenWindow();
//        BackpackManager.Instance.PartSelectable(true, canSelect);
//    }

//    public override void CloseWindow()
//    {
//        base.CloseWindow();
//        if (IsUIOpen) return;
//        foreach (var ia in itemSlots)
//        {
//            ia.current.Recycle();
//            ia.source.Mark(false);
//        }
//        itemSlots.Clear();
//        ZetanUtility.SetActive(UI.tips, true);
//        dialog = string.Empty;
//        SelectionType = ItemSelectionType.SelectNum;
//        onCancel?.Invoke();
//        BackpackManager.Instance.PartSelectable(false);
//        if (bagPausingBef)
//            BackpackManager.Instance.PauseDisplay(true);
//        else if (!bagOpenBef)
//        {
//            if (BackpackManager.Instance.IsPausing)
//                BackpackManager.Instance.PauseDisplay(false);
//            BackpackManager.Instance.CloseWindow();
//        }
//        bagPausingBef = false;
//        bagOpenBef = false;
//        canSelect = null;
//        NewWindowsManager.CloseWindow<ItemWindow>();
//    }

//    //private class ItemSelectionUI
//    //{
//    //    public CanvasGroup window;

//    //    public Canvas windowCanvas;

//    //    public Button closeButton;

//    //    public Text windowTitle;
//    //    public GameObject tips;

//    //    public GameObject itemCellPrefab;
//    //    public Transform itemCellsParent;

//    //    public ScrollRect gridScrollRect;
//    //    public GameObject placementArea;

//    //    public Button confirmButton;
//    //    public Button clearButton;

//    //    public void Init(ModuleWindowUI UI)
//    //    {
//    //        window = UI.GetElement("window").GetComponent<CanvasGroup>();
//    //        closeButton = UI.GetButton("closeButton");

//    //        windowTitle = UI.GetText("windowTitle");
//    //        tips = UI.GetElement("tips");

//    //        itemCellPrefab = UI.GetElement("itemCellPrefab");
//    //        itemCellsParent = UI.GetRectTranstrom("itemCellsParent");

//    //        gridScrollRect = UI.GetElement("gridScrollRect").GetComponent<ScrollRect>();
//    //        placementArea = UI.GetElement("placementArea");

//    //        confirmButton = UI.GetButton("confirmButton");
//    //        clearButton = UI.GetButton("clearButton");

//    //        closeButton.onClick.AddListener(Instance.CloseWindow);
//    //        confirmButton.onClick.AddListener(Instance.Confirm);
//    //        clearButton.onClick.AddListener(Instance.Clear);

//    //        if (!window.gameObject.GetComponent<GraphicRaycaster>())
//    //            window.gameObject.AddComponent<GraphicRaycaster>();
//    //        windowCanvas = window.GetComponent<Canvas>();
//    //        windowCanvas.overrideSorting = true;
//    //        windowCanvas.sortingLayerID = SortingLayer.NameToID("UI");
//    //    }

//    //    public static implicit operator bool(ItemSelectionUI obj)
//    //    {
//    //        return obj != null;
//    //    }
//    //}
//}
public class ItemSelectionData
{
    public readonly ItemInfo source;
    public int amount;

    public bool IsValid => source && amount > 0;

    public ItemSelectionData(ItemInfo source, int amount)
    {
        this.source = source;
        this.amount = amount;
    }

    public static implicit operator bool(ItemSelectionData obj)
    {
        return obj != null;
    }
}