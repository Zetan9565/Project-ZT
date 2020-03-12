using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "behaviour tree", menuName = "ZetanStudio/AI/行为树")]
[System.Serializable]
public class BehaviourTree : ScriptableObject
{
    public List<BehaviourTreeNode> nodes = new List<BehaviourTreeNode>();
}

[System.Serializable]
public abstract class BehaviourTreeNode
{
    public BehaviourTreeNode parent;
    public List<BehaviourTreeNode> childrens = new List<BehaviourTreeNode>();

    public virtual void OnBegin()
    {

    }

    public virtual NodeStatus OnUpate()
    {
        return NodeStatus.Running;
    }

    public virtual void OnEnd()
    {
        
    }
}
[System.Serializable]
public class ConditionNode: BehaviourTreeNode
{
    
}

public enum NodeStatus
{
    Success,
    Failure,
    Running
}