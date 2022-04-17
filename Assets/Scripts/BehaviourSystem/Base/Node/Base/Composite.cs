using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 复合结点：可以有多个子结点的父型结点
    /// </summary>
    public abstract class Composite : ParentNode
    {
        [SerializeReference]
        protected List<Node> children;

        [SerializeField, DisplayName("中止类型")]
        protected AbortType abortType;
        public AbortType AbortType => abortType;

        public override bool IsValid => children.Count > 0;

        protected int currentChildIndex;
        protected Node currentChild;
        protected bool abort;

        public Composite() { children = new List<Node>(); }

        public override List<Node> GetChildren()
        {
            if (children == null) children = new List<Node>();
            return children;
        }

        protected override void OnStart()
        {
            abort = false;
            currentChildIndex = 0;
            HandlingCurrentChild();
        }

        protected void HandlingCurrentChild()
        {
            currentChild = null;
            if (currentChildIndex >= 0 && currentChildIndex < children.Count)
                currentChild = children[currentChildIndex];
            while ((!currentChild || !currentChild.IsValid) && currentChildIndex < children.Count)
            {
                currentChild = null;
                currentChildIndex++;
                if (currentChildIndex >= 0 && currentChildIndex < children.Count)
                    currentChild = children[currentChildIndex];
            }
        }

        protected void InactivateFrom(int childIndex)
        {
            if (abort) return;
            for (int j = childIndex + 1; j < children.Count; j++)
            {
                children[j].Inactivate();
            }
        }

        public bool ReciveConditionalAbort(Node node)
        {
            bool lowerAbort = node is Composite composite && composite.abortType == AbortType.LowerPriority;
            if (abortType != AbortType.None || lowerAbort)
            {
                int index = FindBranchIndex(node);
                if (index >= 0)
                {
                    if (abortType == AbortType.Self || abortType == AbortType.Both || lowerAbort)
                    {
                        for (int i = index + 1; i < children.Count; i++)
                        {
                            if (!lowerAbort)
                                if (children[i] is Action action) action.Abort();
                                else children[i].Inactivate();
                            else children[i].Abort();
                        }
                        abort = true;
                        isStarted = true;
                        currentChildIndex = index;
                        HandlingCurrentChild();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 找包含某节点的分支下标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected int FindBranchIndex(Node node)
        {
            for (int i = 0; i < children.Count; i++)
            {
                bool find = false;
                BehaviourTree.Traverse(children[i], accesser);
                if (find) return i;

                bool accesser(Node child)
                {
                    if (child == node)
                    {
                        find = true;
                        return true;
                    }
                    return false;
                }
            }
            return -1;
        }

        public override Conditional CheckConditionalAbort()
        {
            if (abortType != AbortType.None) return base.CheckConditionalAbort();
            return null;
        }

        #region EDITOR方法
#if UNITY_EDITOR
        public override void AddChild(Node child)
        {
            children.Add(child);
            SortByPosition();
            currentChildIndex = children.IndexOf(currentChild);
        }

        public override void RemoveChild(Node child)
        {
            children.Remove(child);
            SortByPosition();
            if (currentChild == child) HandlingCurrentChild();
            currentChildIndex = children.IndexOf(currentChild);
        }

        public override Node Copy()
        {
            Composite composite = MemberwiseClone() as Composite;
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