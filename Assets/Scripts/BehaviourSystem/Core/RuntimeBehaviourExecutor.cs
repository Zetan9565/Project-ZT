using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class RuntimeBehaviourExecutor : BehaviourExecutor
    {
        public override void SetBehaviour(BehaviourTree tree, bool executeImmediate)
        {
            if (!tree.IsRuntime)
            {
                Debug.LogWarning("行为树类型不匹配");
                return;
            }
            base.SetBehaviour(tree, executeImmediate);
        }

        private void OnValidate()
        {
            if (!behaviour) behaviour = BehaviourTree.GetRuntimeTree();
            isRuntimeMode = true;
        }
    }
}