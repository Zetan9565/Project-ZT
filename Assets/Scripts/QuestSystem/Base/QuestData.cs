using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZetanStudio;
using ZetanStudio.CharacterSystem;

namespace ZetanStudio.QuestSystem
{
    public enum QuestState
    {
        NotAccept,
        InProgress,
        Complete,
        Finished
    }

    public class QuestData
    {
        public string Title => Keyword.HandleKeywords(Tr(Model.Title));
        public string Description => Keyword.HandleKeywords(Tr(Model.Description));

        public Quest Model { get; }

        private readonly List<ObjectiveData> objectives = new List<ObjectiveData>();
        private ReadOnlyCollection<ObjectiveData> readOnlyObjectives;
        public ReadOnlyCollection<ObjectiveData> Objectives
        {
            get
            {
                if (readOnlyObjectives == null) readOnlyObjectives = objectives.AsReadOnly();
                return readOnlyObjectives;
            }
        }

        private readonly List<ObjectiveData> ongoingObjectives = new List<ObjectiveData>();
        public ReadOnlyCollection<ObjectiveData> OngoingObjectives => new ReadOnlyCollection<ObjectiveData>(ongoingObjectives);

        public TalkerData originalQuestHolder;

        public TalkerData currentQuestHolder;

        private Action<QuestData, bool> onStateChanged;
        private Action<ObjectiveData, int> onObjectiveAmountChanged;
        private event Action<ObjectiveData, bool> OnObjectiveStateChanged;

        public int latestHandleDays;
        public ReadOnlyCollection<ObjectiveData> CalculateOngoing()
        {
            var oldComplete = IsComplete;
            HashSet<ObjectiveData> ongoingBef = new HashSet<ObjectiveData>(ongoingObjectives);
            ongoingObjectives.Clear();
            foreach (var objective in objectives)
            {
                if (!objective.IsComplete && (!objective.Model.InOrder || objective.AllPrevComplete))
                    ongoingObjectives.Add(objective);
            }
            List<ObjectiveData> changedAfterStart = new List<ObjectiveData>(ongoingObjectives.Count);
            ongoingObjectives.ForEach(o =>
            {
                if (!ongoingBef.Contains(o))
                {
                    int oldAmount = 0;
                    o.Start(onObjectiveAmountChanged, OnObjectiveStateChanged);
                    if (oldAmount != o.CurrentAmount) changedAfterStart.Add(o);
                }
            });
            changedAfterStart.ForEach(o =>
            {
                onObjectiveAmountChanged(o, 0);
                OnObjectiveStateChanged(o, false);
            });
            ongoingObjectives.RemoveAll(x => x.IsComplete);
            if (oldComplete != IsComplete) onStateChanged?.Invoke(this, oldComplete);
            return OngoingObjectives;
        }
        public void OnAccept(Action<QuestData, bool> questStateListener, Action<ObjectiveData, int> objectiveAmountListener, Action<ObjectiveData, bool> objectiveStateListener)
        {
            InProgress = true;
            onStateChanged = questStateListener;
            onObjectiveAmountChanged = objectiveAmountListener;
            OnObjectiveStateChanged = null;
            OnObjectiveStateChanged += ObjectiveStateChanged;
            OnObjectiveStateChanged += objectiveStateListener;
            CalculateOngoing();
        }
        public void OnSubmit(Action<ObjectiveData> objecitveAccesser = null)
        {
            InProgress = false;
            onStateChanged = null;
            onObjectiveAmountChanged = null;
            OnObjectiveStateChanged = null;
            objectives.ForEach(o =>
            {
                o.Submit();
                objecitveAccesser?.Invoke(o);
            });
            ongoingObjectives.Clear();
        }
        public void OnAbandon(Action<ObjectiveData> objecitveAccesser = null)
        {
            InProgress = false;
            onStateChanged = null;
            onObjectiveAmountChanged = null;
            OnObjectiveStateChanged = null;
            objectives.ForEach(o =>
            {
                o.Abandon();
                objecitveAccesser?.Invoke(o);
            });
            ongoingObjectives.Clear();
        }
        public bool InProgress { get; private set; }

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
        public bool IsSubmitted
        {
            get
            {
                return IsComplete && !InProgress;
            }
        }

        private void ObjectiveStateChanged(ObjectiveData objective, bool oldState)
        {
            if (!objective) return;
            CalculateOngoing();
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

        public static implicit operator bool(QuestData obj)
        {
            return obj != null;
        }

        public string Tr(string text)
        {
            return L.Tr(GetType().Name, text);
        }
    }
}