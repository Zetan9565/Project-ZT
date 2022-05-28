using ZetanStudio.Item;

public abstract class ObjectiveData
{
    public Objective Model { get; }

    public string DisplayName => MiscFuntion.HandlingKeyWords(parent.Tr(Model.DisplayName));

    public T GetInfo<T>() where T : Objective
    {
        return Model as T;
    }

    public ObjectiveData(Objective objective)
    {
        Model = objective;
    }

    private int currentAmount;
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
            if (value < Model.Amount && value >= 0)
                currentAmount = value;
            else if (value < 0)
            {
                currentAmount = 0;
            }
            else currentAmount = Model.Amount;
            if (befAmount != currentAmount) OnStateChangeEvent?.Invoke(this, befCmplt);
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

    public System.Action<ObjectiveData, bool> OnStateChangeEvent;

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

    public void UpdateCollectAmount(Item model, int oldAmount, int newAmount)
    {
        if (IsComplete) return;
        if (model == Model.ItemToCollect)
        {
            if (oldAmount < newAmount)//获得道具
            {
                if (!Model.InOrder || AllPrevComplete) CurrentAmount = newAmount - (!Model.CheckBagAtStart ? amountWhenStart : 0);
                else UnityEngine.Debug.LogWarning($"任务 [{parent.Model.ID}] 的目标 [{Model.DisplayName}] 发生置数错误");
            }
            else if (Model.LoseItemAtSbmt)//失去道具，且在提交任务时要上交此目标收集的道具
            {
                if (AllPrevComplete && !AnyNextOngoing)//前置目标都完成且没有后置目标在进行时，才允许更新
                    CurrentAmount = newAmount;
                else UnityEngine.Debug.LogWarning($"任务 [{parent.Model.ID}] 的目标 [{Model.DisplayName}] 发生置数错误");
            }
        }
    }
}

public class KillObjectiveData : ObjectiveData<KillObjective>
{
    public KillObjectiveData(KillObjective objective) : base(objective) { }

    public void UpdateKillAmount()
    {
        UpdateAmountUp();
    }
}

public class TalkObjectiveData : ObjectiveData<TalkObjective>
{
    public TalkObjectiveData(TalkObjective objective) : base(objective) { }

    public void UpdateTalkState()
    {
        UpdateAmountUp();
    }
}

public class MoveObjectiveData : ObjectiveData<MoveObjective>
{
    public CheckPointData targetPoint;

    public MoveObjectiveData(MoveObjective objective) : base(objective) { }

    public void UpdateMoveState(CheckPointInformation point)
    {
        if (point == Model.AuxiliaryPos) UpdateAmountUp();
    }
}

public class SubmitObjectiveData : ObjectiveData<SubmitObjective>
{
    public SubmitObjectiveData(SubmitObjective objective) : base(objective) { }

    public void UpdateSubmitState(int amount = 1)
    {
        UpdateAmountUp(amount);
    }
}

public class TriggerObjectiveData : ObjectiveData<TriggerObjective>
{
    public TriggerObjectiveData(TriggerObjective objective) : base(objective) { }

    public void UpdateTriggerState(string name, bool state)
    {
        if (name != Model.TriggerName) return;
        if (state) UpdateAmountUp();
        else if (AllPrevComplete && !AnyNextOngoing) CurrentAmount--;
    }
}