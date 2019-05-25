using System;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseManager : MonoBehaviour, IWindow
{
    private static WarehouseManager instance;
    public static WarehouseManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<WarehouseManager>();
            return instance;
        }
    }

    [SerializeField]
    private WarehouseUI UI;

    public Transform cellsParent { get { return UI.itemCellsParent; } }

    private List<ItemAgent> itemAgents { get; } = new List<ItemAgent>();

    public Warehouse MWarehouse { get; private set; }

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public bool StoreAble { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            return UI.windowCanvas;
        }
    }

    public void Init(Warehouse warehouse)
    {
        if (warehouse != null)
        {
            MWarehouse = warehouse;
            foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            int originalSize = itemAgents.Count;
            if (MWarehouse.warehouseSize.Max >= originalSize)
                for (int i = 0; i < MWarehouse.warehouseSize.Max - originalSize; i++)
                {
                    ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                    itemAgents.Add(ia);
                    ia.Init(ItemAgentType.Warehouse, itemAgents.IndexOf(ia), UI.gridRect);
                }
            else
            {
                for (int i = 0; i < originalSize - MWarehouse.warehouseSize.Max; i++)
                {
                    Debug.Log("else");
                    itemAgents[i].Clear(true);
                }
                itemAgents.RemoveAll(x => !x.gameObject.activeSelf);
            }
            /*foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            itemAgents.RemoveAll(x => !x.gameObject.activeSelf);
            for (int i = 0; i < MWarehouse.warehouseSize.Max; i++)
            {
                ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                itemAgents.Add(ia);
                ia.Init(ItemAgentType.Warehouse, itemAgents.IndexOf(ia), UI.gridRect);
            }*/
            foreach (ItemInfo info in MWarehouse.Items)
            {
                if (info.indexInGrid > 0 && info.indexInGrid < itemAgents.Count)
                    itemAgents[info.indexInGrid].InitItem(info);
                else foreach (ItemAgent ia in itemAgents)
                        if (ia.IsEmpty)
                        {
                            ia.InitItem(info);
                            break;
                        }
            }
            UpdateUI();
        }
    }

    public void StoreItem(ItemInfo info, bool all = false)
    {
        if (MWarehouse == null || info == null || !info.Item) return;
        ItemWindowHandler.Instance.CloseItemWindow();
        if (!all)
        {
            if (info.Amount == 1 && OnStore(info))
                MessageManager.Instance.NewMessage(string.Format("存入了1个 [{0}]", info.ItemName));
            else
            {
                AmountHandler.Instance.SetPosition(MyTools.ScreenCenter);
                AmountHandler.Instance.Init(delegate
                {
                    if (OnStore(info, (int)AmountHandler.Instance.Amount))
                        MessageManager.Instance.NewMessage(string.Format("存入了{0}个 [{1}]", (int)AmountHandler.Instance.Amount, info.ItemName));
                }, info.Amount);
            }
        }
        else
        {
            int amountBef = GetItemAmount(info.ItemID);
            if (OnStore(info, info.Amount))
                MessageManager.Instance.NewMessage(string.Format("存入了{0}个 [{1}]", GetItemAmount(info.ItemID) - amountBef, info.ItemName));
        }
    }

    public bool OnStore(ItemInfo info, int amount = 1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
        int finalGet = info.Amount < amount ? info.Amount : amount;
        return GetItem(info, finalGet);
    }

    public bool GetItem(ItemInfo info, int amount = 1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
        if (MWarehouse.IsFull)
        {
            MessageManager.Instance.NewMessage("仓库已满");
            return false;
        }
        if (!info.Item.StackAble)
            if (amount > MWarehouse.warehouseSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个仓库空间", amount));
                return false;
            }
        if (info.Item.StackAble)
        {
            MWarehouse.GetItemSimple(info, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && x.MItemInfo.Item == info.Item);
            if (ia) ia.UpdateInfo();
            else
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.InitItem(MWarehouse.Latest);
                else
                {
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Store Item Error] ID: " + info.ItemID + "[" + DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                MWarehouse.GetItemSimple(info);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.InitItem(MWarehouse.Latest);
                        break;
                    }
            }
        BackpackManager.Instance.LoseItem(info, amount);
        UpdateUI();
        return true;
    }

    public void TakeOutItem(ItemInfo info, bool all = false)
    {
        if (MWarehouse == null || info == null || !info.Item) return;
        ItemWindowHandler.Instance.CloseItemWindow();
        if (!all)
            if (info.Amount == 1 && OnTakeOut(info))
                MessageManager.Instance.NewMessage(string.Format("取出了1个 [{0}]", info.ItemName));
            else
            {
                AmountHandler.Instance.SetPosition(MyTools.ScreenCenter);
                AmountHandler.Instance.Init(delegate
                {
                    if (OnTakeOut(info, (int)AmountHandler.Instance.Amount))
                        MessageManager.Instance.NewMessage(string.Format("取出了{0}个 [{1}]", (int)AmountHandler.Instance.Amount, info.ItemName));
                }, info.Amount);
            }
        else
        {
            int amountBef = GetItemAmount(info.ItemID);
            if (OnTakeOut(info, info.Amount))
                MessageManager.Instance.NewMessage(string.Format("取出了{0}个 [{1}]", amountBef - GetItemAmount(info.ItemID), info.ItemName));
        }
    }

    bool OnTakeOut(ItemInfo info, int amount = 1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        return LoseItem(info, finalLose);
    }

    public bool LoseItem(ItemInfo info, int amount = 1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
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
        if (items[0].Item.StackAble) return items[0].Amount;
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

    #region UI相关
    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        Init(MWarehouse);
        UI.warehouseWindow.alpha = 1;
        UI.warehouseWindow.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        BackpackManager.Instance.OpenWindow();
        //MyTools.SetActive(UI.warehouseButton.gameObject, false);
        UIManager.Instance.EnableJoyStick(false);
        UIManager.Instance.EnableInteractive(false);
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        UI.warehouseWindow.alpha = 0;
        UI.warehouseWindow.blocksRaycasts = false;
        IsUIOpen = false;
        IsPausing = false;
        MWarehouse = null;
        WindowsManager.Instance.Remove(this);
        foreach (ItemAgent ia in itemAgents)
        {
            ia.FinishDrag();
            ia.Empty();
        }
        if (BackpackManager.Instance.IsUIOpen) BackpackManager.Instance.CloseWindow();
        ItemWindowHandler.Instance.CloseItemWindow();
        if (DialogueManager.Instance.IsUIOpen) DialogueManager.Instance.PauseDisplay(false);
        AmountHandler.Instance.Cancel();
        UIManager.Instance.EnableJoyStick(true);
    }

    public void OpenCloseWindow()
    {

    }

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.warehouseWindow.alpha = 1;
            UI.warehouseWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.warehouseWindow.alpha = 0;
            UI.warehouseWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void UpdateUI()
    {
        UI.money.text = MWarehouse.Money.ToString();
        UI.size.text = MWarehouse.warehouseSize.ToString();
    }

    public void Sort()
    {
        MWarehouse.Sort();
        Init(MWarehouse);
    }

    public void SetUI(WarehouseUI UI)
    {
        this.UI = UI;
    }

    public void ResetUI()
    {
        itemAgents.Clear();
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
    }
    #endregion

    public void CanStore(WarehouseAgent agent)
    {
        if (DialogueManager.Instance.TalkAble || DialogueManager.Instance.IsUIOpen || IsUIOpen) return;
        MWarehouse = agent.MWarehouse;
        StoreAble = true;
        UIManager.Instance.EnableInteractive(true, agent.MBuilding.name);
    }

    public void CannotStore()
    {
        //MyTools.SetActive(UI.warehouseButton.gameObject, false);
        CloseWindow();
        ItemWindowHandler.Instance.CloseItemWindow();
        StoreAble = false;
        if (!(DialogueManager.Instance.TalkAble || DialogueManager.Instance.IsUIOpen)) UIManager.Instance.EnableInteractive(false);
    }
}
