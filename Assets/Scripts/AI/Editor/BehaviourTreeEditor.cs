using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BehaviourTree))]
public class BehaviourTreeEditor : Editor
{
    BehaviourTree behaviourTree;

    private void OnEnable()
    {
        behaviourTree = target as BehaviourTree;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("打开编辑器"))
        {
            BehaviourTreeGraphWindow.OpenBehaviourTreeWindow(behaviourTree);
        }
    }
}