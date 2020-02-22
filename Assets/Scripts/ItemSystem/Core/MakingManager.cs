using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/制作管理器")]
public class MakingManager : SingletonMonoBehaviour<MakingManager>, IWindowHandler
{
    [SerializeField]
    private MakingUI UI;

    public bool IsUIOpen { get; private set; }

    public bool IsPausing { get; private set; }

    public Canvas CanvasToSort
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
                if (BackpackManager.Instance.MakeItem(currentItem))
                    MessageManager.Instance.NewMessage(string.Format("制作了1个 [{0}]", currentItem.name));
            });
        }
        else
        {
            AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
            AmountManager.Instance.NewAmount(delegate
            {
                ConfirmManager.Instance.NewConfirm(string.Format("确定制作{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount, currentItem.name), delegate
                {
                    if (BackpackManager.Instance.MakeItem(currentItem, (int)AmountManager.Instance.Amount))
                        MessageManager.Instance.NewMessage(string.Format("制作了{0}个 [{1}]", currentItem.name, (int)AmountManager.Instance.Amount));
                });
            }, amountCanMake);
        }
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
        UI.window.alpha = 1;
        UI.window.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
    }

    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.window.alpha = 0;
        UI.window.blocksRaycasts = false;
        IsUIOpen = false;
        WindowsManager.Instance.Remove(this);
        CurrentTool = null;
        currentItem = null;
        HideDescription();
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseItemWindow();
    }

    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.window.alpha = 1;
            UI.window.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.window.alpha = 0;
            UI.window.blocksRaycasts = false;
        }
        IsPausing = pause;
    }

    public void ShowDescription(ItemBase item)
    {
        if (!item) return;
        currentItem = item;
        List<string> info = currentItem.GetMaterialsInfo(BackpackManager.Instance.MBackpack).ToList();
        StringBuilder materials = new StringBuilder("<b>持有数量：</b>" + BackpackManager.Instance.GetItemAmount(item));
        materials.Append("\n<b>制作材料：</b>\n");
        for (int i = 0; i < info.Count; i++)
            materials.Append(info[i] + (i == info.Count - 1 ? string.Empty : "\n"));
        UI.description.text = materials.ToString();
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
        CurrentTool = null;
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
            ZetanUtility.SetActive(ia.gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsEquipment)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsConsumable)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        foreach (MakingAgent ia in MakingAgents)
        {
            if (ia.MItem.IsMaterial)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }
    #endregion

    public void SetUI(MakingUI UI)
    {
        MakingAgents.RemoveAll(x => !x || !x.gameObject);
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion
}
