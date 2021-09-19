using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedVector2 : SharedVariable<Vector2>
    {
        public static implicit operator SharedVector2(Vector2 value)
        {
            return new SharedVector2() { value = value };
        }
    }
}