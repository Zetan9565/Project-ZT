using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MakingManager : SingletonMonoBehaviour<MakingManager>, IWindow
{
    [SerializeField]
    private MakingUI UI;

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

    public List<ItemBase> ItemLearned { get; private set; } = new List<ItemBase>();

    public List<MakingAgent> MakingAgents { get; private set; } = new List<MakingAgent>();

    public MakingTool CurrentTool { get; private set; }

    private ItemBase currentItem;

    public void Init()
    {
        foreach (MakingAgent ma in MakingAgents)
            ma.Clear(true);
        MakingAgents.Clear();
        if (CurrentTool)
            foreach (ItemBase item in ItemLearned)
                if (item.MakingTool == CurrentTool.ToolType)
                {
                    MakingAgent ma = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<MakingAgent>();
                    ma.Init(item);
                    MakingAgents.Add(ma);
                }
    }

    public void MakeCurrent()
    {
        if (!CurrentTool || !currentItem) return;
        int amountCanMake = currentItem.GetMakeAmount(BackpackManager.Instance.MBackpack);
        if (amountCanMake < 1)
        {
            MessageManager.Instance.NewMessage("材料不足");
            return;
        }
        if (amountCanMake > 0 && amountCanMake < 2)
        {
            ConfirmManager.Instance.NewConfirm(string.Format("确定制作1个 [{0}] 吗？", currentItem.name), delegate
            {
                if (OnMake(currentItem))
                    MessageManager.Instance.NewMessage(string.Format("制作了1个 [{0}]", currentItem.name));
            });
        }
        else
        {
            AmountManager.Instance.SetPosition(MyUtilities.ScreenCenter, Vector2.zero);
            AmountManager.Instance.NewAmount(delegate
            {
                if (OnMake(currentItem, (int)AmountManager.Instance.Amount))
                    MessageManager.Instance.NewMessage(string.Format("制作了{0}个 [{1}]", currentItem.name, (int)AmountManager.Instance.Amount));
            }, amountCanMake);
        }
    }

    private bool OnMake(ItemBase item, int amount = 1)
    {
        if (!item || amount < 1 || !currentItem) return false;
        foreach (MatertialInfo mi in item.Materials)
            if (!BackpackManager.Instance.TryLoseItem_Boolean(mi.Item, mi.Amount * amount))
                return false;
        foreach (MatertialInfo mi in item.Materials)//模拟空出位置来放制作的道具
        {
            BackpackManager.Instance.MBackpack.weightLoad -= mi.Item.Weight * mi.Amount * amount;
            if (mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount * amount == 0)//可叠加且消耗后用尽，则空出一格
                BackpackManager.Instance.MBackpack.backpackSize--;
            else if (!mi.Item.StackAble)//不可叠加，则消耗多少个就能空出多少格
                BackpackManager.Instance.MBackpack.backpackSize -= amount;
        }
        if (!BackpackManager.Instance.TryGetItem_Boolean(currentItem, amount))
        {
            foreach (MatertialInfo mi in item.Materials)//取消模拟
            {
                BackpackManager.Instance.MBackpack.weightLoad += mi.Item.Weight * mi.Amount * amount;
                if (mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount * amount == 0)
                    BackpackManager.Instance.MBackpack.backpackSize++;
                else if (!mi.Item.StackAble)
                    BackpackManager.Instance.MBackpack.backpackSize += amount;
            }
            return false;
        }
        foreach (MatertialInfo mi in item.Materials)//取消模拟并确认操作
        {
            BackpackManager.Instance.MBackpack.weightLoad += mi.Item.Weight * mi.Amount * amount;
            if (mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount * amount == 0)
                BackpackManager.Instance.MBackpack.backpackSize++;
            else if (!mi.Item.StackAble)
                BackpackManager.Instance.MBackpack.backpackSize += amount;
            BackpackManager.Instance.LoseItem(mi.Item, mi.Amount * amount);
        }
        BackpackManager.Instance.GetItem(item, amount);
        UpdateUI();
        return true;
    }

    public bool Learn(ItemBase item)
    {
        if (!item) return false;
        if (item.MakingMethod == MakingMethod.None)
        {
            MessageManager.Instance.NewMessage("无效的道具");
            return false;
        }
        if (ItemLearned.Contains(item))
        {
            MessageManager.Instance.NewMessage("已经学会制作 [" + item.name + "]");
            return false;
        }
        ItemLearned.Add(item);
        MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的制作方法!", item.name));
        return true;
    }

    #region UI相关
    public void OpenWindow()
    {
        if (!CurrentTool) return;
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        Init();
        UI.makingWindow.alpha = 1;
        UI.makingWindow.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        if (UI.tabs != null && UI.tabs.Length > 0)
            UI.tabs[0].isOn = true;
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.makingWindow.alpha = 0;
        UI.makingWindow.blocksRaycasts = false;
        IsUIOpen = false;
        WindowsManager.Instance.Remove(this);
        CurrentTool = null;
        currentItem = null;
        HideDescription();
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseItemWindow();
    }

    void IWindow.OpenCloseWindow() { }

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.makingWindow.alpha = 1;
            UI.makingWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.makingWindow.alpha = 0;
            UI.makingWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void ShowDescription(ItemBase item)
    {
        if (!item) return;
        currentItem = item;
        List<string> info = currentItem.GetMaterialsInfo(BackpackManager.Instance.MBackpack).ToList();
        string materials = "<b>制作材料：</b>\n";
        for (int i = 0; i < info.Count; i++)
        {
            materials += info[i] + (i == info.Count - 1 ? string.Empty : "\n");
        }
        UI.description.text = materials;
        int makeAmount = currentItem.GetMakeAmount(BackpackManager.Instance.MBackpack);
        UI.icon.InitItem(new ItemInfo(currentItem, makeAmount));
        UI.makeButton.interactable = makeAmount > 0;
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
    }

    public void HideDescription()
    {
        UI.description.text = string.Empty;
        UI.icon.Empty();
        UI.descriptionWindow.alpha = 0;
        UI.descriptionWindow.blocksRaycasts = false;
    }

    public void UpdateUI()
    {
        if (!IsUIOpen) return;
        Init();
        if (UI.descriptionWindow.alpha > 0) ShowDescription(currentItem);
        SetPage(currentPage);
    }

    public void CanMake(MakingTool tool)
    {
        if (!tool) return;
        CurrentTool = tool;
        UIManager.Instance.EnableInteractive(true);
    }

    public void CannotMake()
    {
        UIManager.Instance.EnableInteractive(false);
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
            case 3: ShowConsumables(); break;
            case 4: ShowMaterials(); break;
            default: ShowAll(); break;
        }
    }

    private void ShowAll()
    {
        if (!UI || !UI.gameObject) return;
        foreach (MakingAgent ia in MakingAgents)
        {
            MyUtilities.SetActive(ia.gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsEquipment)
                MyUtilities.SetActive(ia.gameObject, true);
            else MyUtilities.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsConsumable)
                MyUtilities.SetActive(ia.gameObject, true);
            else MyUtilities.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsMaterial)
                MyUtilities.SetActive(ia.gameObject, true);
            else MyUtilities.SetActive(ia.gameObject, false);
        }
    }
    #endregion

    public void SetUI(MakingUI UI)
    {
        this.UI = UI;
    }

    public void ResetUI()
    {
        MakingAgents.Clear();
    }
    #endregion
}
