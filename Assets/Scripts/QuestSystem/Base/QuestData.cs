using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZetanStudio;

public enum QuestState
{
    NotAccept,
    InProgress,
    Complete,
    Finished
}

public class QuestData
{
    public string Title => MiscFuntion.HandlingKeyWords(Tr(Model.Title));
    public string Description => MiscFuntion.HandlingKeyWords(Tr(Model.Description));

    public Quest Model { get; }

    private List<ObjectiveData> objectives = new List<ObjectiveData>();
    private ReadOnlyCollection<ObjectiveData> readOnlyObjectives;
    public ReadOnlyCollection<ObjectiveData> Objectives
    {
        get
        {
            if (readOnlyObjectives == null) readOnlyObjectives = objectives.AsReadOnly();
            return readOnlyObjectives;
        }
    }

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
            return objectives.TrueForAll(x => x.IsComplete);
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
            objectives.Add(objective.CreateData());
        }
        objectives.Sort((x, y) =>
        {
            if (x.Model.Priority > y.Model.Priority) return 1;
            else if (x.Model.Priority < y.Model.Priority) return -1;
            else return 0;
        });
        if (Model.InOrder)
            for (int i = 1; i < Objectives.Count; i++)
            {
                if (Objectives[i].Model.Priority >= Objectives[i - 1].Model.Priority)
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

    public string Tr(string displayName)
    {
        return LM.Tr(GetType().Name, displayName);
    }
}