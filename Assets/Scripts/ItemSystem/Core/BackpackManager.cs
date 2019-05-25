using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public delegate void ItemAmountListener(ItemBase item, int amount);

[DisallowMultipleComponent]
public class BackpackManager : MonoBehaviour, IWindow
{
    private static BackpackManager instance;
    public static BackpackManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<BackpackManager>();
            return instance;
        }
    }

    [SerializeField]
    private BackpackUI UI;

    public bool IsPausing { get; private set; }
    public bool IsUIOpen { get; private set; }

    public Transform cellsParent { get { return UI.itemCellsParent; } }

#if UNITY_ANDROID
    public GameObject DiscardArea { get { return UI.discardArea; } }
#endif

    public Image GridMask { get { return UI.gridMask; } }

    private int currentPage;

    public event ItemAmountListener OnGetItemEvent;
    public event ItemAmountListener OnLoseItemEvent;

    private List<ItemAgent> itemAgents = new List<ItemAgent>();

    public Backpack MBackpack
    {
        get
        {
            if (PlayerManager.Instance) return PlayerManager.Instance.Backpack;
            else return null;
        }
    }

    public void Init()
    {
        if (!UI || !UI.gameObject) return;
        if (MBackpack != null)
        {
            foreach (ItemAgent ia in itemAgents)
                ia.Empty();
            int originalSize = itemAgents.Count;
            if (MBackpack.backpackSize.Max >= originalSize)
                for (int i = 0; i < MBackpack.backpackSize.Max - originalSize; i++)
                {
                    ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                    itemAgents.Add(ia);
                    ia.Init(ItemAgentType.Backpack, itemAgents.IndexOf(ia), UI.gridRect);
                }
            else
            {
                for (int i = 0; i < originalSize - MBackpack.backpackSize.Max; i++)
                {
                    itemAgents[i].Clear(true);
                }
                itemAgents.RemoveAll(x => !x.gameObject.activeSelf);
            }
            UpdateUI();
        }
    }


    #region 道具处理相关
    /// <summary>
    /// 尝试获取道具
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">目标获取量</param>
    /// <returns>实际获取量</returns>
    public int TryGetItem_Integer(ItemBase item, int amount = 1)
    {
        if (MBackpack == null || !item || amount < 1) return 0;
        if (MBackpack.IsFull)
        {
            return 0;
        }
        int finalGet = amount;
        if (!item.StackAble)
        {
            if (amount > MBackpack.backpackSize.Rest)
                finalGet = MBackpack.backpackSize.Rest;
        }
        if (MBackpack.weightLoad.Rest + finalGet * item.Weight > MBackpack.weightLoad.Max * 1.5f)
            for (int i = 0; i < MBackpack.backpackSize.Rest; i++)
            {
                if (MBackpack.weightLoad.Rest + i * item.Weight <= MBackpack.weightLoad.Max * 1.5f &&
                    MBackpack.weightLoad.Rest + (i + 1) * item.Weight > MBackpack.weightLoad.Max * 1.5f)
                {
                    finalGet = i;
                    break;
                }
            }
        return finalGet;
    }

    public int TryGetItem_Integer(ItemInfo info)//拾取掉落物用到
    {
        return TryGetItem_Integer(info.Item, info.Amount);
    }
    public bool TryGetItem_Boolean(ItemBase item, int amount = 1)
    {
        if (MBackpack == null || !item || amount < 1) return false;
        if (MBackpack.IsFull)
        {
            MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "已满");
            return false;
        }
        if (!item.StackAble)
        {
            if (amount > MBackpack.backpackSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个{1}空间", amount, GameManager.Instance.BackpackName));
                return false;
            }
        }
        if (MBackpack.weightLoad.Rest < amount * item.Weight)
        {
            MessageManager.Instance.NewMessage("这些物品太重了");
            return false;
        }
        return true;
    }
    public bool TryGetItem_Boolean(ItemInfo info, int amount = -1)
    {
        if (info == null) return false;
        return TryGetItem_Boolean(info.Item, amount < 0 ? info.Amount : amount);
    }

    public bool GetItem(ItemBase item, int amount = 1)
    {
        if (!TryGetItem_Boolean(item, amount)) return false;
        if (item.StackAble)
        {
            MBackpack.GetItemSimple(item, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && (x.MItemInfo.Item == item || x.MItemInfo.ItemID == item.ID));
            if (ia) ia.UpdateInfo();
            else//如果找不到，说明该物品是新的，原来背包里没有的
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.InitItem(MBackpack.Latest);
                else
                {
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Get Item Error] ID: " + item.ID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                MBackpack.GetItemSimple(item);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.InitItem(MBackpack.Latest);
                        break;
                    }
            }
        OnGetItemEvent?.Invoke(item, amount);
        UpdateUI();
        return true;
    }

    public bool GetItem(ItemInfo info, int amount)//仓库、装备专用
    {
        if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
        if (!TryGetItem_Boolean(info.Item, amount)) return false;
        if (info.Item.StackAble)
        {
            MBackpack.GetItemSimple(info, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && (x.MItemInfo.Item == info.Item || x.MItemInfo.ItemID == info.ItemID));
            if (ia) ia.UpdateInfo();
            else//如果找不到，说明该物品是新的，原来背包里没有的
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia) ia.InitItem(MBackpack.Latest);
                else
                {
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Get Item Error] ID: " + info.Item.ID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else for (int i = 0; i < amount; i++)
            {
                MBackpack.GetItemSimple(info);
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.InitItem(MBackpack.Latest);
                        break;
                    }
            }
        OnGetItemEvent?.Invoke(info.Item, amount);
        UpdateUI();
        return true;
    }

    public bool GetItem(ItemInfo info)
    {
        return GetItem(info.Item, info.Amount);
    }

    public void DiscardItem(ItemInfo info)
    {
        if (MBackpack == null || info == null || !info.Item) return;
        if (!info.Item.DiscardAble)
        {
            MessageManager.Instance.NewMessage("该物品不可丢弃");
            return;
        }
        if (info.Amount < 2 && info.Amount > 0)
        {
            ConfirmHandler.Instance.NewConfirm(string.Format("确定丢弃1个 [{0}] 吗？", info.ItemName), delegate
            {
                if (OnDiscard(info))
                    MessageManager.Instance.NewMessage(string.Format("丢掉了1个 [{0}]", info.ItemName));
            });
        }
        else AmountHandler.Instance.Init(delegate
        {
            ConfirmHandler.Instance.NewConfirm(string.Format("确定丢弃{0}个 [{1}] 吗？", (int)AmountHandler.Instance.Amount, info.ItemName), delegate
            {
                if (OnDiscard(info, (int)AmountHandler.Instance.Amount))
                    MessageManager.Instance.NewMessage(string.Format("丢掉了{0}个 [{1}]", (int)AmountHandler.Instance.Amount, info.ItemName));
            });
        }, info.Amount);
    }

    bool OnDiscard(ItemInfo info, int amount = 1)
    {
        if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        if (QuestManager.Instance.HasQuestRequiredItem(info.Item, info.Amount - finalLose))
        {
            MessageManager.Instance.NewMessage("该物品为任务所需");
            return false;
        }
        return LoseItem(info, finalLose);
    }

    public bool TryLoseItem_Boolean(ItemBase item, int amount = 1)
    {
        if (MBackpack == null || !item || amount < 1) return false;
        if (GetItemAmount(item) < amount)
        {
            MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "中没有这么多的 [" + item.name + "]");
            return false;
        }
        if (QuestManager.Instance.HasQuestRequiredItem(item, GetItemAmount(item) - amount))
        {
            MessageManager.Instance.NewMessage(string.Format("[{0}] 为任务所需", item.name));
            return false;
        }
        return true;
    }
    public bool TryLoseItem_Boolean(ItemInfo info, int amount = 1)
    {
        return TryLoseItem_Boolean(info.Item, amount);
    }

    public bool LoseItem(ItemInfo info, int amount = 1)
    {
        if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
        if (!TryLoseItem_Boolean(info, amount)) return false;
        int amountBef = info.Amount;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        MBackpack.LoseItemSimple(info, finalLose);
        ItemAgent ia = GetItemAgentByInfo(info);
        if (ia) ia.UpdateInfo();
        OnLoseItemEvent?.Invoke(info.Item, finalLose);
        if (ItemWindowHandler.Instance.MItemInfo == info && info.Amount < 1) ItemWindowHandler.Instance.CloseItemWindow();
        UpdateUI();
        return true;
    }

    /*private void Awake()
    {
        OnLoseItemEvent += DebugLose;
    }

    public void DebugLose(ItemBase item, int amount)
    {
        Debug.LogFormat("失去[{0}]{1}个。", item.Name, amount);
    }*/

    public bool LoseItem(ItemBase item, int amount = 1)
    {
        if (item.StackAble)
        {
            return LoseItem(MBackpack.Find(item), amount);
        }
        else
        {
            ItemInfo[] finds = MBackpack.FindAll(item).ToArray();
            if (finds.Length < 1)
            {
                MessageManager.Instance.NewMessage("该物品不在" + GameManager.Instance.BackpackName + "中");
                return false;
            }
            for (int i = 0; i < amount; i++)
                if (!LoseItem(finds[i]))
                    return false;
            return true;
        }
    }

    public void GetMoney(long value)
    {
        if (value < 0) return;
        MBackpack.GetMoneySimple(value);
        UpdateUI();
    }

    public bool TryLoseMoney(long value)
    {
        if (value < 0) return false;
        if (MBackpack.Money < value)
        {
            MessageManager.Instance.NewMessage("货币不足");
        }
        return true;
    }
    public void LoseMoney(long value)
    {
        if (!TryLoseMoney(value)) return;
        MBackpack.LoseMoneySimple(value);
        UpdateUI();
    }

    public int GetItemAmount(string id)
    {
        if (MBackpack == null) return 0;
        return MBackpack.GetItemAmount(id);
    }
    public int GetItemAmount(ItemBase item)
    {
        if (MBackpack == null) return 0;
        return MBackpack.GetItemAmount(item);
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmount(id) > 0;
    }

    public ItemAgent GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.MItemInfo == info);
    }

    public IEnumerable<ItemAgent> GetItemAgentsByItem(ItemBase item)
    {
        return itemAgents.FindAll(x => !x.IsEmpty && x.MItemInfo.Item == item).AsEnumerable();
    }

    /// <summary>
    /// 扩展容量
    /// </summary>
    /// <param name="size">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool Expand(int size)
    {
        if (size < 1) return false;
        if (MBackpack.backpackSize.Max >= 192)
        {
            MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "已经达到最大容量了");
            return false;
        }
        int finallyExpand = MBackpack.backpackSize.Max + size > 192 ? 192 - MBackpack.backpackSize.Max : size;
        MBackpack.backpackSize.Max += finallyExpand;
        for (int i = 0; i < finallyExpand; i++)
        {
            ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            itemAgents.Add(ia);
            ia.Init(ItemAgentType.Backpack, itemAgents.IndexOf(ia), UI.gridRect);
        }
        MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "扩张了!");
        return true;
    }

    /// <summary>
    /// 扩展负重
    /// </summary>
    /// <param name="weightLoad">扩展数量</param>
    /// <returns>是否成功扩展</returns>
    public bool Expand(float weightLoad)
    {
        if (weightLoad < 0.01f) return false;
        if (MBackpack.weightLoad.Max >= 1500.0f)
        {
            MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "已经达到最大负重了");
            return false;
        }
        MBackpack.weightLoad.Max += MBackpack.weightLoad.Max + weightLoad > 1500.0f ? 1500.0f - MBackpack.weightLoad.Max : weightLoad;
        MessageManager.Instance.NewMessage(GameManager.Instance.BackpackName + "扩张了!");
        return true;
    }
    #endregion

    #region UI相关
    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        if (DialogueManager.Instance.IsTalking && !WarehouseManager.Instance.IsUIOpen && !ShopManager.Instance.IsUIOpen) return;
        UI.backpackWindow.alpha = 1;
        UI.backpackWindow.blocksRaycasts = true;
        WindowsManager.Instance.Push(this);
        IsUIOpen = true;
        GridMask.raycastTarget = true;
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsPausing) return;
        foreach (ItemAgent ia in itemAgents)
            ia.FinishDrag();
        UI.backpackWindow.alpha = 0;
        UI.backpackWindow.blocksRaycasts = false;
        IsUIOpen = false;
        IsPausing = false;
        WindowsManager.Instance.Remove(this);
        AmountHandler.Instance.Cancel();
        ItemWindowHandler.Instance.CloseItemWindow();
        if (WarehouseManager.Instance.IsUIOpen) WarehouseManager.Instance.CloseWindow();
        if (ShopManager.Instance.IsUIOpen) ShopManager.Instance.CloseWindow();
    }
    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.backpackWindow.alpha = 1;
            UI.backpackWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.backpackWindow.alpha = 0;
            UI.backpackWindow.blocksRaycasts = false;
            ItemWindowHandler.Instance.CloseItemWindow();
        }
        IsPausing = pause;
    }
    public void OpenCloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen)
            OpenWindow();
        else CloseWindow();
    }
    public Canvas SortCanvas
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

    public void Sort()
    {
        if (!UI || !UI.gameObject) return;
        MBackpack.Sort();
        Init();
        foreach (ItemInfo info in MBackpack.Items)
        {
            foreach (ItemAgent ia in itemAgents)
            {
                if (ia.IsEmpty)
                {
                    ia.InitItem(info);
                    break;
                }
            }
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (!UI || !UI.gameObject) return;
        UI.money.text = MBackpack.Money.ToString() + GameManager.Instance.CoinName;
        UI.weight.text = MBackpack.weightLoad.ToString("F2") + "WL";
        UI.size.text = MBackpack.backpackSize.ToString();
        SetPage(currentPage);
        QuestManager.Instance.UpdateUI();
        BuildingManager.Instance.UpdateUI();
        MakingManager.Instance.UpdateUI();
    }

    public void SetUI(BackpackUI UI)
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

    #region 道具页相关
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
        if (!UI || !UI.gameObject) return;
        foreach (ItemAgent ia in itemAgents)
        {
            MyTools.SetActive(ia.gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        ShowAll();
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.MItemInfo.Item.IsEquipment)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        ShowAll();
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.MItemInfo.Item.IsConsumable)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        ShowAll();
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.MItemInfo.Item.IsMaterial)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }
    #endregion
    #endregion

    public void LoadData(BackpackData backpackData)
    {
        if (MBackpack == null) return;
        foreach (ItemData id in backpackData.itemDatas)
            if (!GameManager.GetItemByID(id.itemID)) return;
        MBackpack.LoseMoneySimple(MBackpack.Money);
        MBackpack.GetMoneySimple(backpackData.money);
        MBackpack.backpackSize = new ScopeInt(backpackData.maxSize) { Current = backpackData.currentSize };
        MBackpack.weightLoad = new ScopeFloat(backpackData.maxWeightLoad) { Current = backpackData.currentSize };
        MBackpack.Items.Clear();
        Init();
        foreach (ItemData id in backpackData.itemDatas)
        {
            ItemInfo newInfo = new ItemInfo(GameManager.GetItemByID(id.itemID), id.amount);
            //TODO 把newInfo的耐久度等信息处理
            MBackpack.Items.Add(newInfo);
            if (id.indexInGrid > -1 && id.indexInGrid < itemAgents.Count)
                itemAgents[id.indexInGrid].InitItem(newInfo);
            else foreach (ItemAgent ia in itemAgents)
                {
                    if (ia.IsEmpty) { ia.InitItem(newInfo); break; }
                }
        }
        UpdateUI();
    }
}
