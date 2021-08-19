using System.Collections.Generic;

public enum QuestState
{
    NotAccept,
    InProgress,
    Complete,
    Finished
}

public class QuestData
{
    public Quest Info { get; }

    public List<ObjectiveData> ObjectiveInstances { get; } = new List<ObjectiveData>();

    public TalkerData originalQuestHolder;

    public TalkerData currentQuestHolder;

    public int latestHandleDays;

    public bool InProgress { get; set; }//任务是否正在执行，在运行时用到

    /// <summary>
    /// 所以目标是否都已达成
    /// </summary>
    public bool IsComplete
    {
        get
        {
            if (ObjectiveInstances.Exists(x => !x.IsComplete))
                return false;
            return true;
        }
    }

    /// <summary>
    /// 是否已经提交完成
    /// </summary>
    public bool IsFinished
    {
        get
        {
            return IsComplete && !InProgress;
        }
    }

    public QuestData(Quest quest)
    {
        Info = quest;
        foreach (Objective objective in Info.Objectives)
        {
            if (objective is CollectObjective co)
            { if (co.IsValid) ObjectiveInstances.Add(new CollectObjectiveData(co)); }
            else if (objective is KillObjective ko)
            { if (ko.IsValid) ObjectiveInstances.Add(new KillObjectiveData(ko)); }
            else if (objective is TalkObjective to)
            { if (to.IsValid) ObjectiveInstances.Add(new TalkObjectiveData(to)); }
            else if (objective is MoveObjective mo)
            { if (mo.IsValid) ObjectiveInstances.Add(new MoveObjectiveData(mo)); }
            else if (objective is SubmitObjective so)
            { if (so.IsValid) ObjectiveInstances.Add(new SubmitObjectiveData(so)); }
            else if (objective is TriggerObjective tgo)
            { if (tgo.IsValid) ObjectiveInstances.Add(new TriggerObjectiveData(tgo)); }
        }
        ObjectiveInstances.Sort((x, y) =>
        {
            if (x.Info.OrderIndex > y.Info.OrderIndex) return 1;
            else if (x.Info.OrderIndex < y.Info.OrderIndex) return -1;
            else return 0;
        });
        if (this.Info.CmpltObjctvInOrder)
            for (int i = 1; i < ObjectiveInstances.Count; i++)
            {
                if (ObjectiveInstances[i].Info.OrderIndex >= ObjectiveInstances[i - 1].Info.OrderIndex)
                {
                    ObjectiveInstances[i].prevObjective = ObjectiveInstances[i - 1];
                    ObjectiveInstances[i - 1].nextObjective = ObjectiveInstances[i];
                }
            }
        int i1, i2, i3, i4, i5, i6;
        i1 = i2 = i3 = i4 = i5 = i6 = 0;
        foreach (ObjectiveData o in ObjectiveInstances)
        {
            if (o.Info is CollectObjective)
            {
                o.entityID = quest.ID + "_CO" + i1;
                i1++;
            }
            if (o.Info is KillObjective)
            {
                o.entityID = quest.ID + "_KO" + i2;
                i2++;
            }
            if (o.Info is TalkObjective)
            {
                o.entityID = quest.ID + "_TO" + i3;
                i3++;
            }
            if (o.Info is MoveObjective)
            {
                o.entityID = quest.ID + "_MO" + i4;
                i4++;
            }
            if (o.Info is SubmitObjective)
            {
                o.entityID = quest.ID + "_SO" + i5;
                i5++;
            }
            if (o.Info is TriggerObjective)
            {
                o.entityID = quest.ID + "_CUO" + i6;
                i6++;
            }
            o.runtimeParent = this;
        }
    }

    public static implicit operator bool(QuestData self)
    {
        return self != null;
    }
}