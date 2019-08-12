using System.Collections.Generic;
using UnityEngine;

public delegate void DialogueListener();

[DisallowMultipleComponent]
public class Talker : MonoBehaviour
{
    [SerializeField]
    private TalkerInformation info;
    public TalkerInformation Info
    {
        get
        {
            return info;
        }
    }

    public string TalkerID
    {
        get
        {
            if (info) return info.ID;
            return string.Empty;
        }
    }

    public string TalkerName
    {
        get
        {
            if (info) return info.Name;
            return string.Empty;
        }
    }

    [SerializeField]
    private TalkerData data;
    public TalkerData Data
    {
        get
        {
            return data;
        }
    }

    public List<Quest> QuestInstances
    {
        get
        {
            return data.questInstances;
        }
    }

    public virtual void OnTalkBegin()
    {
        Data.OnTalkBegin();
    }

    public virtual void OnTalkFinished()
    {
        Data.OnTalkFinished();
    }

    public void OnGetGift(ItemBase gift)
    {
        Data.OnGetGift(gift);
    }

    public void Init()
    {
        if (!GameManager.Talkers.ContainsKey(TalkerID)) GameManager.Talkers.Add(TalkerID, this);
        else if (!GameManager.Talkers[TalkerID] || !GameManager.Talkers[TalkerID].gameObject)
        {
            GameManager.Talkers.Remove(TalkerID);
            GameManager.Talkers.Add(TalkerID, this);
        }
        if (!GameManager.TalkerDatas.ContainsKey(TalkerID))
        {
            if (info.IsVendor)
            {
                Data.shop = Instantiate(info.Shop);
                Data.shop.Init();
            }
            Data.info = Info;
            GameManager.TalkerDatas.Add(TalkerID, Data);
            InitQuest(Info.QuestsStored);
        }
        else data = GameManager.TalkerDatas[TalkerID];
        if (info.IsVendor && !ShopManager.Vendors.Contains(Data)) ShopManager.Vendors.Add(Data);
    }

    /// <summary>
    /// 使用任务信息创建任务实例
    /// </summary>
    /// <param name="questsStored">任务信息</param>
    public void InitQuest(List<Quest> questsStored)
    {
        if (questsStored == null) return;
        if (QuestInstances.Count > 0) QuestInstances.Clear();
        foreach (Quest quest in questsStored)
        {
            if (quest)
            {
                Quest questInstances = Instantiate(quest);
                foreach (CollectObjective co in questInstances.CollectObjectives)
                    questInstances.ObjectiveInstances.Add(co);
                foreach (KillObjective ko in questInstances.KillObjectives)
                    questInstances.ObjectiveInstances.Add(ko);
                foreach (TalkObjective to in questInstances.TalkObjectives)
                    questInstances.ObjectiveInstances.Add(to);
                foreach (MoveObjective mo in questInstances.MoveObjectives)
                    questInstances.ObjectiveInstances.Add(mo);
                foreach (CustomObjective cuo in questInstances.CustomObjectives)
                    questInstances.ObjectiveInstances.Add(cuo);
                questInstances.ObjectiveInstances.Sort((x, y) =>
                {
                    if (x.OrderIndex > y.OrderIndex) return 1;
                    else if (x.OrderIndex < y.OrderIndex) return -1;
                    else return 0;
                });
                if (quest.CmpltObjctvInOrder)
                    for (int i = 1; i < questInstances.ObjectiveInstances.Count; i++)
                    {
                        if (questInstances.ObjectiveInstances[i].OrderIndex >= questInstances.ObjectiveInstances[i - 1].OrderIndex)
                        {
                            questInstances.ObjectiveInstances[i].PrevObjective = questInstances.ObjectiveInstances[i - 1];
                            questInstances.ObjectiveInstances[i - 1].NextObjective = questInstances.ObjectiveInstances[i];
                        }
                    }
                int i1, i2, i3, i4, i5;
                i1 = i2 = i3 = i4 = i5 = 0;
                foreach (Objective o in questInstances.ObjectiveInstances)
                {
                    if (o is CollectObjective)
                    {
                        o.runtimeID = questInstances.ID + "_CO" + i1;
                        i1++;
                    }
                    if (o is KillObjective)
                    {
                        o.runtimeID = questInstances.ID + "_KO" + i2;
                        i2++;
                    }
                    if (o is TalkObjective)
                    {
                        o.runtimeID = questInstances.ID + "_TO" + i3;
                        i3++;
                    }
                    if (o is MoveObjective)
                    {
                        o.runtimeID = questInstances.ID + "_MO" + i4;
                        i4++;
                    }
                    if (o is CustomObjective)
                    {
                        o.runtimeID = questInstances.ID + "_CUO" + i5;
                        i5++;
                    }
                    o.runtimeParent = questInstances;
                }
                questInstances.OriginalQuestGiver = data;
                questInstances.CurrentQuestGiver = data;
                QuestInstances.Add(questInstances);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !DialogueManager.Instance.IsTalking)
            DialogueManager.Instance.CanTalk(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && DialogueManager.Instance.CurrentTalker == this)
            DialogueManager.Instance.CannotTalk();
    }*/
}

[System.Serializable]
public class TalkerData
{
    public TalkerInformation info;
    public string TalkerID
    {
        get
        {
            if (info) return info.ID;
            return string.Empty;
        }
    }

    public string TalkerName
    {
        get
        {
            if (info) return info.Name;
            return string.Empty;
        }
    }

    public Relationship relationshipInstance;

    public Warehouse warehouse;

    public ShopInformation shop;

    public List<TalkObjective> objectivesTalkToThis = new List<TalkObjective>();

    public event DialogueListener OnTalkBeginEvent;
    public event DialogueListener OnTalkFinishedEvent;

    public List<Quest> questInstances = new List<Quest>();

    public virtual void OnTalkBegin()
    {
        OnTalkBeginEvent?.Invoke();
    }

    public virtual void OnTalkFinished()
    {
        OnTalkFinishedEvent?.Invoke();
    }

    public void OnGetGift(ItemBase gift)
    {
        if (info.FavoriteItems.Exists(x => x.Item.ID == gift.ID))
        {
            FavoriteItemInfo find = info.FavoriteItems.Find(x => x.Item.ID == gift.ID);
            relationshipInstance.RelationshipValue += (int)find.FavoriteLevel;
        }
        else if (info.HateItems.Exists(x => x.Item.ID == gift.ID))
        {
            HateItemInfo find = info.HateItems.Find(x => x.Item.ID == gift.ID);
            relationshipInstance.RelationshipValue -= (int)find.HateLevel;
        }
        else
        {
            relationshipInstance.RelationshipValue += 5;
        }
    }

    public void TransferQuestToThis(Quest quest)
    {
        if (!quest) return;
        questInstances.Add(quest);
        quest.CurrentQuestGiver.questInstances.Remove(quest);
        quest.CurrentQuestGiver = this;
    }

    public static implicit operator bool(TalkerData self)
    {
        return self != null;
    }
}