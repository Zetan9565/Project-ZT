using System;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    using UI;

    [Serializable, Group("特殊"), Name("其它对话")]
    [Description("开始一段新的对话。")]
    public sealed class OtherDialogueNode : DialogueNode
    {
        [field: SerializeField]
        public Dialogue Dialogue { get; private set; }

        public override bool IsValid => Dialogue;

        public override bool OnEnter() => Dialogue;

        public override bool IsManual() => true;

        public override void DoManual(DialogueWindow window)
        {
            window.CurrentEntryData[this].Access();
            window.PushContinuance(this);
            window.StartWith(Dialogue);
        }
    }
}