using System;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("粗体")]
    [Description("以粗体显示选项标题。")]
    public sealed class BoldDecorator : DecoratorNode
    {
        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.BoldText(title);
        }
    }
}