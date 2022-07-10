using System.Collections.Generic;
using ZetanStudio.Collections;
using ZetanStudio.ItemSystem;
using ZetanStudio.ItemSystem.Module;

public static class CraftManager
{
    private static readonly SortedSet<Item> learnedItems = new SortedSet<Item>(Item.Comparer.Default);
    public static ReadOnlySet<Item> LearnedItems => new ReadOnlySet<Item>(learnedItems);

    public static bool Learn(Item item)
    {
        if (!item) return false;
        if (!item.TryGetModule<CraftableModule>(out var craft) || !craft.IsValid)
        {
            MessageManager.Instance.New("无法制作的道具");
            return false;
        }
        if (IsLearned(item))
        {
            ConfirmWindow.StartConfirm("已经学会制作 [" + item.Name + "]，无需再学习。");
            return false;
        }
        learnedItems.Add(item);
        //MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的制作方法!", item.name));
        ConfirmWindow.StartConfirm(string.Format("学会了 [{0}] 的制作方法!", item.Name));
        NotifyCenter.PostNotify(LearnedCraftableItem, item);
        return true;
    }

    public static bool IsLearned(Item item)
    {
        return learnedItems.Contains(item);
    }

    [SaveMethod]
    public static void SaveData(SaveData saveData)
    {
        var learded = new SaveDataItem();
        foreach (var item in learnedItems)
        {
            learded.Write(item.ID);
        }
        saveData["craftData"] = learded;
    }
    [LoadMethod]
    public static void LoadData(SaveData saveData)
    {
        learnedItems.Clear();
        if(saveData.TryReadData("craftData", out var learned))
            foreach (var item in learned.ReadStringList())
            {
                learnedItems.Add(ItemFactory.GetModel(item));
            }
    }

    #region 消息
    public const string LearnedCraftableItem = "LearnedCraftableItem";
    public const string CraftCanceled = "CraftCanceled";
    #endregion
}
