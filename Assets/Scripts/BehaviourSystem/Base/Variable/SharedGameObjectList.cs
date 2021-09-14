using System.Collections.Generic;
using UnityEngine;

namespace ZetanStudio.BehaviourTree
{
    public class SharedGameObjectList : SharedVariable<List<GameObject>>
    {
        public static implicit operator SharedGameObjectList(List<GameObject> value)
        {
            return new SharedGameObjectList() { value = value };
        }
    }
}