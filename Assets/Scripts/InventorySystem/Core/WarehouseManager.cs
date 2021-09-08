using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/仓库管理器")]
public class WarehouseManager : WindowHandler<WarehouseUI, WarehouseManager>
{
    public Transform CellsParent { get { return UI.itemCellsParent; } }

    private readonly List<ItemSlot> itemAgents = new List<ItemSlot>();

    public WarehouseData CurrentData { get; private set; }

    public bool Managing { get; private set; }

    public bool IsInputFocused => UI ? IsUIOpen && UI.searchInput.isFocused : false;

    private new void Awake()
    {
        if (!UI || !UI.gameObject) return;
        base.Awake();
        for (int i = 0; i < 100; i++)
        {
            MakeSlot();
        }
    }


    public void Init()
    {
        if (CurrentData != null)
        {
            foreach (ItemSlot ia in itemAgents)
                ia.Empty();
            while (itemAgents.Count < CurrentData.size.Max)//格子不够用，新建
            {
                MakeSlot();
            }
            int originalSize = itemAgents.Count;
            for (int i = CurrentData.size.Max; i < originalSize - CurrentData.size.Max; i++)//用不到的格子隐藏
                itemAgents[i].Hide();
            for (int i = 0; i < CurrentData.size.Max; i++)//用得到的格子显示
                itemAgents[i].Show();
            foreach (ItemInfo info in CurrentData.Items)
            {
                if (info.indexInGrid > 0 && info.indexInGrid < itemAgents.Count)
                    itemAgents[info.indexInGrid].SetItem(info);
                else for (int i = 0; i < CurrentData.size.Max; i++)
                        if (itemAgents[i].IsEmpty)
                        {
                            itemAgents[i].SetItem(info);
                            break;
                        }
            }
            UpdateUI();
        }
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
    }


    public void Search()
    {
        if (!UI || !UI.gameObject || !UI.searchInput || !UI.searchButton) return;
        if (string.IsNullOrEmpty(UI.searchInput.text))
        {
            SetPage(currentPage);
            return;
        }
        foreach (var ia in itemAgents)
        {
            if (!ia.IsEmpty && ia.MItemInfo.ItemName.Contains(UI.searchInput.text)) continue;
            else ia.Hide();
        }
        UI.searchInput.text = string.Empty;
    }

