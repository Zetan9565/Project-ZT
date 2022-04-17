using System.Collections.Generic;

namespace ZetanStudio.BehaviourTree
{
    /// <summary>
    /// 父型结点：有子结点的结点
    /// </summary>
    public abstract class ParentNode : Node
    {
        public virtual List<Node> GetChildren() { return new List<Node>(); }

        public virtual Conditional CheckConditionalAbort()
        {
            foreach (var child in GetChildren())
            {
                if (child.IsDone)
                    if (child is Conditional conditional)
                    {
                        if (conditional.CheckConditionalAbort())
                            return conditional;
                    }
                    else if (child is ParentNode parent && (conditional = parent.CheckConditionalAbort()))
                    {
                        return conditional;
                    }
            }
            return null;
        }
    }
}