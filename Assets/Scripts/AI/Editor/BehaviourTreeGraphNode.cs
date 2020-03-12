using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEditor.Experimental.GraphView;

public class BehaviourTreeGraphNode : Node
{
    public string GUID;

    public UnityEvent action;

    public bool entryPoint = false;
}
