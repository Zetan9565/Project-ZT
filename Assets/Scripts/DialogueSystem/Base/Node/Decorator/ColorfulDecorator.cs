using System;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("染色"), Width(50f)]
    [Description("以指定颜色显示选项标题。")]
    public sealed class ColorfulDecorator : DecoratorNode
    {
        [field: SerializeField]
        public Color Color { get; private set; } = Color.black;

        public override void Decorate(DialogueData data, ref string title)
        {
            title = Utility.ColorText(title, Color);
        }
    }
}