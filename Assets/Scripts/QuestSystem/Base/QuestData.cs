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
    public Quest Model { get; }

    public List<ObjectiveData> Objectives { get; } = new List<ObjectiveData>();

    public TalkerData originalQuestHolder;

    public TalkerData currentQuestHolder;

    public int latestHandleDays;

    public bool InProgress { get; set; }

    /// <summary>
    /// 目标是否都已达成
    /// </summary>
    public bool IsComplete
    {
        get
        {
            return Objectives.TrueForAll(x => x.IsComplete);
        }
    }

    /// <summary>
    /// 是否已经提交
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
        Model = quest;
        foreach (Objective objective in Model.Objectives)
        {
            if (objective is CollectObjective co)
            { if (co.IsValid) Objectives.Add(new CollectObjectiveData(co)); }
            else if (objective is KillObjective ko)
            { if (ko.IsValid) Objectives.Add(new KillObjectiveData(ko)); }
            else if (objective is TalkObjective to)
            { if (to.IsValid) Objectives.Add(new TalkObjectiveData(to)); }
            else if (objective is MoveObjective mo)
            { if (mo.IsValid) Objectives.Add(new MoveObjectiveData(mo)); }
            else if (objective is SubmitObjective so)
            { if (so.IsValid) Objectives.Add(new SubmitObjectiveData(so)); }
            else if (objective is TriggerObjective tgo)
            { if (tgo.IsValid) Objectives.Add(new TriggerObjectiveData(tgo)); }
        }
        Objectives.Sort((x, y) =>
        {
            if (x.Model.OrderIndex > y.Model.OrderIndex) return 1;
            else if (x.Model.OrderIndex < y.Model.OrderIndex) return -1;
            else return 0;
        });
        if (this.Model.CmpltObjctvInOrder)
            for (int i = 1; i < Objectives.Count; i++)
            {
                if (Objectives[i].Model.OrderIndex >= Objectives[i - 1].Model.OrderIndex)
                {
                    Objectives[i].prevObjective = Objectives[i - 1];
                    Objectives[i - 1].nextObjective = Objectives[i];
                }
            }
        int i1, i2, i3, i4, i5, i6;
        i1 = i2 = i3 = i4 = i5 = i6 = 0;
        foreach (ObjectiveData o in Objectives)
        {
            if (o.Model is CollectObjective)
            {
                o.ID = quest.ID + "_CO" + i1;
                i1++;
            }
            if (o.Model is KillObjective)
            {
                o.ID = quest.ID + "_KO" + i2;
                i2++;
            }
            if (o.Model is TalkObjective)
            {
                o.ID = quest.ID + "_TO" + i3;
                i3++;
            }
            if (o.Model is MoveObjective)
            {
                o.ID = quest.ID + "_MO" + i4;
                i4++;
            }
            if (o.Model is SubmitObjective)
            {
                o.ID = quest.ID + "_SO" + i5;
                i5++;
            }
            if (o.Model is TriggerObjective)
            {
                o.ID = quest.ID + "_CUO" + i6;
                i6++;
            }
            o.parent = this;
        }
    }

    public static implicit operator bool(QuestData self)
    {
        return self != null;
    }
}