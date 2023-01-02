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

    [CreateAssetMenu(fileName = "dialogue", menuName = "Zetan Studio/剧情/对话")]
    public sealed class Dialogue : ScriptableObject
    {
        public string ID => Entry?.ID ?? string.Empty;

        [SerializeReference]
        private DialogueNode[] nodes = { };
        public ReadOnlyCollection<DialogueNode> Nodes => new ReadOnlyCollection<DialogueNode>(nodes);

        public EntryNode Entry => nodes[0] as EntryNode;

        public bool Exitable => Traverse(Entry, n => n.ExitHere);

        public Dialogue() => nodes = new DialogueNode[] { new EntryNode() };

        public bool Reachable(DialogueNode node) => Reachable(Entry, node);
        public static bool Reachable(DialogueNode from, DialogueNode to)
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

        public static void Traverse(DialogueNode node, Action<DialogueNode> onAccess, bool normalOnly = false)
        {
            if (node)
            {
                if (!normalOnly || DialogueNode.IsNormal(node)) onAccess?.Invoke(node);
                foreach (var option in node.Options)
                {
                    Traverse(option.Next, onAccess, normalOnly);
                }
            }
        }

        ///<param name="onAccess">带中止条件的访问器，返回 <i>true</i> 时将中止遍历</param>
        /// <returns>是否在遍历时产生中止</returns>
        public static bool Traverse(DialogueNode node, Func<DialogueNode, bool> onAccess, bool normalOnly = false)
        {
            if (onAccess != null && node)
            {
                if (!normalOnly || DialogueNode.IsNormal(node))
                    if (onAccess(node)) return true;
                foreach (var option in node.Options)
                {
                    if (Traverse(option.Next, onAccess, normalOnly))
                        return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 用于在编辑器中记录操作退出点，不应在游戏逻辑中使用
        /// </summary>
        public ExitNode exit = new ExitNode();
        /// <summary>
        /// 用于在编辑器中备注本段对话的用途，不应在游戏逻辑中使用
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// 用于在编辑器中设置分组，不应在游戏逻辑中使用
        /// </summary>
        public List<DialogueGroupData> groups = new();

        public static class Editor
        {
            public static DialogueNode AddNode(Dialogue dialogue, Type type)
            {
                if (!typeof(DialogueNode).IsAssignableFrom(type)) return null;
                var node = Activator.CreateInstance(type) as DialogueNode;
                ArrayUtility.Add(ref dialogue.nodes, node);
                return node;
            }
            public static void AddNode(Dialogue dialogue, DialogueNode node)
            {
                ArrayUtility.Add(ref dialogue.nodes, node);
            }
            public static void RemoveNode(Dialogue dialogue, DialogueNode node)
            {
                ArrayUtility.Remove(ref dialogue.nodes, node);
            }

            public static string Preview(Dialogue dialogue)
            {
                if (!dialogue) return null;
                StringBuilder sb = new StringBuilder();
                foreach (var node in dialogue.nodes)
                {
                    if (node is SentenceNode sentence)
                    {
                        sb.Append(sentence.Preview());
                        sb.Append('\n');
                    }
                    else if (node is OtherDialogueNode other && other.Dialogue)
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