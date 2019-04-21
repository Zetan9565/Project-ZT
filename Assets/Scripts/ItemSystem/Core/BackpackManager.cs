using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public delegate void ItemAmountListener(ItemBase item, int amount);

[DisallowMultipleComponent]
public class BackpackManager : MonoBehaviour, IOpenCloseable
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

    public GameObject DiscardArea { get { return UI.discardArea; } }
    public ScrollRect GridRect { get { return UI.gridRect; } }

    public ItemBase testItem;

    public int currentPage;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            GetItem(testItem);
    }

    public event ItemAmountListener OnGetItemEvent;
    public event ItemAmountListener OnLoseItemEvent;

    public Dictionary<string, List<ItemBase>> MItems { get; } = new Dictionary<string, List<ItemBase>>();

    //public List<ItemInfo> Items { get; } = new List<ItemInfo>();
    public List<ItemAgent> itemAgents { get; } = new List<ItemAgent>();

    public Backpack MBackpack
    {
        get
        {
            if (PlayerInfoManager.Instance) return PlayerInfoManager.Instance.Backpack;
            else return null;
        }
    }

    public void Init()
    {
        if (MBackpack != null)
        {
            foreach (ItemAgent ia in itemAgents)
                ia.Clear(true);
            int originalSize = itemAgents.Count;
            if (MBackpack.backpackSize.Max >= originalSize)
                for (int i = 0; i < MBackpack.backpackSize.Max - originalSize; i++)
                {
                    ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
                    ia.agentType = ItemAgentType.Backpack;
                    itemAgents.Add(ia);
                    ia.index = itemAgents.IndexOf(ia);
                }
            else
            {
                for (int i = 0; i < originalSize - MBackpack.backpackSize.Max; i++)
                {
                    itemAgents[i].Clear(false, true);
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

    public int TryGetItem_Integer(ItemInfo info)
    {
        return TryGetItem_Integer(info.Item, info.Amount);
    }
    public bool TryGetItem_Boolean(ItemBase item, int amount = 1)
    {
        if (MBackpack == null || !item || amount < 1) return false;
        if (MBackpack.IsFull)
        {
            MessageManager.Instance.NewMessage("行囊已满");
            return false;
        }
        if (!item.StackAble)
        {
            if (amount > MBackpack.backpackSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个行囊空间", amount));
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
    public bool TryGetItem_Boolean(ItemInfo info)
    {
        return TryGetItem_Boolean(info.Item, info.Amount);
    }

    /*public int TryLoseItem_Integer(ItemInfo item, int amount = 1)
    {

    }*/
    //public bool TryLoseItem_Boolean(ItemInfo info, int amount = 1)
    //{
    //    if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
    //    if (!info.Item.DiscardAble)
    //    {
    //        MessageManager.Instance.NewMessage("该物品不可丢弃");
    //        return false;
    //    }
    //    int finalLose = info.Amount < amount ? info.Amount : amount;
    //    if (QuestRequiredItem(info.Item, info.Amount - finalLose))
    //    {
    //        MessageManager.Instance.NewMessage("该物品为任务所需");
    //        return false;
    //    }
    //    return true;
    //}

    public bool GetItem(ItemBase item, int amount = 1, int index = -1)
    {
        if (MBackpack == null || !item || amount < 1) return false;
        if (MBackpack.IsFull)
        {
            MessageManager.Instance.NewMessage("行囊已满");
            return false;
        }
        if (!item.StackAble)
        {
            if (amount > MBackpack.backpackSize.Rest)
            {
                MessageManager.Instance.NewMessage(string.Format("请至少留出{0}个行囊空间", amount));
                return false;
            }
        }
        if (MBackpack.weightLoad.Rest < amount * item.Weight)
        {
            MessageManager.Instance.NewMessage("这些物品太重了");
            return false;
        }
        if (item.StackAble)
        {
            MBackpack.GetItemSimple(item, amount);
            ItemAgent ia = itemAgents.Find(x => !x.IsEmpty && x.itemInfo.Item == item);
            if (ia)
            {
                ia.UpdateInfo();
            }
            else
            {
                ia = itemAgents.Find(x => x.IsEmpty);
                if (ia)
                {
                    ia.Init(MBackpack.Latest, ItemAgentType.Backpack);
                }
                else
                {
                    MessageManager.Instance.NewMessage("发生内部错误！");
                    Debug.Log("[Get Item Error] ID: " + item.ID + "[" + System.DateTime.Now.ToString() + "]");
                }
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                MBackpack.GetItemSimple(item);
                if (index > -1 && index < itemAgents.Count)
                    itemAgents[i].Init(MBackpack.Latest, ItemAgentType.Backpack);
                else foreach (ItemAgent ia in itemAgents)
                    {
                        if (ia.IsEmpty)
                        {
                            ia.Init(MBackpack.Latest, ItemAgentType.Backpack);
                            break;
                        }
                    }
            }
        }
        OnGetItemEvent?.Invoke(item, amount);
        QuestManager.Instance.UpdateUI();
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
        AmountHandler.Instance.Init(delegate { OnConfirmDiscard(info); }, info.Amount);
    }

    public void OnConfirmDiscard(ItemInfo info)
    {
        Discard(info, (int)AmountHandler.Instance.Amount);
    }

    bool Discard(ItemInfo info, int amount = 1)
    {
        if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
        if (!info.Item.DiscardAble)
        {
            MessageManager.Instance.NewMessage("该物品不可丢弃");
            return false;
        }
        int finalLose = info.Amount < amount ? info.Amount : amount;
        if (QuestRequiredItem(info.Item, info.Amount - finalLose))
        {
            MessageManager.Instance.NewMessage("该物品为任务所需");
            return false;
        }
        return LoseItem(info, finalLose);
    }

    public bool LoseItem(ItemInfo info, int amount = 1)
    {
        if (MBackpack == null || info == null || !info.Item || amount < 1) return false;
        int finalLose = info.Amount < amount ? info.Amount : amount;
        MBackpack.LoseItemSimple(info, finalLose);
        ItemAgent ia = GetItemAgentByInfo(info);
        ia.UpdateInfo();
        if (info.Amount < 1)
        {
            if (ia) ia.Clear(true);
        }
        OnLoseItemEvent?.Invoke(info.Item, finalLose);
        if (ItemWindowManager.Instance.ItemInfo == info && info.Amount < 1) ItemWindowManager.Instance.CloseItemWindow();
        QuestManager.Instance.UpdateUI();
        UpdateUI();
        return true;
    }

    public bool LoseItem(string itemID, int amount = 1)
    {
        ItemInfo[] finds = MBackpack.FindAll(itemID).ToArray();
        if (finds.Length < 1)
        {
            MessageManager.Instance.NewMessage("该物品不在行囊中");
            return false;
        }
        foreach (ItemInfo find in finds)
        {
            if (!LoseItem(find, amount))
                return false;
        }
        return true;
    }

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
                MessageManager.Instance.NewMessage("该物品不在行囊中");
                return false;
            }
            for (int i = 0; i < amount; i++)
                if (!LoseItem(finds[i]))
                    return false;
            return true;
        }
    }

    public int GetItemAmountByID(string id)
    {
        if (MBackpack == null) return 0;
        return MBackpack.GetItemAmountByID(id);
    }
    public int GetItemAmountByItem(ItemBase item)
    {
        if (MBackpack == null) return 0;
        return MBackpack.GetItemAmountByItem(item);
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmountByID(id) > 0;
    }

    /// <summary>
    /// 判定是否有某个任务需要某数量的某个道具
    /// </summary>
    /// <param name="item">要判定的道具ID</param>
    /// <param name="leftAmount">要判定的数量</param>
    /// <returns>是否需要该道具</returns>
    private bool QuestRequiredItem(ItemBase item, int leftAmount)
    {
        return QuestsRequiredItem(item, leftAmount).Count() > 0;
    }
    public IEnumerable<Quest> QuestsRequiredItem(ItemBase item, int leftAmount)
    {
        return QuestManager.Instance.QuestsOngoing.FindAll(x => x.RequiredItem(item, leftAmount)).AsEnumerable();
    }

    public ItemAgent GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.itemInfo == info);
    }

    public IEnumerable<ItemAgent> GetItemAgentsByItem(ItemBase item)
    {
        return itemAgents.FindAll(x => !x.IsEmpty && x.itemInfo.Item == item).AsEnumerable();
    }
    #endregion

    public void OpenUI()
    {
        if (IsPausing) return;
        UI.backpackWindow.alpha = 1;
        UI.backpackWindow.blocksRaycasts = true;
        WindowsManager.Instance.PushWindow(this);
        IsUIOpen = true;
    }
    public void CloseUI()
    {
        if (IsPausing) return;
        foreach (ItemAgent ia in itemAgents)
        {
            ia.FinishDrag();
        }
        UI.backpackWindow.alpha = 0;
        UI.backpackWindow.blocksRaycasts = false;
        IsUIOpen = false;
    }

    public void PauseDisplay(bool state)
    {
        if (!IsUIOpen) return;
        if (!state)
        {
            UI.backpackWindow.alpha = 1;
            UI.backpackWindow.blocksRaycasts = true;
        }
        else
        {
            UI.backpackWindow.alpha = 0;
            UI.backpackWindow.blocksRaycasts = false;
        }
        IsPausing = state;
    }

    public void OpenCloseUI()
    {
        if (UI.backpackWindow.alpha > 0) CloseUI();
        else OpenUI();
    }

    public void Sort()
    {
        Init();
        MBackpack.Sort();
        foreach (ItemInfo info in MBackpack.Items)
        {
            foreach (ItemAgent ia in itemAgents)
            {
                if (ia.IsEmpty)
                {
                    ia.Init(info, ItemAgentType.Backpack);
                    break;
                }
            }
        }
    }

    public void UpdateUI()
    {
        UI.weight.text = MBackpack.weightLoad.Current.ToString("F2") + "/" + MBackpack.weightLoad.Max.ToString("F2") + "WL";
        UI.size.text = MBackpack.backpackSize.ToString();
        SetPage(currentPage);
    }

    public void SetUI(BackpackUI UI)
    {
        this.UI = UI;
    }

    #region 道具页相关
    public void SetPage(int index)
    {
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
            if (!ia.IsEmpty && !ia.itemInfo.Item.IsEquipment)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        ShowAll();
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.itemInfo.Item.IsConsumable)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        ShowAll();
        foreach (ItemAgent ia in itemAgents)
        {
            if (!ia.IsEmpty && !ia.itemInfo.Item.IsMaterial)
                MyTools.SetActive(ia.gameObject, false);
            else if (ia.IsEmpty) MyTools.SetActive(ia.gameObject, false);
        }
    }
    #endregion

    public void LoadData(BackpackData backpackData)
    {
        if (MBackpack == null) return;
        foreach (ItemData id in backpackData.itemDatas)
            if (!GameManager.Instance.GetItemByID(id.itemID)) return;
        MBackpack.LoseMoneySimple(MBackpack.Money);
        MBackpack.GetMoneySimple(backpackData.money);
        MBackpack.backpackSize = new ScopeInt(backpackData.maxSize) { Current = backpackData.currentSize };
        MBackpack.weightLoad = new ScopeFloat(backpackData.maxWeightLoad) { Current = backpackData.currentSize };
        MBackpack.Items.Clear();
        Init();
        foreach (ItemData id in backpackData.itemDatas)
        {
            ItemInfo newInfo = new ItemInfo(GameManager.Instance.GetItemByID(id.itemID), id.amount);
            MBackpack.Items.Add(newInfo);
            if (id.indexInBP > -1 && id.indexInBP < itemAgents.Count)
                itemAgents[id.indexInBP].Init(newInfo, ItemAgentType.Backpack);
        }
        UpdateUI();
    }

}
