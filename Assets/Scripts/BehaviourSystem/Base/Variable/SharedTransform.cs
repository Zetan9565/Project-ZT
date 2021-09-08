using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedTransform : SharedVariable<Transform>
    {
        public static implicit operator SharedTransform(Transform value)
        {
            return new SharedTransform() { value = value };
        }
    }
}