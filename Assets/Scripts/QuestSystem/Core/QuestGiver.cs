using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class QuestGiver : Talker
{
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
                Quest qstinstns = Instantiate(quest);
                foreach (CollectObjective co in qstinstns.CollectObjectives)
                    qstinstns.Objectives.Add(co);
                foreach (KillObjective ko in qstinstns.KillObjectives)
                    qstinstns.Objectives.Add(ko);
                foreach (TalkObjective to in qstinstns.TalkObjectives)
                    qstinstns.Objectives.Add(to);
                foreach (MoveObjective mo in qstinstns.MoveObjectives)
                    qstinstns.Objectives.Add(mo);
                foreach (CustomObjective cuo in qstinstns.CustomObjectives)
                    qstinstns.Objectives.Add(cuo);
                if (qstinstns.CmpltObjctvInOrder)
                {
                    qstinstns.Objectives.Sort((x, y) =>
                    {
                        if (x.OrderIndex > y.OrderIndex) return 1;
                        else if (x.OrderIndex < y.OrderIndex) return -1;
                        else return 0;
                    });
                    for (int i = 1; i < qstinstns.Objectives.Count; i++)
                    {
                        if (qstinstns.Objectives[i].OrderIndex >= qstinstns.Objectives[i - 1].OrderIndex)
                        {
                            qstinstns.Objectives[i].PrevObjective = qstinstns.Objectives[i - 1];
                            qstinstns.Objectives[i - 1].NextObjective = qstinstns.Objectives[i];
                        }
                    }
                }
                int i1, i2, i3, i4, i5;
                i1 = i2 = i3 = i4 = i5 = 0;
                foreach (Objective o in qstinstns.Objectives)
                {
                    if (o is CollectObjective)
                    {
                        o.runtimeID = qstinstns.ID + "_CO" + i1;
                        i1++;
                    }
                    if (o is KillObjective)
                    {
                        o.runtimeID = qstinstns.ID + "_KO" + i2;
                        i2++;
                    }
                    if (o is TalkObjective)
                    {
                        o.runtimeID = qstinstns.ID + "_TO" + i3;
                        i3++;
                    }
                    if (o is MoveObjective)
                    {
                        o.runtimeID = qstinstns.ID + "_MO" + i4;
                        i4++;
                    }
                    if (o is CustomObjective)
                    {
                        o.runtimeID = qstinstns.ID + "_CUO" + i5;
                        i5++;
                    }
                    o.runtimeParent = qstinstns;
                }
                qstinstns.OriginalQuestGiver = this;
                qstinstns.CurrentQuestGiver = this;
                QuestInstances.Add(qstinstns);
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