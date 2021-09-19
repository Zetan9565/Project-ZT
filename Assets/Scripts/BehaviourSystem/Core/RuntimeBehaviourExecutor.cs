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
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
                if (!string.IsNullOrEmpty(path)) Debug.LogError($"不能在预制件使用RuntimeBehaviourExecutor：{path}");
                enabled = false;
                return;
            }
            if (!behaviour) behaviour = BehaviourTree.GetRuntimeTree();
            isRuntimeMode = true;
        }
    }
}