﻿using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("ZetanStudio/管理器/拾取管理器")]
public class LootManager : WindowHandler<LootUI, LootManager>
{
    [SerializeField]
    private int slotCount;

    [SerializeField]
    private LootAgent defaultLootPrefab;
    public static LootAgent DefaultLootPrefab => Instance.defaultLootPrefab;

    private readonly List<ItemAgent> itemAgents = new List<ItemAgent>();

    public LootAgent LootAgent { get; private set; }

    public bool PickAble { get; private set; }
    public bool IsPicking { get; private set; }

    private void Init()
    {
        foreach (var ia in itemAgents)
            ia.Empty();
        while (itemAgents.Count < slotCount)
        {
            ItemAgent ia = ObjectPool.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            ia.Init(ItemAgentType.Loot);
            itemAgents.Add(ia);
        }
        while (itemAgents.Count > slotCount)
        {
            itemAgents[itemAgents.Count - 1].Clear(true);
            itemAgents.RemoveAt(itemAgents.Count - 1);
        }
        if (LootAgent)
            foreach (ItemInfo li in LootAgent.lootItems)
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.SetItem(li);
                        break;
                    }
    }

    public void TakeItem(ItemInfo info, bool all = false)
    {
        if (!LootAgent || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseWindow();
        if (!all)
            if (info.Amount == 1) OnTake(info, 1);
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtility.ScreenCenter, Vector2.zero);
                AmountManager.Instance.New(delegate
                {
                    OnTake(info, (int)AmountManager.Instance.Amount);
                }, info.Amount);
            }
        else OnTake(info, info.Amount);
    }

    private void OnTake(ItemInfo item, int amount)
    {
        if (!LootAgent) return;
        if (!LootAgent.lootItems.Contains(item)) return;
        int takeAmount = BackpackManager.Instance.TryGetItem_Integer(item, amount);
        if (BackpackManager.Instance.GetItem(item.item, takeAmount)) item.Amount -= takeAmount;
        if (item.Amount < 1) LootAgent.lootItems.Remove(item);
        ItemAgent ia = GetItemAgentByInfo(item);
        if (ia) ia.UpdateInfo();
        if (LootAgent.lootItems.Count < 1)
        {
            LootAgent.Recycle();
            CloseWindow();
        }
    }

    public void TakeAll()
    {
        if (!LootAgent) return;
        foreach (ItemInfo item in LootAgent.lootItems)
        {
            int takeAmount = BackpackManager.Instance.TryGetItem_Integer(item);
            if (BackpackManager.Instance.GetItem(item.item, takeAmount)) item.Amount -= takeAmount;
            ItemAgent ia = GetItemAgentByInfo(item);
            if (ia) ia.UpdateInfo();
        }
        LootAgent.lootItems.RemoveAll(x => x.Amount < 1);
        if (LootAgent.lootItems.Count < 1)
        {
            LootAgent.Recycle();
            CloseWindow();
        }
    }

    private ItemAgent GetItemAgentByInfo(ItemInfo info)
    {
        return itemAgents.Find(x => x.MItemInfo == info);
    }

    #region UI相关
    public bool Pick(LootAgent lootAgent)
    {
        if(IsPicking)
        {
            MessageManager.Instance.New("请先拾取完上一个物品");
            return false;
        }
        if(GatherManager.Instance.IsGathering)
        {
            MessageManager.Instance.New("请先等待采集完成");
            return false;
        }
        if (!lootAgent) return false;
        LootAgent = lootAgent;
        OpenWindow();
        return true;
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        if (!IsUIOpen) return;
        Init();
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(LootManager), true);
        IsPicking = true;
    }
    public override void CloseWindow()
    {
        base.CloseWindow();
        if (IsUIOpen) return;
        if (LootAgent) LootAgent.FinishInteraction();
        LootAgent = null;
        PickAble = false;
        if (AmountManager.Instance.IsUIOpen) AmountManager.Instance.Cancel();
        ItemWindowManager.Instance.CloseWindow();
        NotifyCenter.Instance.PostNotify(NotifyCenter.CommonKeys.WindowStateChange, typeof(LootManager), false);
        IsPicking = false;
    }
    #endregion
}
