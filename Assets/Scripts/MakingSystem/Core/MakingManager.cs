using System.Collections.Generic;
using ZetanCollections;
using ZetanStudio.Item;

public class MakingManager : SingletonMonoBehaviour<MakingManager>, ISaveLoad
{
    private readonly SortedSet<ItemBase> learnedItems = new SortedSet<ItemBase>(ItemComparer.Default);
    public ReadOnlySet<ItemBase> readOnlyLearnedItems;
    public ReadOnlySet<ItemBase> LearnedItems
    {
        get
        {
            if (readOnlyLearnedItems == null) readOnlyLearnedItems = new ReadOnlySet<ItemBase>(learnedItems);
            return readOnlyLearnedItems;
        }
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
            ConfirmWindow.StartConfirm("已经学会制作 [" + item.Name + "]，无需再学习。");
            return false;
        }
        LearnedItems.Add(item);
        //MessageManager.Instance.NewMessage(string.Format("学会了 [{0}] 的制作方法!", item.name));
        ConfirmWindow.StartConfirm(string.Format("学会了 [{0}] 的制作方法!", item.Name));
        NotifyCenter.PostNotify(LearnedNewMakingItem, item);
        return true;
    }

    public bool Learn(ItemNew item)
    {
        return true;
    }

    public bool HadLearned(ItemBase item)
    {
        return LearnedItems.Contains(item);
    }

    public void SaveData(SaveData data)
    {
        foreach (var item in LearnedItems)
        {
            data.makingDatas.Add(item.ID);
        }
    }

    public void LoadData(SaveData data)
    {
        LearnedItems.Clear();
        foreach (var md in data.makingDatas)
        {
            LearnedItems.Add(ItemUtility.GetItemByID(md));
        }
    }

    #region 消息
    public const string LearnedNewMakingItem = "LearnedNewMakingItem";
    public const string MakingCanceled = "MakingCanceled";
    #endregion
}