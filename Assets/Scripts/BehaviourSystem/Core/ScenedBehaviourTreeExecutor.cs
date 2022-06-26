using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class ScenedBehaviourTreeExecutor : BehaviourTreeExecutor
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
                if (!string.IsNullOrEmpty(path)) Debug.LogError($"不能在预制件使用{GetType().Name}：{path}");
                enabled = false;
                return;
            }
            if (!behaviour) behaviour = BehaviourTree.GetSceneOnlyTree();
        }
#endif
    }
}