    #region 道具处理相关
    public void StoreItem(ItemInfo info, bool all = false)
    {
        if (CurrentData == null || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseWindow();
        if (!all)
        {
            if (info.Amount == 1 && OnStore(info, 1))
                MessageManager.Instance.New(string.Format("存入了1个 [{0}]", info.ItemName));
            else
            {
                AmountManager.Instance.New(delegate (long amount)
                {
                    if (OnStore(info, (int)amount))
                        MessageManager.Instance.New(string.Format("存入了{0}个 [{1}]", (int)amount, info.ItemName));
                }, info.Amount, "存入数量", ZetanUtility.ScreenCenter, Vector2.zero);
            }
        }
        else
        {
            int amountBef = GetItemAmount(info.ItemID);
            if (OnStore(info, info.Amount))
                MessageManager.Instance.New(string.Format("存入了{0}个 [{1}]", GetItemAmount(info.ItemID) - amountBef, info.ItemName));
        }
    }

    private bool OnStore(ItemInfo info, int amount)
    {
        if (CurrentData == null || info == null || !info.item || amount < 1) return false;
        int finalGet = info.Amount < amount ? info.Amount : amount;
        return GetItem(info, finalGet);
    }

    private bool GetItem(ItemInfo info, int amount)
    {
        if (CurrentData == null || info == null || !info.item || amount < 1) return false;
        if (!BackpackManager.Instance.TryLoseItem_Boolean(info, amount)) return false;
        if (!info.item.StackAble && CurrentData.IsFull)
        {
            MessageManager.Instance.New("仓库已满");
            return false;
        }
        if (!info.item.StackAble && amount > CurrentData.size.Rest)
        {
            MessageManager.Instance.New(string.Format("请至少多留出{0}个仓库空间", amount - CurrentData.size.Rest));
            return false;
        }
        if (info.item.StackAble)
        {
            CurrentData.GetItemSimple(info, amount);
            ItemSlot ia = itemAgents.Find(x => !x.IsEmpty && x.MItemInfo.item == info.item);
            if (ia) ia.UpdateInfo();
            else
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.SetItem(CurrentData.Latest);
                else
                {
                    MessageManager.Instance.New("发生内部错误！");
                    Debug.Log("[Store Item Error: Can't find ItemAgent] ID: " + info.ItemID + "[" + DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                CurrentData.GetItemSimple(info);
                foreach (ItemSlot ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.SetItem(CurrentData.Latest);
                        break;
                    }
            }
        BackpackManager.Instance.LoseItem(info, amount);
        UpdateUI();
        return true;
    }

    public void TakeOutItem(ItemInfo info, bool all = false)
    {
        if (CurrentData == null || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseWindow();
        if (!all)
            if (info.Amount == 1 && OnTakeOut(info, 1))
                MessageManager.Instance.New(string.Format("取出了1个 [{0}]", info.ItemName));
            else
            {
                AmountManager.Instance.New(delegate (long amount)
                {
                    if (OnTakeOut(info, (int)amount))
                        MessageManager.Instance.New(string.Format("取出了{0}个 [{1}]", (int)amount, info.ItemName));
                }, info.Amount,"取出数量",ZetanUtility.ScreenCenter, Vector2.zero);
            }
        else
        {
            int amountBef = GetItemAmount(info.ItemID);
            if (OnTakeOut(info, info.Amount))
                MessageManager.Instance.New(string.Format("取出了{0}个 [{1}]", amountBef - GetItemAmount(info.ItemID), info.ItemName));
        }
    }

    private bool OnTakeOut(ItemInfo info, int amount)
    {
        if (CurrentData == null || info == null || !info.item || amount < 1) return false;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        return LoseItem(info, finalLose);
    }

    private bool LoseItem(ItemInfo info, int amount)
    {
        if (CurrentData == null || info == null || !info.item || amount < 1) return false;
        if (!BackpackManager.Instance.TryGetItem_Boolean(info, amount)) return false;
        BackpackManager.Instance.GetItem(info, amount);
        CurrentData.LoseItemSimple(info, amount);
        ItemSlot ia = GetItemAgentByInfo(info);
        if (ia) ia.UpdateInfo();
        UpdateUI();
        if (!BackpackManager.Instance.IsUIOpen)
            BackpackManager.Instance.OpenWindow();
        return true;
    }

    public int GetItemAmount(string id)
    {
        var items = CurrentData.Items.FindAll(x => x.ItemID == id);
        if (items.Count < 1) return 0;
        if (items[0].item.StackAble) return items[0].Amount;
        return items.Count;
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmount(id) > 0;
    }

    public ItemSlot GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.MItemInfo == info);
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        Init();
        Managing = true;
        BackpackManager.Instance.OpenWindow();
        UIManager.Instance.EnableJoyStick(false);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        if (CurrentData && CurrentData.entity) CurrentData.entity.OnDoneManage();
        CurrentData = null;
        Managing = false;
        foreach (ItemSlot ia in itemAgents)
        {
            ia.FinishDrag();
            ia.Empty();
            ia.Hide();
        }
        if (BackpackManager.Instance.IsUIOpen) BackpackManager.Instance.CloseWindow();
        ItemWindowManager.Instance.CloseWindow();
        if (DialogueManager.Instance.IsUIOpen) DialogueManager.Instance.PauseDisplay(false);
        AmountManager.Instance.Cancel();
        UIManager.Instance.EnableJoyStick(true);
    }

    public void UpdateUI()
    {
        UI.money.text = CurrentData.Money.ToString();
        UI.size.text = CurrentData.size.ToString();
        SetPage(currentPage);
    }

    private void MakeSlot()
    {
        ItemSlot ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemSlot>();
        ia.Init(itemAgents.Count - 1, UI.gridRect, GetHandleButtons, delegate (ItemSlot slot) { TakeOutItem(slot.MItemInfo, true); });
        ia.Empty();
        ia.Hide();
        itemAgents.Add(ia);
    }
    private ButtonWithTextData[] GetHandleButtons(ItemSlot slot)
    {
        if (!slot || slot.IsEmpty) return null;

        List<ButtonWithTextData> buttons = new List<ButtonWithTextData>
        {
            new ButtonWithTextData("取出", delegate
            {
                TakeOutItem(slot.MItemInfo);
            }),
        };
        if (slot.MItemInfo.Amount > 1)
            buttons.Add(new ButtonWithTextData("全部取出", delegate
            {
                TakeOutItem(slot.MItemInfo, true);
            }));
        return buttons.ToArray();
    }

    public void Arrange()
    {
        CurrentData.Arrange();
        foreach (ItemSlot ia in itemAgents)
            ia.Empty();
        for (int i = 0; i < CurrentData.Items.Count; i++)
            itemAgents[i].SetItem(CurrentData.Items[i]);
        UpdateUI();
    }

    public override void SetUI(WarehouseUI UI)
    {
        itemAgents.RemoveAll(x => !x || !x.gameObject);
        foreach (var ia in itemAgents)
        {
            ia.Empty();
        }
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion

    public bool Manage(WarehouseData data)
    {
        if (IsUIOpen || !data || CurrentData == data)
            return false;

        CurrentData = data;
        OpenWindow();
        return true;
    }

    public void CancelManage()
    {
        IsPausing = false;
        CloseWindow();
        ItemWindowManager.Instance.CloseWindow();
    }

    #region 道具页相关
    private int currentPage;
    public void SetPage(int index)
    {
        if (!UI || !UI.gameObject) return;
        currentPage = index;
        switch (index)
        {
            case 1: ShowEquipments(); break;
            case 2: ShowConsumables(); break;
            case 3: ShowMaterials(); break;
            default: ShowAll(); break;
        }
    }

    private void ShowAll()
    {
        if (!UI || !UI.gameObject || !CurrentData) return;
        for (int i = 0; i < CurrentData.size.Max; i++)
        {
            ZetanUtility.SetActive(itemAgents[i].gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        if (!UI || !UI.gameObject || !CurrentData) return;
        for (int i = 0; i < CurrentData.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsEquipment)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        if (!UI || !UI.gameObject || !CurrentData) return;
        for (int i = 0; i < CurrentData.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsConsumable)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        if (!UI || !UI.gameObject || !CurrentData) return;
        for (int i = 0; i < CurrentData.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsMaterial)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }
    #endregion
}
