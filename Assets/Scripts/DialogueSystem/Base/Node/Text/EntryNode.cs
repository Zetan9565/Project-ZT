using System;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Name("开始")]
    [Description("对话的根结点。")]
    public sealed class EntryNode : SentenceNode
    {
        public EntryNode()
        {
            ID = "DLG" + Guid.NewGuid().ToString("N");
            options = new DialogueOption[] { new DialogueOption(true, null) };
            ExitHere = true;
        }

        public EntryNode(string talker, string content) : this()
        {
            Talker = talker;
            Text = content;
        }

        public EntryNode(string id, string talker, string content) : this(talker, content)
        {
            ID = id;
        }

#if UNITY_EDITOR
        public override bool CanLinkFrom(DialogueNode from, DialogueOption option) => false;
#endif
    }
}