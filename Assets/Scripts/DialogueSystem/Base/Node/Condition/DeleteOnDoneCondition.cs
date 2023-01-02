using System;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("完成前显示"), Width(50f)]
    [Description("从本结点开始的分支在完成前可进入。")]
    public class DeleteOnDoneCondition : ConditionNode
    {
        public override bool IsValid => true;

        protected override bool CheckCondition(DialogueData entryData) => !entryData[ID].IsDone;
    }
}