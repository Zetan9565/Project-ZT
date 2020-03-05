using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/制作管理器")]
public class MakingManager : WindowHandler<MakingUI, MakingManager>
{
    private readonly List<ItemBase> learnedItems = new List<ItemBase>();

    private readonly List<MakingAgent> makingAgents = new List<MakingAgent>();

    public MakingTool CurrentTool { get; private set; }

    private ItemBase currentItem;

    public bool IsMaking { get; private set; }

    public void Init()
    {
        foreach (MakingAgent ma in makingAgents)
            ma.Clear(true);
        makingAgents.Clear();
        if (CurrentTool)
            foreach (ItemBase item in learnedItems)
                if (item.MakingTool == CurrentTool.ToolType)
                {
                    MakingAgent ma = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<MakingAgent>();
                    ma.Init(item);
                    makingAgents.Add(ma);
                }
    }

    public void MakeCurrent()
    {
        if (!CurrentTool || !currentItem) return;
        if (IsMaking)
        {
            MessageManager.Instance.NewMessage("正在制作中");
            return;
        }
        int amountCanMake = BackpackManager.Instance.GetAmountCanMake(currentItem.Materials);
        if (amountCanMake < 1)
        {
            MessageManager.Instance.NewMessage("材料不足");
            return;
        }
        if (!currentItem.DIYAble)
            if (amountCanMake > 0 && amountCanMake < 2)
            {
                ConfirmManager.Instance.NewConfirm(string.Format("确定制作1个 [{0}] 吗？", currentItem.name), delegate
                {
                    IsMaking = true;
                    PauseDisplay(true);
                    ProgressBar.Instance.NewProgress(CurrentTool.MakingTime, delegate
                    {
                        IsMaking = false;
                        PauseDisplay(false);
                        if (MakeItem(currentItem))
                            MessageManager.Instance.NewMessage(string.Format("制作了1个 [{0}]", currentItem.name));
                    }, null, "制作中");
                });
            }
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
                AmountManager.Instance.NewAmount(delegate
                {
                    ConfirmManager.Instance.NewConfirm(string.Format("确定制作{0}个 [{1}] 吗？", (int)AmountManager.Instance.Amount, currentItem.name), delegate
                    {
                        int amount = (int)AmountManager.Instance.Amount;
                        IsMaking = true;
                        PauseDisplay(true);
                        ProgressBar.Instance.NewProgress(CurrentTool.MakingTime, delegate
                        {
                            IsMaking = false;
                            PauseDisplay(false);
                            if (MakeItem(currentItem, amount))
                                MessageManager.Instance.NewMessage(string.Format("制作了{0}个 [{1}]", currentItem.name, amount));
                        }, null, "制作中");
                    });
                }, amountCanMake);
            }
        else ItemSelectionManager.Instance.StartSelection(ItemSelectionType.Making, "放入一份材料", MakeCurrent);
    }

    private void MakeCurrent(List<ItemInfo> materials)
    {
        int amount = BackpackManager.Instance.GetAmountCanMake(currentItem.Materials, materials);
        if (amount < 1)
        {
            MessageManager.Instance.NewMessage("材料不正确或不足");
            return;
        }
        //Debug.Log("Start making");
        IsMaking = true;
        PauseDisplay(true);
        if (UI.loopToggle.isOn)
            ProgressBar.Instance.NewProgress(CurrentTool.MakingTime, amount,
            delegate
            {
                return !BackpackManager.Instance.CheckMaterialsEnough(currentItem.Materials, materials) && UI.loopToggle.isOn;
            },
            delegate
            {
                //Debug.Log("Break");
                IsMaking = false;
                PauseDisplay(false);
            },
            delegate
            {
                if (!BackpackManager.Instance.CheckMaterialsEnough(currentItem.Materials, materials))
                {
                    //Debug.Log("Failed1");
                    MessageManager.Instance.NewMessage("材料不足，无法继续制作");
                    IsMaking = false;
                    PauseDisplay(false);
                    ProgressBar.Instance.Cancel();
                }
                else if (!MakeItem(currentItem, materials))
                {
                    //Debug.Log("Failed2");
                    IsMaking = false;
                    PauseDisplay(false);
                    ProgressBar.Instance.Cancel();
                }
            },
            delegate
            {
                //Debug.Log("Cancel");
                IsMaking = false;
                PauseDisplay(false);
            }, "制作中");
        else
            ProgressBar.Instance.NewProgress(CurrentTool.MakingTime,
                delegate
                {
                    MakeItem(currentItem, materials);
                    //Debug.Log("Finished");
                    IsMaking = false;
                    PauseDisplay(false);
                    ProgressBar.Instance.Cancel();
                },
                delegate
                {
                    //Debug.Log("Cancel");
                    IsMaking = false;
                    PauseDisplay(false);
                }, "制作中 ");
    }

    public bool Learn(ItemBase item)
    {
        if (!item) return false;
        if (item.MakingMethod == MakingMethod.None || item.Materials.Count < 1)
        {
            MessageManager.Instance.NewMessage("无法制作的道具");
            return false;
        }
        if (learnedItems.Contains(item))
        {
            MessageManager.Instance.NewMessage("已经学会制作 [" + item.name + "]");
            return false;
        }
        learnedItems.Add(item);
        MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的制作方法!", item.name));
        return true;
    }

    public bool HadLearned(ItemBase item)
    {
        return learnedItems.Contains(item) || learnedItems.Exists(x => x.ID == item.ID);
    }

    /// <summary>
    /// 制作道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="itemToMake">目标道具</param>
    /// <param name="amount">制作数量</param>
    /// <returns>是否成功制作</returns>
    private bool MakeItem(ItemBase itemToMake, int amount = 1)
    {
        if (!itemToMake || itemToMake.Materials.Count < 1 || amount < 1) return false;
        if (itemToMake.DIYAble) return false;
        List<ItemInfo> materials = new List<ItemInfo>();
        foreach (var m in itemToMake.Materials)
            materials.Add(new ItemInfo(m.Item, amount * m.Amount));
        return BackpackManager.Instance.GetItem(itemToMake, amount, materials.ToArray());
    }
    /// <summary>
    /// 制作道具，用于根据已知配方自己放入材料来制作道具的玩法
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="materials">放入的材料</param>
    private bool MakeItem(ItemBase item, IEnumerable<ItemInfo> materials)
    {
        if (!item || materials == null || materials.Count() < 1)
        {
            MessageManager.Instance.NewMessage("无效的制作");
            return false;
        }
        if (CheckMaterialsMatch(item.Materials, materials))
        {
            ItemInfo production = new ItemInfo(item);
            Dictionary<ItemInfo, int> itemsToLose = new Dictionary<ItemInfo, int>();
            foreach (var material in item.Materials)
            {
                if (material.MakingType == MakingType.SingleItem)
                {
                    ItemInfo find = materials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return false;//若材料数量不符合，则无法制作
                    //否则加入消耗列表
                    if (find.item.StackAble) //可叠加，则find是克隆出来的新实例
                        find = BackpackManager.Instance.GetItem(find.ItemID);//需从背包中找到相应道具
                    itemsToLose.Add(find, find.Amount);
                }
                else
                {
                    var finds = materials.Where(x => x.item.MaterialType == material.MaterialType);//找到种类相同的道具
                    if (finds.Count() > 0)
                    {
                        if (finds.Select(x => x.Amount).Sum() != material.Amount) return false;//若材料总数不符合，则无法制作
                        foreach (var find in finds)
                        {
                            if (!BackpackManager.Instance.TryLoseItem_Boolean(find.item, find.Amount)) return false;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                            else
                            {
                                //否则加入消耗列表
                                if (find.item.StackAble)//可叠加，则find是克隆出来的新实例
                                    itemsToLose.Add(BackpackManager.Instance.GetItem(find.ItemID), find.Amount);//需从背包中找到相应道具
                                else itemsToLose.Add(find, find.Amount);
                            }
                        }
                    }
                    else return false;//材料不足
                }
            }
            List<ItemInfo> itemsToLoseTemp = new List<ItemInfo>();
            foreach (var kvp in itemsToLose)
            {
                itemsToLoseTemp.Add(new ItemInfo(kvp.Key.item, kvp.Value));
            }
            if (BackpackManager.Instance.TryGetItem_Boolean(production, itemsToLoseTemp.ToArray()))
            {
                BackpackManager.Instance.GetItem(production);
                foreach (var kvp in itemsToLose)//精确到背包中的道具个例
                    BackpackManager.Instance.LoseItem(kvp.Key, kvp.Value);
                MessageManager.Instance.NewMessage(string.Format("生产了 1 个[{0}]", ZetanUtility.ColorText(production.ItemName, GameManager.QualityToColor(production.item.Quality))));
                if (!HadLearned(item)) Learn(item);
                return true;
            }
            else return false;
        }
        else
        {
            MessageManager.Instance.NewMessage("材料不正确(确保只放入了一份)");
            return false;
        }
    }
    /// <summary>
    /// 制作道具，用于自己放入材料来组合新道具的玩法
    /// </summary>
    /// <param name="materials">放入的材料</param>
    private bool MakeItem(IEnumerable<ItemInfo> materials)
    {
        var items = Resources.LoadAll<ItemBase>("").Where(x => x.Makable);
        foreach (var item in items)
            if (CheckMaterialsMatch(item.Materials, materials))
                return MakeItem(item, materials);
        MessageManager.Instance.NewMessage("这样什么也做不了。");
        return false;
    }

    private bool CheckMaterialsMatch(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<ItemInfo> materials)
    {
        if (itemMaterials == null || itemMaterials.Count() < 1 || materials == null || materials.Count() < 1 || itemMaterials.Count() != materials.Count()) return false;
        using (var materialEnum = itemMaterials.GetEnumerator())
            while (materialEnum.MoveNext())
            {
                var material = materialEnum.Current;
                if (material.MakingType == MakingType.SingleItem)
                {
                    var find = materials.FirstOrDefault(x => x.ItemID == material.ItemID);
                    if (!find) return false;//所提供的材料中没有这种材料
                    if (find.Amount != material.Amount) return false;//提供的材料数量不符
                }
                else
                {
                    int amount = materials.Where(x => x.item.MaterialType == material.MaterialType).Select(x => x.Amount).Sum();
                    if (amount != material.Amount) return false;//提供的材料数量不符
                }
            }
        return true;
    }

    #region UI相关
    public override void OpenWindow()
    {
        if (!CurrentTool) return;
        if (IsMaking)
        {
            MessageManager.Instance.NewMessage("正在制作中");
            return;
        }
        base.OpenWindow();
        Init();
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        IsMaking = false;
        currentItem = null;
        HideDescription();
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseWindow();
        if (ItemSelectionManager.Instance.SelectionType == ItemSelectionType.Making) ItemSelectionManager.Instance.CloseWindow();
    }

    public void ShowDescription(ItemBase item)
    {
        if (!item) return;
        currentItem = item;
        List<string> info = BackpackManager.Instance.GetMaterialsInfo(currentItem.Materials).ToList();
        StringBuilder materials = new StringBuilder("<b>持有数量：</b>" + BackpackManager.Instance.GetItemAmount(item));
        materials.Append("\n<b>制作材料：</b>\n");
        for (int i = 0; i < info.Count; i++)
            materials.Append(info[i] + (i == info.Count - 1 ? string.Empty : "\n"));
        UI.description.text = materials.ToString();
        int makeAmount = BackpackManager.Instance.GetAmountCanMake(currentItem.Materials);
        UI.icon.SetItem(new ItemInfo(currentItem, makeAmount));
        UI.makeButton.interactable = makeAmount > 0;
        UI.descriptionWindow.alpha = 1;
        UI.descriptionWindow.blocksRaycasts = true;
        ZetanUtility.SetActive(UI.loopToggle.gameObject, currentItem.DIYAble);
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
        if (IsMaking) ProgressBar.Instance.Cancel();
        CloseWindow();
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
        foreach (MakingAgent ia in makingAgents)
        {
            ZetanUtility.SetActive(ia.gameObject, true);
        }
    }

    private void ShowEquipments()
    {
        foreach (MakingAgent ia in makingAgents)
        {
            if (ia.MItem.IsEquipment)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowConsumables()
    {
        foreach (MakingAgent ia in makingAgents)
        {
            if (ia.MItem.IsConsumable)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }

    private void ShowMaterials()
    {
        foreach (MakingAgent ia in makingAgents)
        {
            if (ia.MItem.IsMaterial)
                ZetanUtility.SetActive(ia.gameObject, true);
            else ZetanUtility.SetActive(ia.gameObject, false);
        }
    }
    #endregion

    public override void SetUI(MakingUI UI)
    {
        makingAgents.RemoveAll(x => !x || !x.gameObject);
        IsPausing = false;
        CloseWindow();
        this.UI = UI;
    }
    #endregion

    public void SaveData(SaveData data)
    {
        foreach (var item in learnedItems)
        {
            data.makingDatas.Add(item.ID);
        }
    }

    public void LoadData(SaveData data)
    {
        learnedItems.Clear();
        foreach (var md in data.makingDatas)
        {
            learnedItems.Add(GameManager.GetItemByID(md));
        }
    }
}
