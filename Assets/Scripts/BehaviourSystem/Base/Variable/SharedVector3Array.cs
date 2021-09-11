using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZetanStudio.BehaviourTree;


[System.Serializable]
public class SharedVector3Array : SharedVariable<Vector3[]>
{
    public static implicit operator SharedVector3Array(Vector3[] value)
    {
        return new SharedVector3Array() { value = value };
    }
}