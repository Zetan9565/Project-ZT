﻿using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Zetan Studio/管理器/制作管理器")]
public class MakingManager : WindowHandler<MakingUI, MakingManager>
{
    private readonly HashSet<ItemBase> learnedItems = new HashSet<ItemBase>();

    private readonly List<MakingAgent> makingAgents = new List<MakingAgent>();

    private ItemBase currentItem;

    private bool bagPausingBef;
    private bool bagOpenBef;

    public MakingTool CurrentTool { get; private set; }

    private MakingToolInformation toolInfo;
    public MakingToolInformation ToolInfo
    {
        get
        {
            if (CurrentTool) return CurrentTool.ToolInfo;
            else return toolInfo;
        }
    }

    public bool IsMaking { get; private set; }

    public void Init()
    {
        foreach (MakingAgent ma in makingAgents)
            ma.Clear(true);
        makingAgents.Clear();
        if (ToolInfo)
            foreach (ItemBase item in learnedItems)
                if (item.MakingTool == ToolInfo.ToolType)
                {
                    MakingAgent ma = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<MakingAgent>();
                    ma.Init(item);
                    makingAgents.Add(ma);
                }
    }

    public void MakeCurrent()
    {
        if (!ToolInfo || !currentItem) return;
        if (IsMaking)
        {
            MessageManager.Instance.New("正在制作中");
            return;
        }
        int amountCanMake = BackpackManager.Instance.GetAmountCanMake(currentItem.Formulation.Materials);
        if (amountCanMake < 1)
        {
            MessageManager.Instance.New("材料不足");
            return;
        }
        if (!currentItem.DIYAble)
        {
            if (amountCanMake > 0 && amountCanMake < 2)
            {
                ConfirmManager.Instance.New(string.Format("确定制作1次 [{0}] 吗？", currentItem.name), delegate
                {
                    IsMaking = true;
                    PauseDisplay(true);
                    ProgressBar.Instance.New(ToolInfo.MakingTime, delegate
                    {
                        IsMaking = false;
                        PauseDisplay(false);
                        int amoutBef = BackpackManager.Instance.GetItemAmount(currentItem.ID);
                        if (MakeItem(currentItem))
                            MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", BackpackManager.Instance.GetItemAmount(currentItem.ID) - amoutBef,
                                ZetanUtility.ColorText(currentItem.name, GameManager.QualityToColor(currentItem.Quality))));
                    },
                    delegate
                    {
                        IsMaking = false;
                        PauseDisplay(false);
                    }, "制作中", true);
                });
            }
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
                AmountManager.Instance.New(delegate (long amount)
                {
                    ConfirmManager.Instance.New(string.Format("确定制作{0}次 [{1}] 吗？", (int)amount, currentItem.name), delegate
                    {
                        int num = (int)amount;
                        IsMaking = true;
                        PauseDisplay(true);
                        ProgressBar.Instance.New(ToolInfo.MakingTime, num - 1,
                            delegate
                            {
                                IsMaking = false;
                                PauseDisplay(false);
                            },
                            delegate
                            {
                                int amoutBef = BackpackManager.Instance.GetItemAmount(currentItem.ID);
                                if (MakeItem(currentItem))
                                    MessageManager.Instance.New(string.Format("制作了 {0} 个 [{1}]", BackpackManager.Instance.GetItemAmount(currentItem.ID) - amoutBef,
                                        ZetanUtility.ColorText(currentItem.name, GameManager.QualityToColor(currentItem.Quality))));
                                else
                                {
                                    IsMaking = false;
                                    PauseDisplay(false);
                                    ProgressBar.Instance.Cancel();
                                }
                            },
                            delegate
                            {
                                IsMaking = false;
                                PauseDisplay(false);
                            }, "制作中", true);
                    });
                }, amountCanMake, "制作次数");
            }
        }
        else
        {
            ItemSelectionManager.Instance.StartSelection(ItemSelectionType.SelectNum, "放入一份材料", delegate (ItemInfo info)
            {
                return info && info.item.MaterialType != MaterialType.None;
            }, MakeCurrent);
        }
    }

    private void MakeCurrent(IEnumerable<ItemSelectionData> materialsRaw)
    {
        //Debug.Log("Start making");
        List<ItemInfoBase> materials = ConvertRawMatList(materialsRaw);
        IsMaking = true;
        PauseDisplay(true);
        if (UI.loopToggle.isOn)
            ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                bool enough = BackpackManager.Instance.IsMaterialsEnough(currentItem.Formulation.Materials, materials);
                if (!enough) MessageManager.Instance.New("材料不足，无法继续制作");
                return !enough && UI.loopToggle.isOn;
            },
            delegate
            {
                //Debug.Log("Break");
                IsMaking = false;
                PauseDisplay(false);
            },
            delegate
            {
                if (!MakeItem(currentItem, materialsRaw))
                {
                    //Debug.Log("Failed3");
                    ProgressBar.Instance.Cancel();
                }
            },
            delegate
            {
                //Debug.Log("Cancel");
                IsMaking = false;
                PauseDisplay(false);
            }, "制作中", true);
        else
            ProgressBar.Instance.New(ToolInfo.MakingTime,
                delegate
                {
                    MakeItem(currentItem, materialsRaw);
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
                }, "制作中 ", true);
    }

    public void DIY()
    {
        bool select(ItemInfo info)
        {
            return info && info.item.MaterialType != MaterialType.None;
        }
        ItemSelectionManager.Instance.StartSelection(ItemSelectionType.SelectNum, "放入一份材料", select, DIYMake);
        HideDescription();
    }
    public void DIYMake(IEnumerable<ItemSelectionData> materialsRaw)
    {
        IsMaking = true;
        PauseDisplay(true);
        ProgressBar.Instance.New(ToolInfo.MakingTime,
            delegate
            {
                MakeItem(materialsRaw);
                //Debug.Log("Finished");
                IsMaking = false;
                PauseDisplay(false);
            },
            delegate
            {
                //Debug.Log("Cancel");
                IsMaking = false;
                PauseDisplay(false);
            }, "制作中", true);
    }

    private static List<ItemInfoBase> ConvertRawMatList(IEnumerable<ItemSelectionData> materialsRaw)
    {
        List<ItemInfoBase> materials = new List<ItemInfoBase>();
        foreach (var isd in materialsRaw)
        {
            materials.Add(new ItemInfo(isd.source.item, isd.amount));
        }

        return materials;
    }

    public bool Learn(ItemBase item)
    {
        if (!item) return false;
        if (item.MakingMethod == MakingMethod.None || !item.Formulation || item.Formulation.Materials.Count < 1)
        {
            MessageManager.Instance.New("无法制作的道具");
            return false;
        }
        if (HadLearned(item))
        {
            ConfirmManager.Instance.New("已经学会制作 [" + item.name + "]，无需再学习。");
            return false;
        }
        learnedItems.Add(item);
        //MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的制作方法!", item.name));
        ConfirmManager.Instance.New(string.Format("学会了 [{0}] 的制作方法!", item.name));
        UpdateUI();
        return true;
    }

    public bool HadLearned(ItemBase item)
    {
        return learnedItems.Contains(item);
    }

    /// <summary>
    /// 制作道具，用于根据已知配方直接消耗材料来制作道具的玩法
    /// </summary>
    /// <param name="itemToMake">目标道具</param>
    /// <param name="amount">制作数量</param>
    /// <returns>是否成功制作</returns>
    private bool MakeItem(ItemBase itemToMake)
    {
        if (!itemToMake || !itemToMake.Formulation || itemToMake.Formulation.Materials.Count < 1) return false;
        if (itemToMake.DIYAble) return false;
        List<ItemSelectionData> materialsRaw = BackpackManager.Instance.GetMaterialsFromBackpack(itemToMake.Formulation.Materials);
        if (BackpackManager.Instance.IsMaterialsEnough(itemToMake.Formulation.Materials, ConvertRawMatList(materialsRaw)))
            return BackpackManager.Instance.GetItem(itemToMake, Random.Range(itemToMake.MinYield, itemToMake.MaxYield + 1), materialsRaw.ToArray());
        else
        {
            MessageManager.Instance.New("材料不足，无法继续制作");
            return false;
        }
    }
    /// <summary>
    /// 制作道具，用于根据已知配方自己放入材料来制作道具的玩法
    /// </summary>
    /// <param name="item">目标道具</param>
    /// <param name="materialsRaw">放入的材料</param>
    private bool MakeItem(ItemBase item, IEnumerable<ItemSelectionData> materialsRaw)
    {
        if (!item || !item.Formulation || materialsRaw == null || materialsRaw.Count() < 1)
        {
            MessageManager.Instance.New("无效的制作");
            return false;
        }
        if (CheckMaterialsMatch(item.Formulation.Materials, ConvertRawMatList(materialsRaw)))
        {
            ItemInfo production = new ItemInfo(item, Random.Range(item.MinYield, item.MaxYield + 1));
            List<ItemSelectionData> itemsToLose = new List<ItemSelectionData>();
            foreach (var material in item.Formulation.Materials)
            {
                if (material.MakingType == MakingType.SingleItem)
                {
                    ItemSelectionData find = materialsRaw.FirstOrDefault(x => x.source.ItemID == material.ItemID);
                    if (!find || find.amount != material.Amount)
                    {
                        MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
                        return false;//所提供的材料中没有这种材料或数量不符合，则无法制作
                    }
                    //否则加入消耗列表
                    else itemsToLose.Add(find);
                }
                else
                {
                    var finds = materialsRaw.Where(x => x.source.item.MaterialType == material.MaterialType);//找到种类相同的道具
                    if (finds.Count() > 0)
                    {
                        if (finds.Select(x => x.amount).Sum() != material.Amount)
                        {
                            MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
                            return false;//若材料总数不符合，则无法制作
                        }
                        foreach (var find in finds)
                        {
                            if (!BackpackManager.Instance.TryLoseItem_Boolean(find.source, find.amount))
                            {
                                MessageManager.Instance.New("材料不足，无法继续制作");
                                return false;//若任意一个相应数量的材料无法失去，则会导致总数量不符合，所以无法制作
                            }
                            else
                            {
                                //否则加入消耗列表
                                itemsToLose.Add(find);
                            }
                        }
                    }
                    else
                    {
                        MessageManager.Instance.New("材料不足，无法继续制作");
                        return false;//材料不足
                    }
                }
            }
            if (BackpackManager.Instance.TryGetItem_Boolean(production, itemsToLose.ToArray()))
            {
                BackpackManager.Instance.GetItem(production);
                foreach (var isd in itemsToLose)//精确到背包中的道具个例
                    BackpackManager.Instance.LoseItem(isd.source, isd.amount);
                MessageManager.Instance.New(string.Format("生产了 {0} 个[{1}]", production.Amount, ZetanUtility.ColorText(production.ItemName, GameManager.QualityToColor(production.item.Quality))));
                if (!HadLearned(item)) Learn(item);
                return true;
            }
            else return false;
        }
        else
        {
            MessageManager.Instance.New("材料不正确(请确保只放入了一份)");
            return false;
        }
    }
    /// <summary>
    /// 制作道具，用于自己放入材料来组合新道具的玩法
    /// </summary>
    /// <param name="materials">放入的材料</param>
    private bool MakeItem(IEnumerable<ItemSelectionData> materialsRaw)
    {
        List<ItemInfoBase> materials = ConvertRawMatList(materialsRaw);
        foreach (var item in learnedItems.Where(x => x.MakingTool == ToolInfo.ToolType))
            if (CheckMaterialsMatch(item.Formulation.Materials, materials))
                return MakeItem(item, materialsRaw);
        var items = Resources.LoadAll<ItemBase>("Configuration").Where(x => x.Makable && x.MakingTool == ToolInfo.ToolType).Except(learnedItems);
        foreach (var item in items)
            if (CheckMaterialsMatch(item.Formulation.Materials, materials))
                return MakeItem(item, materialsRaw);
        MessageManager.Instance.New("这样似乎什么也做不了");
        return false;
    }

    private bool CheckMaterialsMatch(IEnumerable<MaterialInfo> itemMaterials, IEnumerable<ItemInfoBase> materials)
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
        if (!ToolInfo) return;
        base.OpenWindow();
        if (!IsUIOpen) return;
        IsMaking = true;
        Init();
        UI.pageSelector.SetValueWithoutNotify(0);
        SetPage(0);
        bagPausingBef = BackpackManager.Instance.IsPausing;
        bagOpenBef = BackpackManager.Instance.IsUIOpen;
        if (!BackpackManager.Instance.IsUIOpen)
        {
            if (bagPausingBef)
                BackpackManager.Instance.PauseDisplay(false);
            BackpackManager.Instance.OpenWindow();
        }
        if (ItemWindowManager.Instance.IsUIOpen) ItemWindowManager.Instance.CloseWindow();
        BackpackManager.Instance.EnableHandwork(false);
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        IsMaking = false;
        if (CurrentTool) CurrentTool.OnDoneManage();
        CurrentTool = null;
        currentItem = null;
        toolInfo = null;
        HideDescription();
        AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseWindow();
        if (ItemSelectionManager.Instance.IsUIOpen) ItemSelectionManager.Instance.CloseWindow();
        if (bagPausingBef)
            BackpackManager.Instance.PauseDisplay(true);
        else if (!bagOpenBef)
        {
            if (BackpackManager.Instance.IsPausing)
                BackpackManager.Instance.PauseDisplay(false);
            BackpackManager.Instance.CloseWindow();
        }
        bagPausingBef = false;
        bagOpenBef = false;
        BackpackManager.Instance.EnableHandwork(true);
    }

    public override void PauseDisplay(bool pause)
    {
        bool pauseBef = IsPausing;
        base.PauseDisplay(pause);
        if (pauseBef && !IsPausing) BackpackManager.Instance.OpenWindow();
        else if (!pauseBef && IsPausing) BackpackManager.Instance.CloseWindow();
    }

    public void ShowDescription(ItemBase item)
    {
        if (!item) return;
        currentItem = item;
        List<string> info = BackpackManager.Instance.GetMaterialsInfoString(currentItem.Formulation.Materials);
        StringBuilder materials = new StringBuilder("<b>持有数量：</b>" + BackpackManager.Instance.GetItemAmount(item));
        materials.Append("\n<b>制作材料：</b>");
        materials.Append(BackpackManager.Instance.IsMaterialsEnough(item.Formulation.Materials) ? "<color=green>(可制作)</color>\n" : "<color=red>(耗材不足)</color>\n");
        for (int i = 0; i < info.Count; i++)
            materials.Append(info[i] + (i == info.Count - 1 ? string.Empty : "\n"));
        UI.description.text = materials.ToString();
        int makeAmount = BackpackManager.Instance.GetAmountCanMake(currentItem.Formulation.Materials);
        UI.icon.SetItem(new ItemInfo(currentItem, makeAmount));
        UI.nameText.text = item.name;
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

    public bool Make(MakingTool tool)
    {
        if (!tool || tool.ToolInfo.ToolType == MakingToolType.None) return false;
        if (IsMaking)
        {
            MessageManager.Instance.New("正在制作中");
            return false;
        }
        CurrentTool = tool;
        OpenWindow();
        return true;
    }

    public bool Make(MakingToolInformation tool)
    {
        if (!tool || tool.ToolType == MakingToolType.None) return false;
        if (IsMaking)
        {
            MessageManager.Instance.New("正在制作中");
            return false;
        }
        CurrentTool = null;
        toolInfo = tool;
        OpenWindow();
        return true;
    }

    public void CancelMake()
    {
        if (IsMaking) ProgressBar.Instance.CancelWithoutAction();
        PauseDisplay(false);
        UI.window.alpha = 0;
        CloseWindow();
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
        UI.icon.Init(delegate (ItemSlot _)
        {
            return new ButtonWithTextData[] { new ButtonWithTextData("制作", delegate { MakeCurrent(); }) };
        });
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
