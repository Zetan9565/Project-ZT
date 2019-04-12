using System.Collections.Generic;
using UnityEngine;

public delegate void ItemAmountListener(string itemID, int amount);

[DisallowMultipleComponent]
public class BagManager : MonoBehaviour {

    private static BagManager instance;
    public static BagManager Instance
    {
        get
        {
            if (!instance || !instance.gameObject)
                instance = FindObjectOfType<BagManager>();
            return instance;
        }
    }

    public event ItemAmountListener OnGetItemEvent;
    public event ItemAmountListener OnLoseItemEvent;

    private Dictionary<string, List<ItemBase>> items = new Dictionary<string, List<ItemBase>>();
    public Dictionary<string, List<ItemBase>> Items
    {
        get
        {
            return items;
        }
    }

    public bool GetItem(ItemBase item, int amount = 1)
    {
        if (!item) return false;
        int originalAmount = GetItemAmountByID(item.ID);
        for (int i = 0; i < amount; i++)
        {
            if (Items.ContainsKey(item.ID)) Items[item.ID].Add(item);
            else
            {
                Items.Add(item.ID, new List<ItemBase>());
                Items[item.ID].Add(item);
            }
        }
        OnGetItemEvent?.Invoke(item.ID, GetItemAmountByID(item.ID) - originalAmount);
        QuestManager.Instance.UpdateObjectivesUI();
        return true;
    }

    public bool LoseItem(ItemBase item)
    {
        if (!HasItemWithID(item.ID)) return false;
        if (!item || QuestRequiredItem(item.ID, GetItemAmountByID(item.ID) - 1) || GetItemAmountByID(item.ID) < 1) return false;
        items[item.ID].Remove(item);
        if (Items[item.ID].Count <= 0) Items.Remove(item.ID);
        OnLoseItemEvent?.Invoke(item.ID, GetItemAmountByID(item.ID));
        QuestManager.Instance.UpdateObjectivesUI();
        return true;
    }
    public bool LoseItemByID(string itemID, int amount = 1)
    {
        if (!HasItemWithID(itemID)) return false;
        if (itemID == string.Empty || QuestRequiredItem(itemID, GetItemAmountByID(itemID) - amount) || GetItemAmountByID(itemID) < amount) return false;
        for (int i = 0; i < amount; i++)
        {
            Items[itemID].RemoveAt(Items[itemID].Count - 1);
            if (Items[itemID].Count <= 0)
            {
                Items.Remove(itemID);
                break;
            }
        }
        OnLoseItemEvent?.Invoke(itemID, GetItemAmountByID(itemID));
        QuestManager.Instance.UpdateObjectivesUI();
        return true;
    }

    public int GetItemAmountByID(string id)
    {
        if (Items.ContainsKey(id))
        {
            return Items[id].Count;
        }
        return 0;
    }

    public bool HasItemWithID(string id)
    {
        return GetItemAmountByID(id) > 0;
    }

    /// <summary>
    /// 判定是否有某个任务需要某数量的某个道具
    /// </summary>
    /// <param name="itemID">要判定的道具ID</param>
    /// <param name="amount">要判定的数量</param>
    /// <returns>是否需要该道具</returns>
    private bool QuestRequiredItem(string itemID, int amount)
    {
        return QuestManager.Instance.QuestsOngoing.Exists(x => x.RequiredItem(itemID, amount));
    }
}
