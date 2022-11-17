using System;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace ZetanStudio.DialogueSystem
{
    using ConditionSystem;

    [CreateAssetMenu(fileName = "dialogue", menuName = "Zetan Studio/����/�Ի�")]
    public sealed class Dialogue : ScriptableObject
    {
        public string ID => Entry?.ID ?? string.Empty;

        [SerializeReference]
        private DialogueContent[] contents = { };
        public ReadOnlyCollection<DialogueContent> Contents => new ReadOnlyCollection<DialogueContent>(contents);

        public EntryContent Entry => contents[0] as EntryContent;

        public bool Exitable => Traverse(Entry, n => n.ExitHere);

        public Dialogue() => contents = new DialogueContent[] { new EntryContent() };

        public bool Reachable(DialogueContent content) => Reachable(Entry, content);
        public static bool Reachable(DialogueContent from, DialogueContent to)
        {
            if (!to) return false;
            bool reachable = false;
            Traverse(from, c =>
            {
                reachable = c == to;
                return reachable;
            });
            return reachable;
        }

        public static void Traverse(DialogueContent content, Action<DialogueContent> onAccess, bool normalOnly = false)
        {
            if (content)
            {
                if (!normalOnly || DialogueContent.IsNormal(content)) onAccess?.Invoke(content);
                foreach (var option in content.Options)
                {
                    Traverse(option.Content, onAccess, normalOnly);
                }
            }
        }

        ///<param name="onAccess">����ֹ�����ķ����������� <i>true</i> ʱ����ֹ����</param>
        /// <returns>�Ƿ��ڱ���ʱ������ֹ</returns>
        public static bool Traverse(DialogueContent content, Func<DialogueContent, bool> onAccess, bool normalOnly = false)
        {
            if (onAccess != null && content)
            {
                if (!normalOnly || DialogueContent.IsNormal(content))
                    if (onAccess(content)) return true;
                foreach (var option in content.Options)
                {
                    if (Traverse(option.Content, onAccess, normalOnly))
                        return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// �����ڱ༭���м�¼�����˳��㣬��Ӧ����Ϸ�߼���ʹ��
        /// </summary>
        public ExitContent exit = new ExitContent();
        /// <summary>
        /// �����ڱ༭���б�ע���ζԻ�����;����Ӧ����Ϸ�߼���ʹ��
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// �����ڱ༭�������÷��飬��Ӧ����Ϸ�߼���ʹ��
        /// </summary>
        public List<DialogueContentGroup> groups = new();

        public static class Editor
        {
            public static DialogueContent AddContent(Dialogue dialogue, Type type)
            {
                if (!typeof(DialogueContent).IsAssignableFrom(type)) return null;
                var content = Activator.CreateInstance(type) as DialogueContent;
                ArrayUtility.Add(ref dialogue.contents, content);
                return content;
            }
            public static void PasteContent(Dialogue dialogue, DialogueContent content)
            {
                ArrayUtility.Add(ref dialogue.contents, content);
            }
            public static void RemoveContent(Dialogue dialogue, DialogueContent content)
            {
                ArrayUtility.Remove(ref dialogue.contents, content);
            }

            public static string Preview(Dialogue dialogue)
            {
                if (!dialogue) return null;
                StringBuilder sb = new StringBuilder();
                foreach (var content in dialogue.contents)
                {
                    if (content is TextContent text)
                    {
                        sb.Append(text.Preview());
                        sb.Append('\n');
                    }
                    else if (content is OtherDialogueContent other && other.Dialogue)
                    {
                        sb.Append(other.Dialogue.Entry.Preview());
                        sb.Append('\n');
                    }
                }
                if (sb.Length > 0) sb.Remove(sb.Length - 1, 1); ;
                return sb.ToString();
            }
        }
#endif
    }

    [Serializable]
    public class AffectiveDialogue
    {
        [SerializeField]
        private int lowerBound = 10;
        public int LowerBound => lowerBound;

        [SerializeField]
        private int upperBound = 20;
        public int UpperBound => upperBound;

        [SerializeField]
        private Dialogue dialogue;
        public Dialogue Dialogue => dialogue;
    }

    [Serializable]
    public class ConditionDialogue
    {
        [SerializeField]
        private ConditionGroup condition;
        public ConditionGroup Condition => condition;

        [SerializeField]
        private Dialogue dialogue;
        public Dialogue Dialogue => dialogue;

        public bool IsValid => dialogue && condition.IsValid;
    }
}