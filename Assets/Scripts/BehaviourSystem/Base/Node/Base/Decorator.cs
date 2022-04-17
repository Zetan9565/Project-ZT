using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 修饰结点：只有一个子结点的父型结点
    /// </summary>
    public abstract class Decorator : ParentNode
    {
        [SerializeReference]
        protected Node child;

        public override bool IsValid => child;

        public override List<Node> GetChildren()
        {
            List<Node> children = new List<Node>();
            if (child) children.Add(child);
            return children;
        }

#if UNITY_EDITOR
        public override void AddChild(Node child)
        {
            this.child = child;
        }

        public override void RemoveChild(Node child)
        {
            if (child == this.child) this.child = null;
        }

        public override Node Copy()
        {
            Decorator decorator = MemberwiseClone() as Decorator;
            if (child) decorator.child = child.Copy();
            return decorator;
        }
#endif
    }
}