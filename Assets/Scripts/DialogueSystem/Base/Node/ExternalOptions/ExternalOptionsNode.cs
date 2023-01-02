using System;
using System.Collections.ObjectModel;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Group("外置选项")]
    public abstract class ExternalOptionsNode : DialogueNode
    {
        public abstract ReadOnlyCollection<DialogueOption> GetOptions(DialogueData entryData, DialogueNode owner);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueNode from, DialogueOption option)
        {
            return from is not DecoratorNode and not ExternalOptionsNode && option.IsMain;
        }
#endif
    }
}