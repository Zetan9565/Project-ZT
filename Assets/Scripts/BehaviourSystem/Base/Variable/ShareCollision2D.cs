using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class ShareCollision2D : SharedVariable<Collision2D>
    {
        public static implicit operator ShareCollision2D(Collision2D value)
        {
            return new ShareCollision2D() { value = value };
        }
    }
}