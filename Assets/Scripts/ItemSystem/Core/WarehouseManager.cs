using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WarehouseManager : MonoBehaviour
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

    public void OpenWarehouseWindow()
    {
        UI.warehouseWindow.alpha = 1;
        UI.warehouseWindow.blocksRaycasts = true;
    }

    public void Sort()
    {
        Debug.Log("Try sort");
    }

    public void CloseWarehouseWindow()
    {
        UI.warehouseWindow.alpha = 0;
        UI.warehouseWindow.blocksRaycasts = false;
    }

    public Transform cellsParent { get { return UI.itemCellsParent; } }
    public ScrollRect GridRect { get { return UI.gridRect; } }

    public Dictionary<string, List<ItemBase>> MItems { get; } = new Dictionary<string, List<ItemBase>>();

    public List<ItemInfo> Items { get; } = new List<ItemInfo>();
    public List<ItemAgent> itemAgents { get; } = new List<ItemAgent>();

    public Warehouse MWarehouse { get; private set; }

    public void Init()
    {
        if (MWarehouse != null)
        {
            foreach (ItemAgent ia in itemAgents)
                ia.Clear(true);
            int originalSize = itemAgents.Count;
            if (MWarehouse.backpackSize.Max >= originalSize)
                for (int i = 0; i < MWarehouse.backpackSize.Max - originalSize; i++)
                {
                    ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                    ia.agentType = ItemAgentType.Backpack;
                    itemAgents.Add(ia);
                    ia.index = itemAgents.IndexOf(ia);
                }
            else
            {
                for (int i = 0; i < originalSize - MWarehouse.backpackSize.Max; i++)
                {
                    itemAgents[i].Clear(false, true);
                }
                itemAgents.RemoveAll(x => !x.gameObject.activeSelf);
            }
            UpdateUI();
        }
    }

    /// <summary>
    /// 尝试获取道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">目标获取量</param>
    /// <returns>实际获取量</returns>
    public int TryGetItem_Integer(ItemBase item, int amount = 1)
    {
        if (MWarehouse == null || !item || amount < 1) return 0;
        if (MWarehouse.IsFull)
        {
            return 0;
        }
        int finalGet = amount;
        if (!item.StackAble)
        {
            if (amount > MWarehouse.backpackSize.Rest)
                finalGet = MWarehouse.backpackSize.Rest;
        }
        return finalGet;
    }
    public int TryGetItem_Integer(ItemInfo info)
    {
        return TryGetItem_Integer(info.Item, info.Amount);
    }
    public bool TryGetItem_Boolean(ItemBase item, int amount = 1)
    {
        if (MWarehouse == null || !item || amount < 1) return false;
        if (MWarehouse.IsFull)
        {
            MessageManager.Instance.NewMessage("行囊已满");
            return false;
        }
        if (!item.StackAble)
        {
            if (amount > MWarehouse.backpackSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个行囊空间", amount));
                return false;
            }
        }
        return true;
    }
    public bool TryGetItem_Boolean(ItemInfo info)
    {
        return TryGetItem_Boolean(info.Item, info.Amount);
    }

    public bool GetItem(ItemInfo info, int amount = 1, int index = -1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
        if (MWarehouse.IsFull)
        {
            MessageManager.Instance.NewMessage("仓库已满");
            return false;
        }
        if (!info.Item.StackAble)
        {
            if (amount > MWarehouse.backpackSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个仓库空间", amount));
                return false;
            }
        }
        if (info.Item.StackAble)
        {
            MWarehouse.GetItemSimple(info, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && x.itemInfo.Item == info.Item);
            if (ia)
            {
                ia.UpdateInfo();
            }
            else
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia)
                {
                    ia.Init(MWarehouse.Latest, ItemAgentType.Backpack);
                }
                else
                {
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Store Item Error] ID: " + info.ItemID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                MWarehouse.GetItemSimple(info);
                if (index > -1 && index < itemAgents.Count)
                    itemAgents[i].Init(MWarehouse.Latest, ItemAgentType.Backpack);
                else foreach (ItemAgent ia in itemAgents)
                    {
                        if (ia.IsEmpty)
                        {
                            ia.Init(MWarehouse.Latest, ItemAgentType.Backpack);
                            break;
                        }
                    }
            }
        }
        BackpackManager.Instance.LoseItem(info, amount);
        UpdateUI();
        return true;
    }

    public bool LoseItem(ItemInfo info, int amount = 1)
    {
        if (MWarehouse == null || info == null || !info.Item || amount < 1) return false;
        MWarehouse.LoseItemSimple(info);
        ItemAgent ia = GetItemAgentByInfo(info);
        ia.UpdateInfo();
        if (info.Amount < 1)
        {
            if (ia) ia.Clear(true);
        }
        QuestManager.Instance.UpdateUI();
        UpdateUI();
        return true;
    }

    public int GetItemAmountByID(string id)
    {
        var items = MWarehouse.Items.FindAll(x => x.ItemID == id);
        if (items.Count < 1) return 0;
        if (items[0].Item.StackAble) return items[0].Amount;
        return items.Count;
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmountByID(id) > 0;
    }

    public ItemAgent GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.itemInfo == info);
    }

    public IEnumerable<ItemAgent> GetItemAgentsByItem(ItemBase item)
    {
        return itemAgents.FindAll(x => !x.IsEmpty && x.itemInfo.Item == item).AsEnumerable();
    }

    public void UpdateUI()
    {
        UI.size.text = MWarehouse.backpackSize.ToString();
    }

    public void CanStore(WarehouseAgent warehouseAgent)
    {
        MWarehouse = warehouseAgent.Warehouse;
        MyTools.SetActive(UI.openButton.gameObject, true);
    }

    public void SetUI(WarehouseUI UI)
    {
        this.UI = UI;
    }
}
