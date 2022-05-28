using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree.Nodes
{
    /// <summary>
    /// 开始结点：每棵<see cref="BehaviourTree"/>的根结点，应该有且只能有一个
    /// </summary>
    [Description("开始结点：每棵行为树的根结点，应该有且只能有一个")]
    public sealed class Entry : ParentNode
    {
        [SerializeReference]
        private Node start;

        public override bool IsValid => start;

        protected override NodeStates OnUpdate()
        {
            if (!start) return NodeStates.Success;
            else return start.Evaluate();
        }

        public override List<Node> GetChildren()
        {
            List<Node> children = new List<Node>();
            if (start) children.Add(start);
            return children;
        }

#if UNITY_EDITOR
        public override void AddChild(Node child)
        {
            start = child;
        }

        public override void RemoveChild(Node child)
        {
            if (child == start) start = null;
        }

        public override Node Copy()
        {
            Entry entry = MemberwiseClone() as Entry;
            if (start) entry.start = start.Copy();
            return entry;
        }

        public void ConvertToRuntime()
        {
            name += "(R)";
            isRuntime = true;
        }
#endif
    }
}