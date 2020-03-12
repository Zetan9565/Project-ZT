using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using System;
using UnityEditor;

public class BehaviourTreeGraphView : GraphView
{
    public BehaviourTree tree;

    public BehaviourTreeGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(CreateEntry());
    }
    public BehaviourTreeGraphView(BehaviourTree behaviourTree)
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        tree = behaviourTree;

        AddElement(CreateEntry());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var oPorts = new List<Port>();
        ports.ForEach(p =>
        {
            if (startPort != p && startPort.node != p.node)
                oPorts.Add(p);
        });
        return oPorts;
    }

    private Port GetPort(BehaviourTreeGraphNode node, Direction direction, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
    }

    private BehaviourTreeGraphNode CreateEntry()
    {
        var node = new BehaviourTreeGraphNode()
        {
            title = "开始",
            GUID = Guid.NewGuid().ToString(),
            action = new UnityEngine.Events.UnityEvent(),
            entryPoint = true
        };

        var outputPort = GetPort(node, Direction.Output);
        outputPort.portName = "下个";
        node.outputContainer.Add(outputPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 400, 200));

        return node;
    }

    public void AddNode(string name, Vector2 position)
    {
        AddElement(CreateBehaviourTreeNode(name, position));
    }

    public BehaviourTreeGraphNode CreateBehaviourTreeNode(string name, Vector2 position)
    {
        var node = new BehaviourTreeGraphNode()
        {
            title = name,
            GUID = Guid.NewGuid().ToString(),
            action = new UnityEngine.Events.UnityEvent()
        };

        var inputPort = GetPort(node, Direction.Input);
        inputPort.name = "上一个";
        node.inputContainer.Add(inputPort);

        node.SetPosition(new Rect(position, new Vector2(400, 200)));

        var button = new Button(delegate { AddChoidPort(node); });
        button.text = "新出口";
        node.titleContainer.Add(button);

        node.RefreshExpandedState();
        node.RefreshPorts();
        return node;
    }

    private void AddChoidPort(BehaviourTreeGraphNode node)
    {
        var gPort = GetPort(node, Direction.Output);

        var outputPortCount = node.outputContainer.Query().ToList().Count;
        gPort.portName = $"出口{outputPortCount}";

        node.outputContainer.Add(gPort);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }
}