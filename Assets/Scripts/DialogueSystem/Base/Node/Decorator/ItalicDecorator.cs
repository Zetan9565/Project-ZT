using System;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("斜体")]
    [Description("以斜体显示选项标题")]
    public sealed class ItalicDecorator : DecoratorNode
    {
        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.ItalicText(title);
        }
    }
}