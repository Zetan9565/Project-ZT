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
        protected List<Node> children = new List<Node>();

        [SerializeField, DisplayName("中止类型")]
        protected AbortType abortType;
        public AbortType AbortType => abortType;
        public bool AbortSelf => abortType == AbortType.Self || abortType == AbortType.Both;
        public bool AbortLowerPriority => abortType == AbortType.LowerPriority || abortType == AbortType.Both;

        public override bool IsValid => children.Count > 0;

        protected int currentChildIndex;
        protected Node currentChild;
        protected bool abort;

        public sealed override List<Node> GetChildren()
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
        /// <summary>
        /// 接收ConditionalAbort
        /// </summary>
        /// <param name="conditional">发起ConditionalAbort的<see cref="Conditional"/></param>
        /// <returns>是否成功接收</returns>
        public bool ReciveConditionalAbort(Conditional conditional)
        {
            int index = FindBranchIndex(conditional);
            if (index >= 0)
            {
                if (AbortSelf)
                {
                    currentChild.Abort();
                    AbortFrom(index);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 接收LowerPriorityAbort
        /// </summary>
        /// <param name="composite">发起LowerPriorityAbort的<see cref="Composite"/></param>
        /// <returns>是否成功接收</returns>
        public bool ReciveLowerPriorityAbort(Composite composite)
        {
            //评估已结束或当前评估结点比发起中止的Composite优先级高，则不会被中止
            if (State != NodeStates.Running || currentChild.ComparePriority(composite)) return false;
            int index = FindBranchIndex(composite);
            if (index >= 0)
            {
                currentChild.Abort();
                AbortFrom(index);
                return true;
            }
            return false;
        }
        private void AbortFrom(int index)
        {
            abort = true;
            isStarted = true;
            currentChildIndex = index;
            HandlingCurrentChild();
        }

        /// <summary>
        /// 找包含某节点的分支下标
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int FindBranchIndex(Node node)
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

        public sealed override Conditional CheckConditionalAbort()
        {
            if (NeedReevaluate()) return base.CheckConditionalAbort();
            return null;
        }
        public bool NeedReevaluate()
        {
            return IsDone && AbortLowerPriority || State == NodeStates.Running && AbortSelf;
        }

        #region EDITOR方法
#if UNITY_EDITOR
        public sealed override void AddChild(Node child)
        {
            children.Add(child);
            SortByPosition();
            currentChildIndex = children.IndexOf(currentChild);
        }

        public sealed override void RemoveChild(Node child)
        {
            children.Remove(child);
            SortByPosition();
            if (currentChild == child) HandlingCurrentChild();
            currentChildIndex = children.IndexOf(currentChild);
        }

        public sealed override Node Copy()
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
        Self,//如果正在执行的Action在这个Composite分支内，则可以中止
        [InspectorName("更低优先")]
        LowerPriority,//如果正在执行的Action在这个分支外优先级更低的位置，则可以中止
        [InspectorName("以上两种")]
        Both//既能中止此分支内部正在执行的Action又可以中止分支外部优先级更低的分支中的Action
    }
}