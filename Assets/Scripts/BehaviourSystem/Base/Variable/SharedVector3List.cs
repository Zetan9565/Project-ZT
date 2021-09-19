using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    [System.Serializable]
    public class SharedVector3List : SharedVariable<List<Vector3>>
    {
        public static implicit operator SharedVector3List(List<Vector3> value)
        {
            return new SharedVector3List() { value = value };
        }
    }
}