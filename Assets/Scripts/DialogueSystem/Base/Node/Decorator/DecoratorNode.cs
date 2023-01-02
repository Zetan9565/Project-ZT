using System;

namespace ZetanStudio.DialogueSystem
{
    using UI;

    [Serializable, Group("选项修饰器")]
    public abstract class DecoratorNode : DialogueNode, ISoloMainOption
    {
        public DecoratorNode() => options = new DialogueOption[] { new DialogueOption(true, null) };

        public override bool IsValid => true;

        public sealed override bool IsManual() => false;
        public sealed override void DoManual(DialogueWindow window) { }

        public abstract void Decorate(DialogueData data, ref string title);

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueNode from, DialogueOption option) => from is DecoratorNode || !option.IsMain;
#endif
    }
}