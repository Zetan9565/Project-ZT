using UnityEngine;
using System.Collections;

public class BehaviourExecutor : MonoBehaviour
{
    public BehaviourTree tree;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!tree) return;
        tree.nodes.ForEach(n =>
        {
            switch (n.OnUpate())
            {
                case NodeStatus.Success:
                    break;
                case NodeStatus.Failure:
                    break;
                case NodeStatus.Running:
                    break;
                default:
                    break;
            }
        });
    }
}
