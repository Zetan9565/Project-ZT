using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 开始结点：每棵行为树的根结点，应该有且只能有一个
    /// </summary>
    [NodeDescription("开始结点：每棵行为树的根结点，应该有且只能有一个")]
    public sealed class Entry : Node
    {
        [SerializeField]
        private Node start;

        public override bool IsValid => start;

        protected override NodeStates OnUpdate()
        {
            if (!start) return NodeStates.Success;
            else return start.Evaluate();
        }

        public override Node GetInstance()
        {
            Entry entry = GetInstance<Entry>();
            entry.start = start.GetInstance();
            entry.IsInstance = true;
            return entry;
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

        public override Node ConvertToLocal()
        {
            Entry entry = ConvertToLocal<Entry>();
            entry.start = start.ConvertToLocal();
            return entry;
        }
#endif
    }
}