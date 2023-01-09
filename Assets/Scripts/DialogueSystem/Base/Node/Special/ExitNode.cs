#if UNITY_EDITOR
using System;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    /// <summary>
    /// 用于在编辑器中设置退出点，不应在游戏逻辑中使用
    /// </summary>
    [Serializable, Name("结束"), Width(100f)]
    [Description("用于标识对话的结束位置。")]
    public sealed class ExitNode : SuffixNode
    {
        public ExitNode() => _position = new Vector2(360, 0);

        public override bool IsValid => true;

        public override bool CanLinkFrom(DialogueNode from, DialogueOption option) => option.IsMain && from.Options.Count == 1 && from is not DecoratorNode && from is not BlockerNode;
    }
}
#endif