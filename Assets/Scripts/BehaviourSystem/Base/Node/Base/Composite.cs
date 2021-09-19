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

        protected int currentChildIndex;
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
            currentChildIndex = 0;
            if (children.Count > 0)
            {
                currentChild = children[currentChildIndex];
                while (!currentChild.IsValid)
                {
                    currentChild = children[currentChildIndex++];
                }
            }
        }

        public virtual void OnConditionalAbort(int index)
        {
            if (abortType == AbortType.None)
            {
                if (children[index] is Composite)
                {
                    for (int i = index + 1; i < children.Count; i++)
                    {
                        children[i].Abort();
                    }
                    Composite parent = Owner.FindParent(this, out var childIndex) as Composite;
                    if (parent) parent.OnConditionalAbort(childIndex);
                }
            }
            if (abortType == AbortType.LowerPriority || abortType == AbortType.Both)
            {
                Composite parent = Owner.FindParent(this, out var childIndex) as Composite;
                if (parent) parent.OnConditionalAbort(childIndex);
            }
            if (abortType == AbortType.Self || abortType == AbortType.Both)
            {
                for (int i = index + 1; i < children.Count; i++)
                {
                    if (children[i] is Action action)
                        action.Abort();
                }
            }
            isStarted = true;
            currentChildIndex = index;
            currentChild = children[index];
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
                if (l._position.x < r._position.x) return -1;
                else if (l._position.x > r._position.x) return 1;
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
        [InspectorName("更低优先")]
        LowerPriority,
        [InspectorName("以上两种")]
        Both
    }
}