using System;
using System.Collections.Generic;
using System.Linq;
using ZetanStudio;
using ZetanStudio.DialogueSystem;
using ZetanStudio.InventorySystem;
using ZetanStudio.ItemSystem;

namespace ZetanStudio.QuestSystem
{
    using SavingSystem;
    using TriggerSystem;
    using ZetanStudio.CharacterSystem;

    public abstract class ObjectiveData
    {
        public Objective Model { get; }

        public string DisplayName => Keyword.HandleKeywords(parent.Tr(Model.DisplayName));

        public T GetInfo<T>() where T : Objective
        {
            return Model as T;
        }

        public ObjectiveData(Objective objective)
        {
            Model = objective;
        }

        protected int currentAmount;
        public int CurrentAmount
        {
            get
            {
                return currentAmount;
            }
            set
            {
                bool befCmplt = IsComplete;
                int befAmount = currentAmount;
                if (value < Model.Amount && value >= 0) currentAmount = value;
                else if (value < 0) currentAmount = 0;
                else currentAmount = Model.Amount;
                if (befAmount != currentAmount) OnAmountChanged?.Invoke(this, befAmount);
                if (befCmplt != IsComplete) OnStateChanged?.Invoke(this, befCmplt);
            }
        }

        public string AmountString => Model.ShowAmount ? $"{CurrentAmount}/{Model.Amount}" : string.Empty;

        public bool IsComplete
        {
            get
            {
                return currentAmount >= Model.Amount;
            }
        }

        public ObjectiveData prevObjective;
        public ObjectiveData nextObjective;

        public string ID;

        public QuestData parent;

        public event Action<ObjectiveData, int> OnAmountChanged;
        public event Action<ObjectiveData, bool> OnStateChanged;

        protected virtual void UpdateAmountUp(int amount = 1)
        {
            if (IsComplete) return;
            if (!Model.InOrder) CurrentAmount += amount;
            else if (AllPrevComplete) CurrentAmount += amount;
        }

        /// <summary>
        /// 判定所有前置目标是否都完成
        /// </summary>
        public bool AllPrevComplete
        {
            get
            {
                ObjectiveData tempObj = prevObjective;
                while (tempObj != null)
                {
                    if (!tempObj.IsComplete && tempObj.Model.Priority < Model.Priority)
                    {
                        return false;
                    }
                    tempObj = tempObj.prevObjective;
                }
                return true;
            }
        }
        /// <summary>
        /// 判定是否有后置目标正在进行
        /// </summary>
        public bool AnyNextOngoing
        {
            get
            {
                ObjectiveData tempObj = nextObjective;
                while (tempObj != null)
                {
                    if (tempObj.CurrentAmount > 0 && tempObj.Model.Priority > Model.Priority)
                    {
                        return true;
                    }
                    tempObj = tempObj.nextObjective;
                }
                return false;
            }
        }

        /// <summary>
        /// 可并行？
        /// </summary>
        public bool Parallel
        {
            get
            {
                if (!Model.InOrder) return true;//不按顺序，说明可以并行执行
                if (!prevObjective || prevObjective.Model.Priority == Model.Priority) return true;//没有有前置目标，或者顺序码与前置目标相同，说明可以并行执行
                if (!nextObjective || nextObjective.Model.Priority == Model.Priority) return true;//没有有后置目标，或者顺序码与后置目标相同，说明可以并行执行
                return false;
            }
        }
        public bool CanParallelWith(ObjectiveData other)
        {
            if (!other || !other.Model.InOrder || !Model.InOrder) return true;
            else if (other.Model.Priority == Model.Priority) return true;
            return false;
        }
        public void Start(Action<ObjectiveData, int> onAmountChanged, Action<ObjectiveData, bool> onStateChanged)
        {
            OnAmountChanged = onAmountChanged;
            OnStateChanged = onStateChanged;
            OnStart();
        }
        protected abstract void OnStart();
        public void Submit()
        {
            OnStateChanged = null;
            OnSubmit();
        }
        protected abstract void OnSubmit();
        public void Abandon()
        {
            currentAmount = 0;
            OnAbandon();
        }
        protected abstract void OnAbandon();

