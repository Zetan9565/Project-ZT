using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/仓库管理器")]
public class WarehouseManager : WindowHandler<WarehouseUI, WarehouseManager>
{
    public Transform CellsParent { get { return UI.itemCellsParent; } }

    private readonly List<ItemAgent> itemAgents = new List<ItemAgent>();

    public Warehouse MWarehouse { get; private set; }

    public bool StoreAble { get; private set; }

    private void Awake()
    {
        if (!UI || !UI.gameObject) return;
        for (int i = 0; i < 100; i++)
        {
            ItemAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            itemAgents.Add(ia);
            ia.Init(ItemAgentType.Warehouse, itemAgents.Count - 1, UI.gridRect);
            ia.Empty();
            ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    public void Init(Warehouse warehouse)
    {
        if (warehouse != null)
        {
            MWarehouse = warehouse;
            foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            while (itemAgents.Count < MWarehouse.size.Max)//格子不够用，新建
            {
                ItemAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                itemAgents.Add(ia);
                ia.Empty();
                ia.Init(ItemAgentType.Warehouse, itemAgents.Count - 1, UI.gridRect);
            }
            int originalSize = itemAgents.Count;
            for (int i = MWarehouse.size.Max; i < originalSize - MWarehouse.size.Max; i++)//用不到的格子隐藏
                itemAgents[i].Hide();
            for (int i = 0; i < MWarehouse.size.Max; i++)//用得到的格子显示
                itemAgents[i].Show();
            foreach (ItemInfo info in MWarehouse.Items)
            {
                if (info.indexInGrid > 0 && info.indexInGrid < itemAgents.Count)
                    itemAgents[info.indexInGrid].SetItem(info);
                else for (int i = 0; i < MWarehouse.size.Max; i++)
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

    #region 道具处理相关
    public void StoreItem(ItemInfo info, bool all = false)
    {
        if (MWarehouse == null || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseWindow();
        if (!all)
        {
            if (info.Amount == 1 && OnStore(info, 1))
                MessageManager.Instance.New(string.Format("存入了1个 [{0}]", info.ItemName));
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
                AmountManager.Instance.New(delegate
                {
                    if (OnStore(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("存入了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.ItemName));
                }, info.Amount);
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
        if (MWarehouse == null || info == null || !info.item || amount < 1) return false;
        int finalGet = info.Amount < amount ? info.Amount : amount;
        return GetItem(info, finalGet);
    }

    private bool GetItem(ItemInfo info, int amount)
    {
        if (MWarehouse == null || info == null || !info.item || amount < 1) return false;
        if (!BackpackManager.Instance.TryLoseItem_Boolean(info, amount)) return false;
        if (!info.item.StackAble && MWarehouse.IsFull)
        {
            MessageManager.Instance.New("仓库已满");
            return false;
        }
        if (!info.item.StackAble && amount > MWarehouse.size.Rest)
        {
            MessageManager.Instance.New(string.Format("请多留出至少{0}个仓库空间", amount - MWarehouse.size.Rest));
            return false;
        }
        if (info.item.StackAble)
        {
            MWarehouse.GetItemSimple(info, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && x.MItemInfo.item == info.item);
            if (ia) ia.UpdateInfo();
            else
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.SetItem(MWarehouse.Latest);
                else
                {
                    MessageManager.Instance.New("发生内部错误！");
                    Debug.Log("[Store Item Error: Can't find ItemAgent] ID: " + info.ItemID + "[" + DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                MWarehouse.GetItemSimple(info);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.SetItem(MWarehouse.Latest);
                        break;
                    }
            }
        BackpackManager.Instance.LoseItem(info, amount);
        UpdateUI();
        return true;
    }

    public void TakeOutItem(ItemInfo info, bool all = false)
    {
        if (MWarehouse == null || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseWindow();
        if (!all)
            if (info.Amount == 1 && OnTakeOut(info, 1))
                MessageManager.Instance.New(string.Format("取出了1个 [{0}]", info.ItemName));
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
                AmountManager.Instance.New(delegate
                {
                    if (OnTakeOut(info, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.New(string.Format("取出了{0}个 [{1}]", (int)AmountManager.Instance.Amount, info.ItemName));
                }, info.Amount);
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
        if (MWarehouse == null || info == null || !info.item || amount < 1) return false;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        return LoseItem(info, finalLose);
    }

    private bool LoseItem(ItemInfo info, int amount)
    {
        if (MWarehouse == null || info == null || !info.item || amount < 1) return false;
        if (!BackpackManager.Instance.TryGetItem_Boolean(info, amount)) return false;
        BackpackManager.Instance.GetItem(info, amount);
        MWarehouse.LoseItemSimple(info, amount);
        ItemAgent ia = GetItemAgentByInfo(info);
        if (ia) ia.UpdateInfo();
        UpdateUI();
        if (!BackpackManager.Instance.IsUIOpen)
            BackpackManager.Instance.OpenWindow();
        return true;
    }

    public int GetItemAmount(string id)
    {
        var items = MWarehouse.Items.FindAll(x => x.ItemID == id);
        if (items.Count < 1) return 0;
        if (items[0].item.StackAble) return items[0].Amount;
        return items.Count;
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmount(id) > 0;
    }

    public ItemAgent GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.MItemInfo == info);
    }
    #endregion

    #region UI相关
    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        Init(MWarehouse);
        BackpackManager.Instance.OpenWindow();
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteractive(false);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        MWarehouse = null;
        foreach (ItemAgent ia in itemAgents)
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
        UI.money.text = MWarehouse.Money.ToString();
        UI.size.text = MWarehouse.size.ToString();
        SetPage(currentPage);
    }

    public void Sort()
    {
        MWarehouse.Sort();
        //Init(MWarehouse);
        foreach (ItemAgent ia in itemAgents)
        {
            ia.Empty();
        }
        foreach (ItemInfo ii in MWarehouse.Items)
        {
            foreach (ItemAgent ia in itemAgents)
            {
                if (ia.IsEmpty)
                {
                    ia.SetItem(ii);
                    break;
                }
            }
        }
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

    public void CanStore(WarehouseAgent agent)
    {
        if (DialogueManager.Instance.TalkAble || DialogueManager.Instance.IsUIOpen || IsUIOpen) return;
        MWarehouse = agent.MWarehouse;
        StoreAble = true;
        UIManager.Instance.EnableInteractive(true, agent.name);
    }

    public void CannotStore()
    {
        CloseWindow();
        ItemWindowManager.Instance.CloseWindow();
        StoreAble = false;
        if (!(DialogueManager.Instance.TalkAble || DialogueManager.Instance.IsUIOpen)) UIManager.Instance.EnableInteractive(false);
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
        if (!UI || !UI.gameObject || !MWarehouse) return;
        for (int i = 0; i < MWarehouse.size.Max; i++)
        {
            ZetanUtility.SetActive(itemAgents[i].gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        if (!UI || !UI.gameObject || !MWarehouse) return;
        for (int i = 0; i < MWarehouse.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsEquipment)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        if (!UI || !UI.gameObject || !MWarehouse) return;
        for (int i = 0; i < MWarehouse.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsConsumable)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        if (!UI || !UI.gameObject || !MWarehouse) return;
        for (int i = 0; i < MWarehouse.size.Max; i++)
        {
            if (!itemAgents[i].IsEmpty && itemAgents[i].MItemInfo.item.IsMaterial)
                ZetanUtility.SetActive(itemAgents[i].gameObject, true);
            else ZetanUtility.SetActive(itemAgents[i].gameObject, false);
        }
    }
    #endregion
}
