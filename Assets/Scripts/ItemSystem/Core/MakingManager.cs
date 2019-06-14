using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MakingManager : MonoBehaviour, IWindow
{
    private static MakingManager instance;
    public static MakingManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<MakingManager>();
            return instance;
        }
    }

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
            ConfirmHandler.Instance.NewConfirm(string.Format("确定制作1个 [{0}] 吗？", currentItem.name), delegate
            {
                if (OnMake(currentItem))
                    MessageManager.Instance.NewMessage(string.Format("制作了1个 [{0}]", currentItem.name));
            });
        }
        else
        {
            AmountHandler.Instance.SetPosition(MyUtilities.ScreenCenter, Vector2.zero);
            AmountHandler.Instance.Init(delegate
            {
                if (OnMake(currentItem, (int)AmountHandler.Instance.Amount))
                    MessageManager.Instance.NewMessage(string.Format("制作了{0}个 [{1}]", currentItem.name, (int)AmountHandler.Instance.Amount));
            }, amountCanMake);
        }
    }

    private bool OnMake(ItemBase item, int amount = 1)
    {
        if (!item || amount < 1 || !currentItem) return false;
        foreach (MatertialInfo mi in item.Materials)
            if (!BackpackManager.Instance.TryLoseItem_Boolean(mi.Item, mi.Amount))
                return false;
        foreach (MatertialInfo mi in item.Materials)//模拟空出位置来放制作的道具
        {
            BackpackManager.Instance.MBackpack.weightLoad -= mi.Item.Weight * mi.Amount;
            if ((!mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount == 0) || mi.Item.StackAble)
                BackpackManager.Instance.MBackpack.backpackSize--;
        }
        if (!BackpackManager.Instance.TryGetItem_Boolean(currentItem, amount))
        {
            foreach (MatertialInfo mi in item.Materials)//取消模拟
            {
                BackpackManager.Instance.MBackpack.weightLoad += mi.Item.Weight * mi.Amount;
                if ((!mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount == 0) || mi.Item.StackAble)
                    BackpackManager.Instance.MBackpack.backpackSize++;
            }
            return false;
        }
        foreach (MatertialInfo mi in item.Materials)//取消模拟并确认操作
        {
            BackpackManager.Instance.MBackpack.weightLoad += mi.Item.Weight * mi.Amount;
            if ((!mi.Item.StackAble && BackpackManager.Instance.GetItemAmount(mi.Item) - mi.Amount == 0) || mi.Item.StackAble)
                BackpackManager.Instance.MBackpack.backpackSize++;
            BackpackManager.Instance.LoseItem(mi.Item, mi.Amount);
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
        AmountHandler.Instance.Cancel();
        ItemWindowHandler.Instance.CloseItemWindow();
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
