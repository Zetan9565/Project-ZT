using System;
using System.Linq;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using UI;

    [Serializable, Name("递归"), Width(100f)]
    [Description("返回到指定的前置句子结点。")]
    public sealed class RecursionSuffix : SuffixNode
    {
        [field: SerializeField, Min(2)]
        public int Depth { get; private set; } = 2;

        public override bool IsValid => Depth > 1;

        public RecursionSuffix() { }

        public RecursionSuffix(int depth) => Depth = depth;

        public DialogueNode FindRecursionPoint(EntryNode entry)
        {
            DialogueNode find = null;
            if (!entry) return find;
            int depth = 0;
            DialogueNode temp = this;
            while (temp && depth < Depth)
            {
                if (!Dialogue.Traverse(entry, n =>
                {
                    if (n.Options.Any(x => x.Next == temp))
                    {
                        temp = n;
                        if (temp is SentenceNode) depth++;
                        return true;
                    }
                    return false;
                })) temp = null;
                if (temp == entry) break;
            }
            if (depth == Depth || temp == entry) find = temp;
            return find;
        }

        public override bool IsManual() => true;
        public override void DoManual(DialogueWindow window)
        {
            window.CurrentEntryData[this].Access();
            window.ContinueWith(FindRecursionPoint(window.CurrentEntry));
        }

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueNode from, DialogueOption option)
        {
            return from is not EntryNode;
        }
#endif
    }
}