        public static implicit operator bool(ObjectiveData self)
        {
            return self != null;
        }

        public override string ToString()
        {
            return $"{DisplayName}{(Model.ShowAmount ? $" [{currentAmount}/{Model.Amount}]" : string.Empty)}";
        }
    }

    public abstract class ObjectiveData<T> : ObjectiveData where T : Objective
    {
        public new T Model => base.Model as T;

        protected ObjectiveData(T objective) : base(objective) { }
    }

    public class CollectObjectiveData : ObjectiveData<CollectObjective>
    {
        public CollectObjectiveData(CollectObjective objective) : base(objective) { }

        public int amountWhenStart;

        protected override void OnStart()
        {
            BackpackManager.Instance.Inventory.OnItemAmountChanged += UpdateCollectAmount;
            if (AllPrevComplete)
            {
                if (Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) currentAmount = BackpackManager.Instance.GetAmount(Model.ItemToCollect);
                else if (!Model.CheckBagAtStart && !SaveManager.Instance.IsLoading) amountWhenStart = BackpackManager.Instance.GetAmount(Model.ItemToCollect);
            }
        }

        protected override void OnSubmit()
        {
            BackpackManager.Instance.Inventory.OnItemAmountChanged -= UpdateCollectAmount;
            if (!SaveManager.Instance.IsLoading && Model.LoseItemAtSbmt) BackpackManager.Instance.Lose(Model.ItemToCollect, Model.Amount);
        }

        protected override void OnAbandon()
        {
            BackpackManager.Instance.Inventory.OnItemAmountChanged -= UpdateCollectAmount;
        }

        public void UpdateCollectAmount(Item model, int oldAmount, int newAmount)
        {
            if (IsComplete) return;
            if (model == Model.ItemToCollect)
            {
                if (oldAmount < newAmount)//获得道具
                {
                    if (!Model.InOrder || AllPrevComplete) CurrentAmount = newAmount - (!Model.CheckBagAtStart ? amountWhenStart : 0);
                }
                else if (Model.LoseItemAtSbmt)//失去道具，且在提交任务时要上交此目标收集的道具
                {
                    if (AllPrevComplete && !AnyNextOngoing)//前置目标都完成且没有后置目标在进行时，才允许更新
                        CurrentAmount = newAmount;
                }
            }
        }
    }

    public class KillObjectiveData : ObjectiveData<KillObjective>
    {
        public KillObjectiveData(KillObjective objective) : base(objective) { }

