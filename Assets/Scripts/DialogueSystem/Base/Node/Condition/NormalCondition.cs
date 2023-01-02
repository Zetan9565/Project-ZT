using System;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using ConditionSystem;

    [Serializable, Name("按条件显示"), Width(246f)]
    [Description("满足条件时可进入从本结点开始的分支。")]
    public class NormalCondition : ConditionNode
    {
        [field: SerializeField]
        public ConditionGroup Condition { get; private set; } = new ConditionGroup();

        public override bool IsValid => Condition.IsValid;

        protected override bool CheckCondition(DialogueData entryData) => Condition.IsMeet();
    }
}