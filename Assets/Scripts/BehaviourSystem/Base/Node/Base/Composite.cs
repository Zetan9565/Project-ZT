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
        protected bool abort;

        public Composite() { children = new List<Node>(); }

        public override Node GetInstance()
        {
            Composite composite = GetInstance<Composite>();
            if (children != null) composite.children = children.ConvertAll(c => c.GetInstance());
            return composite;
        }

        public override List<Node> GetChildren()
        {
            if (children == null) children = new List<Node>();
            return children;
        }

        protected override void OnStart()
        {
            abort = false;
            currentChildIndex = 0;
            if (children.Count > 0)
            {
                currentChild = children[currentChildIndex];
                while (!currentChild.IsValid)
                {
                    currentChild = children[currentChildIndex++];
                }
            }
            Owner.OnCompositeEvaluate(this);
        }

        protected virtual void OnConditionalAbort(int index, bool lowerAbort)
        {
            if (lowerAbort && abortType == AbortType.None)
            {
                if (children[index] is Composite)
                {
                    for (int i = index + 1; i < children.Count; i++)
                    {
                        children[i].Abort();
                    }
                    Composite parent = Owner.FindParent(this, out var childIndex) as Composite;
                    if (parent) parent.OnConditionalAbort(childIndex, lowerAbort);
                    AcceptAbort();
                }
            }
            else if (lowerAbort && (abortType == AbortType.LowerPriority || abortType == AbortType.Both))
            {
                Composite parent = Owner.FindParent(this, out var childIndex) as Composite;
                if (parent) parent.OnConditionalAbort(childIndex, lowerAbort);
                AcceptAbort();
            }
            else if (!lowerAbort && (abortType == AbortType.Self || abortType == AbortType.Both))
            {
                for (int i = index + 1; i < children.Count; i++)
                {
                    if (children[i] is Action action)
                        action.Abort();
                    else children[i].Inactivate();
                }
                AcceptAbort();
            }

            void AcceptAbort()
            {
                abort = true;
                isStarted = true;
                currentChildIndex = index;
                currentChild = children[index];
            }
        }

        public bool CheckConditionalAbort()
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Conditional conditional && conditional.IsDone)
                {
                    bool lowerAbort = conditional.CheckCondition();
                    if (State == NodeStates.Failure && (abortType == AbortType.LowerPriority || abortType == AbortType.Both) && lowerAbort
                        || (State == NodeStates.Success || State == NodeStates.Running) && (abortType == AbortType.Self || abortType == AbortType.Both) && !lowerAbort)
                    {
                        OnConditionalAbort(i, lowerAbort);
                        return true;
                    }
                }
            }
            return false;
        }

        protected void InactivateFrom(int childIndex)
        {
            if (abort) return;
            for (int j = childIndex + 1; j < children.Count; j++)
            {
                children[j].Inactivate();
            }
        }

        #region EDITOR方法
#if UNITY_EDITOR
        public override void AddChild(Node child)
        {
            children.Add(child);
            SortByPosition();
        }

        public override void RemoveChild(Node child)
        {
            children.Remove(child);
            SortByPosition();
        }

        public override Node ConvertToLocal()
        {
            Composite composite = ConvertToLocal<Composite>();
            composite.children = children.ConvertAll(c => c.ConvertToLocal());
            return composite;
        }

        public override Node Copy()
        {
            Composite composite = Instantiate(this);
            if (children != null) composite.children = children.ConvertAll(c => c.Copy());
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