using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ZetanStudio.DialogueSystem
{
    [Serializable, Group("对话"), Name("句子")]
    [Description("一段文本，是最基本的对话单位。")]
    public class SentenceNode : DialogueNode, IEventNode
    {
        [field: SerializeField, HideInNode]
        public string Talker { get; protected set; }

        [field: SerializeField, TextArea, HideInNode]
        public string Text { get; protected set; }

#if ZTDS_ENABLE_PORTRAIT
        [field: SerializeField, SpriteSelector, HideInNode]
        public Sprite Portrait { get; protected set; }

        [field: SerializeField, HideInNode]
        public PortraitSide PortrSide { get; protected set; }
#endif

        [SerializeReference, PolymorphismList("GetName"), HideInNode]
        protected DialogueEvent[] events = { };
        public ReadOnlyCollection<DialogueEvent> Events => new ReadOnlyCollection<DialogueEvent>(events);

        [field: SerializeField, HideInNode]
        public AnimationCurve SpeakInterval { get; protected set; } = new AnimationCurve(new Keyframe(0, 0.02f), new Keyframe(1, 0.02f));

        public override bool IsValid => !string.IsNullOrEmpty(Talker) && !string.IsNullOrEmpty(Text);

#if UNITY_EDITOR
        public string Preview()
        {
            string result = string.Empty;
            string talker = $"[{(string.IsNullOrEmpty(Talker) ? "(未定义)" : Keyword.Editor.HandleKeywords(Talker))}]说：";
            talker = System.Text.RegularExpressions.Regex.Replace(talker, @"{\[NPC\]}", "(交互对象)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            talker = System.Text.RegularExpressions.Regex.Replace(talker, @"{\[PLAYER\]}", "(玩家)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result += talker;
            string text = $"{(string.IsNullOrEmpty(Text) ? "(无内容)" : Keyword.Editor.HandleKeywords(Text))}";
            text = System.Text.RegularExpressions.Regex.Replace(text, @"{\[NPC\]}", "[交互对象]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"{\[PLAYER\]}", "[玩家]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result += text;
            return Utility.RemoveTags(result);
        }
#endif

    }

#if ZTDS_ENABLE_PORTRAIT
    public enum PortraitSide
    {
        Left,
        Right
    }
#endif
}