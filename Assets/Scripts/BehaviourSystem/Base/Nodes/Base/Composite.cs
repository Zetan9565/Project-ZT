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

        [SerializeField, DisplayName("中止类型")]
        protected AbortType abortType;
        public AbortType AbortType => abortType;

        public override bool IsValid => children.Count > 0;

        protected int currentIndex;
        protected Node currentChild;

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

        protected override void OnStart()
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Conditional conditional)
                {
                    conditional.parent = this;
                    conditional.childIndex = i;
                }
            }
            currentIndex = 0;
            if (children.Count > 0)
            {
                currentChild = children[currentIndex];
                while (!currentChild.IsValid)
                {
                    currentChild = children[currentIndex++];
                }
            }
        }

        public virtual void OnConditionalAbort(int index)
        {
            if (index >= 0 && index < children.Count && children[index] is Conditional conditional)
                if ((abortType == AbortType.Self || abortType == AbortType.Both) && (State == NodeStates.Running || State == NodeStates.Success) && !conditional.CheckCondition())
                    OnAbort();
                else if ((abortType == AbortType.LowerPriority || abortType == AbortType.Both) && State == NodeStates.Failure && conditional.CheckCondition())
                {
                    currentIndex = index;
                    currentChild = children[index];
                    isStarted = true;
                }
        }

        #region EDITOR方法
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
        #endregion
    }

    public enum AbortType
    {
        [InspectorName("无")]
        None,
        [InspectorName("自我")]
        Self,
        [InspectorName("低优先级")]
        LowerPriority,
        [InspectorName("两种")]
        Both
    }
}