        protected override void OnStart()
        {
            switch (Model.KillType)
            {
                case KillObjectiveType.Specific:
                    GameManager.Enemies[Model.Enemy.ID].ForEach(e => e.OnDeathEvent += UpdateKillAmount);
                    break;
                case KillObjectiveType.Race:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == Model.Race))
                        enemies.ForEach(e => e.OnDeathEvent += UpdateKillAmount);
                    break;
                case KillObjectiveType.Group:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && Model.Group.Contains(x[0].Info.ID)))
                    {
                        enemies.ForEach(e => e.OnDeathEvent += UpdateKillAmount);
                    }
                    break;
                case KillObjectiveType.Any:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Select(x => x.Value))
                        enemies.ForEach(e => e.OnDeathEvent += UpdateKillAmount);
                    break;
            }
        }

        protected override void OnSubmit()
        {
            switch (Model.KillType)
            {
                case KillObjectiveType.Specific:
                    GameManager.Enemies[Model.Enemy.ID].ForEach(e => e.OnDeathEvent -= UpdateKillAmount);
                    break;
                case KillObjectiveType.Race:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && x[0].Info.Race && x[0].Info.Race == Model.Race))
                    {
                        enemies.ForEach(e => e.OnDeathEvent -= UpdateKillAmount);
                    }
                    break;
                case KillObjectiveType.Group:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Values.Where(x => x.Count > 0 && Model.Group.Contains(x[0].Info.ID)))
                    {
                        enemies.ForEach(e => e.OnDeathEvent -= UpdateKillAmount);
                    }
                    break;
                case KillObjectiveType.Any:
                    foreach (List<Enemy> enemies in GameManager.Enemies.Values)
                        enemies.ForEach(e => e.OnDeathEvent -= UpdateKillAmount);
                    break;
            }
        }

        protected override void OnAbandon()
        {
            OnSubmit();
        }

        public void UpdateKillAmount()
        {
            UpdateAmountUp();
        }
    }

    public class TalkObjectiveData : ObjectiveData<TalkObjective>
    {
        public TalkObjectiveData(TalkObjective objective) : base(objective) { }

        protected override void OnStart()
        {
            if (!IsComplete)
            {
                var talker = DialogueManager.Talkers[Model.NPCToTalk.ID];
                talker.objectivesTalkToThis.Add(this);
                OnStateChanged += talker.TryRemoveObjective;
            }
        }
        protected override void OnSubmit()
        {
            var talker = DialogueManager.Talkers[Model.NPCToTalk.ID];
            talker.objectivesTalkToThis.Remove(this);
            OnStateChanged -= talker.TryRemoveObjective;
        }

        protected override void OnAbandon()
        {
            OnSubmit();
            DialogueManager.RemoveData(Model.Dialogue.Entry);
        }

        public void UpdateTalkState()
        {
            UpdateAmountUp();
        }
    }

    public class MoveObjectiveData : ObjectiveData<MoveObjective>
    {
        public CheckPointData targetPoint;

        public MoveObjectiveData(MoveObjective objective) : base(objective) { }

        protected override void OnStart()
        {
            targetPoint = CheckPointManager.CreateCheckPoint(Model.AuxiliaryPos, UpdateMoveState);
        }
        protected override void OnSubmit()
        {
            targetPoint = null;
            CheckPointManager.RemoveCheckPointListener(Model.AuxiliaryPos, UpdateMoveState);
        }
        protected override void OnAbandon()
        {
            OnSubmit();
        }
        public void UpdateMoveState(CheckPointInformation point)
        {
            if (point == Model.AuxiliaryPos) UpdateAmountUp();
        }
    }

    public class SubmitObjectiveData : ObjectiveData<SubmitObjective>
    {
        public EntryContent Dialogue { get; private set; }

        public SubmitObjectiveData(SubmitObjective objective) : base(objective)
        {
        }

        protected override void OnStart()
        {
            if (!IsComplete)
            {
                var talker = DialogueManager.Talkers[Model.NPCToSubmit.ID];
                talker.objectivesSubmitToThis.Add(this);
                OnStateChanged += talker.TryRemoveObjective;
                Dialogue = new EntryContent(ID, Model.Talker, Model.WordsWhenSubmit);
            }
        }
        protected override void OnSubmit()
        {
            var talker = DialogueManager.Talkers[Model.NPCToSubmit.ID];
            talker.objectivesSubmitToThis.Remove(this);
            OnStateChanged -= talker.TryRemoveObjective;
        }

        protected override void OnAbandon()
        {
            OnSubmit();
            DialogueManager.RemoveData(Dialogue);
        }

        public void UpdateSubmitState(int amount = 1)
        {
            UpdateAmountUp(amount);
        }
    }

    public class TriggerObjectiveData : ObjectiveData<TriggerObjective>
    {
        public TriggerObjectiveData(TriggerObjective objective) : base(objective) { }

        protected override void OnStart()
        {
            TriggerManager.RegisterTriggerEvent(UpdateTriggerState);
            var state = TriggerManager.GetTriggerState(Model.TriggerName);
            if (Model.CheckStateAtAcpt && state != TriggerState.NotExist)
                TriggerManager.SetTrigger(Model.TriggerName, state == TriggerState.On);
        }
        protected override void OnSubmit()
        {
            TriggerManager.DeleteTriggerListner(UpdateTriggerState);
        }
        protected override void OnAbandon()
        {
            OnSubmit();
        }
        public void UpdateTriggerState(string name, bool state)
        {
            if (name != Model.TriggerName) return;
            if (state) UpdateAmountUp();
            else if (AllPrevComplete && !AnyNextOngoing) CurrentAmount--;
        }
    }
}