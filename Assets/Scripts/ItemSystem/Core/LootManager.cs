using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : SingletonMonoBehaviour<LootManager>, IWindowHandler
{
    [SerializeField]
    private LootUI UI;

    private List<ItemAgent> itemAgents = new List<ItemAgent>();

    public LootAgent LootAgent { get; private set; }

    public bool IsUIOpen { get; private set; }
    public bool IsPausing { get; private set; }

    public bool PickAble { get; private set; }
    public bool IsPicking { get; private set; }

    public Canvas SortCanvas
    {
        get
        {
            if (!UI) return null;
            return UI.windowCanvas;
        }
    }

    private void Init()
    {
        foreach (var ia in itemAgents)
            ia.Empty();
        int befCount = itemAgents.Count;
        for (int i = 0; i < 10 - befCount; i++)
        {
            ItemAgent ia = ObjectPool.Instance.Get(UI.itemCellPrefab, UI.itemCellsParent).GetComponent<ItemAgent>();
            ia.Init(ItemAgentType.Loot);
            itemAgents.Add(ia);
        }
        if (LootAgent)
            foreach (ItemInfo li in LootAgent.lootItems)
                foreach (ItemAgent ia in itemAgents)
                    if (ia.IsEmpty)
                    {
                        ia.InitItem(li);
                        break;
                    }
    }

    public void TakeItem(ItemInfo info, bool all = false)
    {
        if (!LootAgent || info == null || !info.item) return;
        ItemWindowManager.Instance.CloseItemWindow();
        if (!all)
            if (info.Amount == 1) OnTake(info, 1);
            else
            {
                AmountManager.Instance.SetPosition(ZetanUtilities.ScreenCenter, Vector2.zero);
                AmountManager.Instance.NewAmount(delegate
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
        int takeAmount = BackpackManager.Instance.TryGetItem_Integer(item.item, amount);
        if (BackpackManager.Instance.GetItem(item.item, takeAmount))
            item.Amount -= takeAmount;
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
            if (BackpackManager.Instance.GetItem(item.item, takeAmount))
                item.Amount -= takeAmount;
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
    public void CanPick(LootAgent lootAgent)
    {
        if (!lootAgent) return;
        LootAgent = lootAgent;
        UIManager.Instance.EnableInteractive(true, LootAgent.name);
        PickAble = true;
    }
    public void CannotPick()
    {
        LootAgent = null;
        UIManager.Instance.EnableInteractive(false);
        PickAble = false;
        CloseWindow();
    }

    public void OpenWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (IsUIOpen) return;
        if (IsPausing) return;
        Init();
        UI.lootWindow.alpha = 1;
        UI.lootWindow.blocksRaycasts = true;
        IsUIOpen = true;
        WindowsManager.Instance.Push(this);
        IsPicking = true;
        UIManager.Instance.EnableInteractive(false);
    }
    public void CloseWindow()
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing) return;
        UI.lootWindow.alpha = 0;
        UI.lootWindow.blocksRaycasts = false;
        IsUIOpen = false;
        WindowsManager.Instance.Remove(this);
        IsPicking = false;
        CannotPick();
        if (AmountManager.Instance.IsUIOpen) AmountManager.Instance.Cancel();
    }
    void IWindowHandler.OpenCloseWindow() { }
    public void PauseDisplay(bool pause)
    {
        if (!UI || !UI.gameObject) return;
        if (!IsUIOpen) return;
        if (IsPausing && !pause)
        {
            UI.lootWindow.alpha = 1;
            UI.lootWindow.blocksRaycasts = true;
        }
        else if (!IsPausing && pause)
        {
            UI.lootWindow.alpha = 0;
            UI.lootWindow.blocksRaycasts = false;
        }
        IsPausing = pause;
    }
    #endregion
}
