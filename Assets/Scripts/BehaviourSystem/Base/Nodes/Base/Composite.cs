using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 复合结点：可以有多个子结点
    /// </summary>
    public abstract class Composite : Node
    {
        [SerializeField]
        protected List<Node> children;

        public override bool IsValid => children.Count > 0;

        public Composite() { children = new List<Node>(); }

        public override Node GetInstance()
        {
            Composite composite = GetInstance<Composite>();
            composite.children = children.ConvertAll(c => c.GetInstance());
            return composite;
        }

        public override List<Node> GetChildren()
        {
            if (children == null) children = new List<Node>();
            return children;
        }

#if UNITY_EDITOR
        public override void AddChild(Node child)
        {
            children.Add(child);
        }

        public override void RemoveChild(Node child)
        {
            children.Remove(child);
        }

        public override Node ConvertToLocal()
        {
            Composite composite = ConvertToLocal<Composite>();
            composite.children = children.ConvertAll(c => c.ConvertToLocal());
            return composite;
        }

        public void SortByPosition()
        {
            children.Sort((l, r) =>
            {
                if (l.position.x < r.position.x) return -1;
                else if (l.position.x > r.position.x) return 1;
                else return 0;
            });
        }
#endif
    }
}