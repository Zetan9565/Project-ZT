using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.Collections;
using ZetanStudio.Item;
using ZetanStudio.Item.Module;

public class CraftManager : SingletonMonoBehaviour<CraftManager>, ISaveLoad
{
    private readonly SortedSet<Item> learnedItems = new SortedSet<Item>(Item.Comparer.Default);
    public ReadOnlySet<Item> readOnlyLearnedItems;
    public ReadOnlySet<Item> LearnedItems
    {
        get
        {
            if (readOnlyLearnedItems == null) readOnlyLearnedItems = new ReadOnlySet<Item>(learnedItems);
            return readOnlyLearnedItems;
        }
    }
    public bool Learn(Item item)
    {
        if (!item) return false;
        if (item.GetModule<CraftableModule>() is not CraftableModule craft || !craft.IsValid)
        {
            MessageManager.Instance.New("无法制作的道具");
            return false;
        }
        if (HadLearned(item))
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

    public bool HadLearned(Item item)
    {
        return learnedItems.Contains(item);
    }

    public void SaveData(SaveData data)
    {
        foreach (var item in learnedItems)
        {
            data.craftDatas.Add(item.ID);
        }
    }

    public void LoadData(SaveData data)
    {
        learnedItems.Clear();
        foreach (var md in data.craftDatas)
        {
            learnedItems.Add(ItemUtility.GetItemByID(md));
        }
    }

    #region 消息
    public const string LearnedCraftableItem = "LearnedCraftableItem";
    public const string CraftCanceled = "CraftCanceled";
    #endregion
}
namespace ZetanStudio.Item.Craft
{
    public enum CraftType
    {
        [InspectorName("单种道具")]
        SingleItem,//单种道具

        [InspectorName("同类道具")]
        SameType//同类道具
    }
}