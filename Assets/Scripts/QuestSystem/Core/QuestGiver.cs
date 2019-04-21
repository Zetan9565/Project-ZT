using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestGiver : Talker {

    [SerializeField]
    private List<Quest> questsStored;
    public List<Quest> QuestsStored
    {
        get
        {
            return questsStored;
        }
    }

    [SerializeField]
    private List<Quest> questInstances = new List<Quest>();
    public List<Quest> QuestInstances
    {
        get
        {
            return questInstances;
        }
        private set
        {
            questInstances = value;
        }
    }

    private List<QuestGroup> questGroupInstances = new List<QuestGroup>();

    public void Init()
    {
        InitQuest(questsStored);
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
                Quest tempq = Instantiate(quest);
                foreach (CollectObjective co in tempq.CollectObjectives)
                    tempq.Objectives.Add(co);
                foreach (KillObjective ko in tempq.KillObjectives)
                    tempq.Objectives.Add(ko);
                foreach (TalkObjective to in tempq.TalkObjectives)
                    tempq.Objectives.Add(to);
                foreach (MoveObjective mo in tempq.MoveObjectives)
                    tempq.Objectives.Add(mo);
                if (tempq.CmpltObjctvInOrder)
                {
                    tempq.Objectives.Sort((x, y) =>
                    {
                        if (x.OrderIndex > y.OrderIndex) return 1;
                        else if (x.OrderIndex < y.OrderIndex) return -1;
                        else return 0;
                    });
                    for (int i = 1; i < tempq.Objectives.Count; i++)
                    {
                        if (tempq.Objectives[i].OrderIndex >= tempq.Objectives[i - 1].OrderIndex)
                        {
                            tempq.Objectives[i].PrevObjective = tempq.Objectives[i - 1];
                            tempq.Objectives[i - 1].NextObjective = tempq.Objectives[i];
                        }
                    }
                }
                int i1, i2, i3, i4;
                i1 = i2 = i3 = i4 = 0;
                foreach(Objective o in tempq.Objectives)
                {
                    if (o is CollectObjective)
                    {
                        o.runtimeID = tempq.ID + "_CO" + i1;
                        i1++;
                    }
                    if (o is KillObjective)
                    {
                        o.runtimeID = tempq.ID + "_KO" + i2;
                        i2++;
                    }
                    if (o is TalkObjective)
                    {
                        o.runtimeID = tempq.ID + "_TO" + i3;
                        i3++;
                    }
                    if (o is MoveObjective)
                    {
                        o.runtimeID = tempq.ID + "_MO" + i4;
                        i4++;
                    }
                    o.runtimeParent = tempq;
                }
                tempq.OriginalQuestGiver = this;
                tempq.CurrentQuestGiver = this;
                QuestInstances.Add(tempq);
            }
        }
    }

    /// <summary>
    /// 向此对象交接任务。因为往往会有些任务不在同一个NPC接取并完成，所以就要在两个NPC之间交接该任务
    /// </summary>
    /// <param name="quest">需要进行交接的任务</param>
    public void TransferQuestToThis(Quest quest)
    {
        if (!quest) return;
        QuestInstances.Add(quest);
        quest.CurrentQuestGiver.QuestInstances.Remove(quest);
        quest.CurrentQuestGiver = this;
    }
}