using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class SharedTransformList : SharedVariable<List<Transform>>
    {
        public static implicit operator SharedTransformList(List<Transform> value)
        {
            return new SharedTransformList() { value = value };
        }
    }
}