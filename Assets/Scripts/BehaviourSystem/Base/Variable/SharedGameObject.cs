using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedGameObject : SharedVariable<GameObject>
    {
        public static implicit operator SharedGameObject(GameObject value)
        {
            return new SharedGameObject() { value = value };
        }
    }
}