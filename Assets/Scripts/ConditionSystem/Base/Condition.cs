using System;
using System.Reflection;
using UnityEngine;
using ZetanStudio.ItemSystem;

namespace ZetanStudio.ConditionSystem
{
    [Serializable]
    public abstract class Condition
    {
        public abstract bool IsValid { get; }

        public abstract bool IsMeet();

        public static implicit operator bool(Condition self)
        {
            return self != null;
        }

        protected sealed class GroupAttribute : Attribute
        {
            public readonly string group;

            public GroupAttribute(string group)
            {
                this.group = group;
            }
        }
        protected sealed class NameAttribute : Attribute
        {
            public readonly string name;

            public NameAttribute(string name)
            {
                this.name = name;
            }
        }

        public static string GetGroup(Type type)
        {
            if (type.GetCustomAttribute<GroupAttribute>() is GroupAttribute attr) return attr.group;
            return string.Empty;
        }
        public static string GetName(Type type)
        {
            if (type.GetCustomAttribute<NameAttribute>() is NameAttribute attr) return attr.name;
            return string.Empty;
        }
    }

    #region 道具相关
    [Serializable]
    public abstract class ItemCondition : Condition
    {
        [field: SerializeField]
        public Item Item { get; private set; }

        public override bool IsValid => Item;
    }
    [Serializable, Name("持有道具"), Group("道具相关")]
    public sealed class HaveItem : ItemCondition
    {
        public override bool IsMeet()
        {
            return BackpackManager.Instance.HasItem(Item);
        }
    }
    [Serializable]
    public abstract class ItemAmountCondition : ItemCondition
    {
        [field: SerializeField]
        public int Amount { get; private set; }
    }
    [Serializable, Name("道具持有数量等于"), Group("道具相关")]
    public sealed class ItemAmountEquals : ItemAmountCondition
    {
        public override bool IsMeet()
        {
            return BackpackManager.Instance.GetAmount(Item) == Amount;
        }

        public override bool IsValid => base.IsValid && Amount >= 0;
    }
    [Serializable, Name("道具持有数量多于"), Group("道具相关")]
    public sealed class ItemAmountLargeThan : ItemAmountCondition
    {
        public override bool IsMeet()
        {
            return BackpackManager.Instance.GetAmount(Item) > Amount;
        }
    }
    [Serializable, Name("道具持有数量少于"), Group("道具相关")]
    public sealed class ItemAmountLessThan : ItemAmountCondition
    {
        public override bool IsMeet()
        {
            return BackpackManager.Instance.GetAmount(Item) < Amount;
        }

        public override bool IsValid => base.IsValid && Amount > 0;
    }
    #endregion

    #region 等级相关
    [Serializable]
    public abstract class LevelCondition : Condition
    {
        [field: SerializeField, Min(1)]
        public int Level { get; private set; } = 1;

        public override bool IsValid => Level > 0;
    }
    [Serializable, Name("等级等于"), Group("等级相关")]
    public sealed class LevelEquals : LevelCondition
    {
        public override bool IsMeet()
        {
            return PlayerManager.Instance.PlayerInfo.level == Level;
        }
    }
    [Serializable, Name("等级大于"), Group("等级相关")]
    public sealed class LevelLargeThan : LevelCondition
    {
        public override bool IsMeet()
        {
            return PlayerManager.Instance.PlayerInfo.level > Level;
        }
    }
    [Serializable, Name("等级小于"), Group("等级相关")]
    public sealed class LevelLessThan : LevelCondition
    {
        public override bool IsMeet()
        {
            return PlayerManager.Instance.PlayerInfo.level < Level;
        }
    }
    #endregion

    #region 任务相关
    [Serializable]
    public abstract class QuestCondition : Condition
    {
        [field: SerializeField, ObjectSelector("Title")]
        public Quest Quest { get; private set; }

        public override bool IsValid => Quest;
    }
    [Serializable, Name("完成任务"), Group("任务相关")]
    public sealed class QuestCompleted : QuestCondition
    {
        public override bool IsMeet()
        {
            return QuestManager.HasCompleteQuest(Quest);
        }
    }
    [Serializable, Name("完成任务后"), Group("任务相关")]
    public sealed class AfterQuestCompleted : QuestCondition
    {
        [field: SerializeField]
        public int Days { get; private set; }

        public override bool IsMeet()
        {
            return QuestManager.HasCompleteQuest(Quest) && TimeManager.Instance.Days - QuestManager.FindQuest(Quest.ID).latestHandleDays >= Days;
        }
    }
    [Serializable, Name("接取任务"), Group("任务相关")]
    public sealed class QuestAccepted : QuestCondition
    {
        public override bool IsMeet()
        {
            return QuestManager.HasOngoingQuest(Quest);
        }
    }
    [Serializable, Name("接取任务后"), Group("任务相关")]
    public sealed class AfterQuestAccepted : QuestCondition
    {
        [field: SerializeField, Min(1)]
        public int Days { get; private set; } = 1;

        public override bool IsMeet()
        {
            return QuestManager.HasOngoingQuest(Quest) && TimeManager.Instance.Days - QuestManager.FindQuest(Quest.ID).latestHandleDays >= Days;
        }
    }
    #endregion

    [Serializable, Name("触发器状态")]
    public sealed class TriggerIsState : Condition
    {
        [field: SerializeField]
        public string TriggerName { get; private set; }

        [field: SerializeField]
        public bool State { get; private set; }

        public override bool IsValid => !string.IsNullOrEmpty(TriggerName);

        public override bool IsMeet()
        {
            return TriggerManager.GetTriggerState(TriggerName) == TriggerState.On;
        }
    }
}