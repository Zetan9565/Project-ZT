using System;
using System.Linq;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Group("特殊"), Name("分支"), Width(60f)]
    [Description("以第一个满足条件的分支作为前一个结点的分支。")]
    public sealed class BranchNode : DialogueNode
    {
        public override bool IsValid => options.Length > 0 && options.All(x => x.IsMain);

        public DialogueNode GetBranch(DialogueData entryData)
        {
            foreach (var option in options)
            {
                if (option?.Next is ConditionNode condition && condition.Check(entryData))
                {
                    var temp = condition.Options[0]?.Next;
                    while (temp is ConditionNode)
                    {
                        temp = temp[0]?.Next;
                    }
                    return temp;
                }
            }
            return null;
        }
    }
}