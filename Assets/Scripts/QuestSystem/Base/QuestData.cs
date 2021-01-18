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

    public QuestData(Quest quest)
    {
        Info = quest;
        foreach (CollectObjective co in quest.CollectObjectives)
            if (co.IsValid) ObjectiveInstances.Add(new CollectObjectiveData(co));
        foreach (KillObjective ko in quest.KillObjectives)
            if (ko.IsValid) ObjectiveInstances.Add(new KillObjectiveData(ko));
        foreach (TalkObjective to in quest.TalkObjectives)
            if (to.IsValid) ObjectiveInstances.Add(new TalkObjectiveData(to));
        foreach (MoveObjective mo in quest.MoveObjectives)
            if (mo.IsValid) ObjectiveInstances.Add(new MoveObjectiveData(mo));
        foreach (SubmitObjective so in quest.SubmitObjectives)
            if (so.IsValid) ObjectiveInstances.Add(new SubmitObjectiveData(so));
        foreach (CustomObjective cuo in quest.CustomObjectives)
            if (cuo.IsValid) ObjectiveInstances.Add(new CustomObjectiveData(cuo));
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
                    ObjectiveInstances[i].PrevObjective = ObjectiveInstances[i - 1];
                    ObjectiveInstances[i - 1].NextObjective = ObjectiveInstances[i];
                }
            }
        int i1, i2, i3, i4, i5, i6;
        i1 = i2 = i3 = i4 = i5 = i6 = 0;
        foreach (ObjectiveData o in ObjectiveInstances)
        {
            if (o.Info is CollectObjective)
            {
                o.runtimeID = quest.ID + "_CO" + i1;
                i1++;
            }
            if (o.Info is KillObjective)
            {
                o.runtimeID = quest.ID + "_KO" + i2;
                i2++;
            }
            if (o.Info is TalkObjective)
            {
                o.runtimeID = quest.ID + "_TO" + i3;
                i3++;
            }
            if (o.Info is MoveObjective)
            {
                o.runtimeID = quest.ID + "_MO" + i4;
                i4++;
            }
            if (o.Info is SubmitObjective)
            {
                o.runtimeID = quest.ID + "_SO" + i5;
                i5++;
            }
            if (o.Info is CustomObjective)
            {
                o.runtimeID = quest.ID + "_CUO" + i6;
                i6++;
            }
            o.runtimeParent = this;
        }
    }

    public List<ObjectiveData> ObjectiveInstances { get; } = new List<ObjectiveData>();

    public TalkerData originalQuestHolder;

    public TalkerData currentQuestHolder;

    public bool InProgress { get; set; }//任务是否正在执行，在运行时用到

    public bool IsComplete
    {
        get
        {
            if (ObjectiveInstances.Exists(x => !x.IsComplete))
                return false;
            return true;
        }
    }

    public bool IsFinished
    {
        get
        {
            return IsComplete && !InProgress;
        }
    }

    public static implicit operator bool(QuestData self)
    {
        return self != null;
    }
}